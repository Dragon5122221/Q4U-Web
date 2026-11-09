# Firebase setup (per-record Firestore storage)

This guide goes with `Services/FirestoreCollectionRepository.cs`. It replaces the original single-document Firestore adapter; everything else in the application is unchanged. The original `docs/FIREBASE.md` is kept as written. Its "Data layout and scaling" section describes the old one-document-per-organisation layout, which this version no longer uses.

This app uses three Firebase products:

| Product | Used for |
|---|---|
| **Authentication** (Email/Password) | Signing in on the website and, later, the mobile app |
| **Cloud Firestore** (Native mode) | All records: vehicles, service requests, document metadata, quotes, notifications, audit history |
| **Cloud Storage** | The uploaded PDF/PNG/JPEG files themselves (private) |

```
Browser / mobile app ──sign in──▶ Firebase Authentication
        │ ID token
        ▼
Q4U server (ASP.NET Core) ──verifies token, reads memberships/{uid}──▶ Firestore
        │ business rules (WorkspaceService)
        ├──────────────▶ Firestore   (records)
        └──────────────▶ Cloud Storage (files)
```

Clients never read or write Firestore or Storage directly. `firebase/firestore.rules` and `firebase/storage.rules` deny all client access; the server uses the Firebase Admin SDK, which bypasses rules. That keeps every business rule (quotes, status changes, who can see whose documents) in one place, shared by the website and the mobile API.

---

## 1. Data model

One Firestore document per record. Field-level schemas (JSON Schema 2020-12, with examples) are in [`firestore-schema.json`](firestore-schema.json) for the Data Schema section of the report.

| Path | Holds | Written by |
|---|---|---|
| `memberships/{uid}` | `active`, `tenantId`, `role` for each Firebase user | `tools/provision` (users can never set their own role or tenant) |
| `tenants/{tenantId}` | `organisation`, `version`, `schemaVersion` | Provision tool, app |
| `tenants/{tenantId}/vehicles/{id}` | Registration, description, licence expiry, driver, branch | App |
| `tenants/{tenantId}/requests/{id}` | Service, status, quote (`amountCents`), payment, fulfilment | App |
| `tenants/{tenantId}/documents/{id}` | File metadata and review outcome; `storageKey` points at the Storage object | App |
| `tenants/{tenantId}/members/{id}` | Invitation drafts | App |
| `tenants/{tenantId}/users/{uid}` | Profile, contact preferences, `version` | App |
| `tenants/{tenantId}/users/{uid}/notices/{id}` | In-app notifications for that person | App |
| `tenants/{tenantId}/audit/{id}` | Append-only: who did what, when | App |
| Storage: `tenants/{tenantId}/{random-guid}` | The uploaded file | App |

**Tenant** means an organisation. The Q4U shop is one tenant (`q4u`); its staff (Administrator) and its walk-in/online customers (Customer) are members of it. Customers only ever see records where `ownerUid` is their own UID; this is enforced in `WorkspaceService` and also in the queries themselves.

Design choices worth explaining in the report:

- **Why Firestore (document store) rather than SQL:** Firebase was the client-approved platform; records are naturally hierarchical (organisation → vehicles/requests), queries are simple equality filters, and it scales to zero cost for a small shop.
- **Money as integer cents** (`amountCents: 128450` = R1 284,50) to avoid floating-point rounding.
- **Dates** (`expiry`) as `yyyy-MM-dd` strings; **instants** (`createdAt`) as Firestore Timestamps.
- **`tenantId` on every record**, so a future staff view can use collection-group queries across organisations.
- **Only automatic single-field indexes are used.** No composite indexes need deploying.

### Reads are scoped to the person

`FirestoreCollectionRepository` takes the signed-in person from the request and only loads what that role can see:

| Role | Loads |
|---|---|
| Customer | Their own vehicles, requests and documents (`where ownerUid == uid`), their profile and latest 200 notices |
| FleetOwner / FleetManager / Administrator | All of the organisation's vehicles, requests and documents. Invitation drafts for FleetOwner/Administrator; latest 200 audit entries for Administrator |
| Developer | Their own profile only |

### Concurrency (no lost updates)

Each page carries a `version`. Saving runs in a Firestore **transaction** that re-reads the version and refuses the save if anything changed (the user sees "Refresh and try again", HTTP 409). Only changed documents are written.

- Organisation roles are guarded by `tenants/{tenantId}.version`.
- Customers are guarded by `tenants/{tenantId}/users/{uid}.version`.
- Every save increments the tenant version **and** the version of every person whose records it touched. So when staff quote Ayesha's request, Ayesha's open page must refresh; when another customer saves, it doesn't.

---

## 2. Set up the Firebase project (one person, about 30 minutes)

Use one Google account as the project owner and add teammates under **Project settings → Users and permissions**.

### Step 1: Create the project
1. Go to <https://console.firebase.google.com> → **Create a project**. Name it e.g. `q4u-licensing`. Note the **project ID** shown under the name (e.g. `q4u-licensing-3f9a2`); you'll need it everywhere.
2. Google Analytics is optional; you can turn it off.

### Step 2: Upgrade to the Blaze plan and add a budget alert
Cloud Storage for Firebase requires the pay-as-you-go **Blaze** plan. The no-cost quotas still apply, so a student project normally costs R0.
1. Click **Upgrade** (bottom-left) → **Blaze** → link a billing account.
2. In Google Cloud Console → **Billing → Budgets & alerts**, create a budget of e.g. R100/month with email alerts at 50%, 90% and 100%. A budget alerts you; it does not cap spending.

### Step 3: Register the web app
1. **Project settings (gear) → General → Your apps → Web (`</>`)**. Nickname `Q4U web`. You don't need Firebase Hosting for this.
2. Copy `apiKey`, `authDomain` and `projectId` from the config snippet. The `apiKey` is a public identifier, not a secret.

### Step 4: Turn on Email/Password sign-in
1. **Build → Authentication → Get started → Sign-in method → Email/Password → Enable → Save.** Leave "Email link" off.
2. **Authentication → Settings → Authorised domains:** `localhost` is already there. Add your hosted domain when you deploy.
3. Optional: **Settings → Password policy**, minimum length 8, to match the registration form.

### Step 5: Create the Firestore database
1. **Build → Firestore Database → Create database.**
2. Edition: **Standard**. Mode: **Native**. Database ID: leave as `(default)`.
3. Location: **`africa-south1` (Johannesburg)**. It keeps customers' personal information in South Africa (helps with POPIA) and is close to the shop. **The location can't be changed later.**
4. Rules: **Start in production mode** (deny all). You'll deploy the project's rules in Step 7.

### Step 6: Create the Storage bucket
1. **Build → Storage → Get started.** Location: pick `africa-south1` if it's offered, for the same POPIA reason. Production mode.
2. Copy the bucket name shown at the top of the Files tab, e.g. `q4u-licensing-3f9a2.firebasestorage.app`, **without** `gs://`.

> Trade-off for the report: Storage's no-cost quota (5 GB) only applies to buckets in `us-central1`, `us-east1` or `us-west1`. A Johannesburg bucket costs about US$0.02–0.03 per GB-month. For a shop's scanned documents that is a few rand a month, in exchange for keeping identity documents in South Africa.

### Step 7: Download a server credential (local development)
The server needs an Admin credential to reach Firestore, Storage and Auth.

1. **Project settings → Service accounts → Firebase Admin SDK → Generate new private key.** A JSON file downloads.
2. Move it **outside the repository**, e.g. `C:\q4u-secrets\q4u-admin.json`. Anyone with this file has full access to the database. Never commit it, email it or put it in `wwwroot`. `.gitignore` blocks the default file name as a safety net.
3. Point the environment variable `GOOGLE_APPLICATION_CREDENTIALS` at it:
   - Windows (Command Prompt or PowerShell): `setx GOOGLE_APPLICATION_CREDENTIALS "C:\q4u-secrets\q4u-admin.json"`, then **close and reopen Visual Studio and your terminals**.
   - macOS/Linux: add `export GOOGLE_APPLICATION_CREDENTIALS="$HOME/q4u-secrets/q4u-admin.json"` to `~/.zshrc` or `~/.bashrc`.

Each teammate who runs the app in Firebase mode needs their own copy of the file (or their own key). Delete keys you no longer use from Google Cloud Console → IAM → Service accounts.

### Step 8: Deploy the security rules
Needs Node.js 22+.
```bash
npm install -g firebase-tools
firebase login
cd <repository folder>          # the folder that contains firebase.json
firebase use --add              # pick your project, alias: default (creates .firebaserc; commit it)
firebase deploy --only firestore:rules,storage
```
Check in the console: **Firestore → Rules** should show `allow read, write: if false;`.

### Step 9: Configure the app (User Secrets)
In Visual Studio: right-click the project → **Manage User Secrets** and paste (with your values):
```json
{
  "Q4U": { "Mode": "Firebase" },
  "Firebase": {
    "ProjectId": "q4u-licensing-3f9a2",
    "StorageBucket": "q4u-licensing-3f9a2.firebasestorage.app",
    "WebApiKey": "AIza...",
    "AuthDomain": "q4u-licensing-3f9a2.firebaseapp.com"
  }
}
```
Or from the project folder:
```bash
dotnet user-secrets set "Q4U:Mode" "Firebase"
dotnet user-secrets set "Firebase:ProjectId" "q4u-licensing-3f9a2"
dotnet user-secrets set "Firebase:StorageBucket" "q4u-licensing-3f9a2.firebasestorage.app"
dotnet user-secrets set "Firebase:WebApiKey" "AIza..."
dotnet user-secrets set "Firebase:AuthDomain" "q4u-licensing-3f9a2.firebaseapp.com"
```
User Secrets are stored in your Windows profile, not the repository, and only load in Development. To go back to the demo, set `Q4U:Mode` to `Demo`.

### Step 10: Create the shop tenant and the first staff account
```bash
cd tools/provision
npm install
node provision.mjs tenant q4u "Q4U Licensing Services" --project q4u-licensing-3f9a2
node provision.mjs user owner@example.com Administrator q4u --name "Shop Owner" --project q4u-licensing-3f9a2
```
For a new user without `--password`, the tool prints a one-time password-reset link. Open it (or send it to the person) to choose a password. For test accounts you can pass `--password "LongTestPassword1"` instead.

Other commands:
```bash
node provision.mjs list q4u --project <id>                       # who has access
node provision.mjs user someone@example.com FleetOwner fleet-abc --project <id>   # change role/tenant
node provision.mjs deactivate someone@example.com --project <id> # remove access and sign them out
```
Create test customers the same way, e.g. `node provision.mjs user customer1@example.com Customer q4u --name "Test Customer" --password "LongTestPassword1" --project <id>`. As in the original design, there is no public self-registration: every account is created by a trusted operator.

### Step 11: Run it and test end to end
Run the **https** profile (F5). Firebase sign-in cookies require HTTPS.

Acceptance checks. Screenshot the Firestore console as you go; it's useful evidence for the report.

1. Sign in as the Administrator. Firestore shows `tenants/q4u`.
2. Create a test customer with the provision tool (Step 10) and sign in as them in a private window. `memberships/{uid}` exists with `role: "Customer"`.
3. As the customer: add a vehicle (see `tenants/q4u/vehicles/…` with your `ownerUid`), upload a PDF (a `documents/…` record **and** a Storage object under `tenants/q4u/`), submit a licence renewal.
4. As the Administrator (Admin work queue): review the document, issue a quote. The customer gets a notice in `users/{uid}/notices`.
5. The request stays at **Quoted**. This is the original design: in Firebase mode, only a trusted payment integration may mark a request paid, and there is no Simulate payment button outside Demo mode. Ready/Completed and receipts can only be tested in Demo mode until a payment step is added.
6. Concurrency: open the same customer page in two tabs, save in tab 1, then save in tab 2. Tab 2 must say the page is out of date.
7. Isolation: create and sign in as a second customer. They must not see the first customer's vehicles, and `/Workspace/Download?id=<first customer's document id>` must return 404.
8. `node provision.mjs deactivate <customer email> --project <id>`. The customer's next page load signs them out.
9. Firestore → Rules → **Rules playground**: simulate an authenticated `get` on `/tenants/q4u`. It must be denied.

---

## 3. Firestore usage per action (for Running Costs)

Every query is billed at least 1 read, even if it returns nothing. Writes include the notice, audit entry and version counters that each save adds.

| Action | Reads | Writes |
|---|---|---|
| Any signed-in page (membership check) | 1 | 0 |
| Customer opens a workspace page | 1 + 1 (tenant) + 1 (profile) + own vehicles + own requests + own documents + own notices (min 1 each). Typical customer: **~15–20** | 0 |
| Customer saves something (add vehicle, submit request, cancel…) | Page load as above + the same read again + 1 (transaction) ≈ **35–40** | **~5** |
| Staff/fleet opens a page | 3 + **every** vehicle, request and document in the tenant + up to 200 notices + up to 200 audit | 0 |
| Staff action (quote, review, progress) | as above + 1 | ~6 |

No-cost daily quota: 50,000 reads, 20,000 writes, 20,000 deletes; then about US$0.06 per 100,000 reads and US$0.18 per 100,000 writes (check the current price list on firebase.google.com/pricing). Example: 25 customers a day × ~5 page loads × 20 reads ≈ 2,500 reads, plus staff pages. Staff page cost grows with the total number of records in the tenant. That is the main scaling item (section 6).

Cloud Storage: each upload and download is one operation; storage is billed per GB-month.

---

## 4. Mobile app (to build)

Register an Android app in the same project (**Project settings → Add app → Android**), use the Firebase Auth Android SDK to sign in, and call this server's API with `Authorization: Bearer <ID token>`. See [`API.md`](API.md). The app must not use the Firestore or Storage SDKs directly: the rules deny it, and all business rules live on the server. The same membership decides the role, so a customer sees the same data in the app and on the website.

---

## 5. Troubleshooting

| What you see | Likely cause | Fix |
|---|---|---|
| Startup error: *Firebase ProjectId and StorageBucket are required* | `Q4U:Mode` is `Firebase` but the Firebase settings aren't loaded | Check User Secrets (Step 9) are on **this** project and you're running the Development profile |
| *Your default credentials were not found* / *Could not load the default credentials* | `GOOGLE_APPLICATION_CREDENTIALS` not set, or Visual Studio started before you set it | Step 7; restart Visual Studio |
| Sign-in: *could not be completed… active workspace membership* | No `memberships/{uid}`, `active: false`, or bad role/tenant | `node provision.mjs list --project <id>`; re-run `user …` |
| *Authentication could not load* | Wrong `WebApiKey`/`AuthDomain`, or offline | Re-copy from Step 3 |
| `PERMISSION_DENIED` in server logs | Key belongs to a different project, or IAM role removed | Download a key from this project (Step 7) |
| `FAILED_PRECONDITION … database (default) does not exist` | Firestore not created | Step 5 |
| Upload says *could not be saved* (503) | Wrong bucket name, or Storage not created | Step 6; bucket name without `gs://` |
| *Refresh and try again* (409) | Someone saved first | Expected. Refresh, review, save again |
| Signed out after restarting the app | ASP.NET Data Protection keys regenerate | Fine locally; persist keys when hosting (section 6) |
| Can't create a key: *key creation is disabled by policy* | Your university Google account blocks keys | Use a personal Google account for the project, or `gcloud auth application-default login` then `gcloud auth application-default set-quota-project <project-id>` |

The old prototype layout (`tenants/{id}/workspace/state`, one JSON blob per organisation) is no longer read or written.

---

## 6. Known limits and next steps

- **Staff across organisations.** An Administrator sees one tenant. That works for the shop (staff and individual customers share `q4u`). A fleet company should be its own tenant, because fleet roles see their whole tenant. But then Q4U staff can't see that fleet's requests. Fix before onboarding fleets: a staff work queue that queries across tenants (`collectionGroup("requests").where("status", "in", […])`, which is why every record stores `tenantId`), plus a single-field index exemption with collection-group scope in `firestore.indexes.json`.
- **Staff page size.** Staff pages load every record in the tenant. Fine for hundreds of records; beyond that, load only open requests and pending documents, with paging for history.
- **Page-level versions.** Staff pages are guarded by one tenant version, so a busy shop will see more "refresh" messages on staff pages. Per-record versions would remove that.
- **Hosting.** Firebase Hosting serves static files only, so the ASP.NET Core server needs **Cloud Run** (same Google project; give the service its own service account with *Cloud Datastore User*, *Firebase Authentication Viewer* and *Storage Object Admin* on the bucket, so no key file) or Azure App Service (key file content in an app setting). Either way: environment variables instead of User Secrets (`Q4U__Mode=Firebase`, `Firebase__ProjectId=…`), persistent Data Protection keys, forwarded headers for HTTPS, and the domain added to Authorised domains.
- **Not connected (as in the original):** any way to record payment in Firebase mode, customer self-registration, email/SMS/push, invitation acceptance, MFA, automated backups (enable Firestore point-in-time recovery or scheduled exports before real use), malware scanning of uploads.

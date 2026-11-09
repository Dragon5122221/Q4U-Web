Running Website Link: https://q4u-web-626614350043.europe-west1.run.app
# Q4U · Expanded web application prototype

ASP.NET Core MVC · .NET 10 · Visual Studio · Firebase-ready web/mobile template

## Start here

1. Extract the archive into a normal writable folder.
2. Open **Q4UPrototypeWeb.slnx** in Visual Studio with the **ASP.NET and web development** workload and **.NET 10 SDK** installed. You can alternatively open the `.csproj`.
3. Allow NuGet restore to finish. Select the **https** launch profile and press F5. Trust the local ASP.NET development certificate if asked.
4. Open **Customer portal / Sign in**. Choose a demo profile and select **Open demo workspace**.
5. Use **Preview profile** at the top of the workspace to switch roles while keeping the same sample organisation. Sign-out then sign-in starts a new isolated demo organisation.

The included launch profiles use `https://localhost:7070` and `http://localhost:5285`. HTTP is useful for local troubleshooting; use HTTPS for Firebase mode.

No Firebase account, database, service account or payment credentials are needed for the default demo. Do not put real personal documents into the demo.

## What changed

The original public website, navy/yellow/white palette, ZA/Q4U logo treatment and Segoe UI font stack are retained. The operational portal now uses one shared workspace instead of separate pages with conflicting hard-coded records. The sidebar labels, next-step overview, visible statuses, empty states, help centre, forms and confirmations improve discoverability. Old portal URLs redirect to their new sections.

Implemented:
- Customer, FleetOwner, FleetManager, Administrator and Developer profiles, with server-side access and ownership checks.
- Vehicle create/edit/archive/restore, duplicate-registration checks, live expiry calculations, search/filter and CSV export.
- Renewal queue, driver/branch assignments and batch fleet renewal requests (up to 25 selected vehicles).
- Service requests for licences, plates, ownership, registration, fines, personalised plates, roadworthy assistance, fleet enquiries, support and privacy.
- Administrator quotes and status progression; cancellation checks; linked request records and in-app notifications.
- Quotes, explicit demo-only payments and printable demo receipts. No card details are collected.
- Private PDF/PNG/JPEG uploads and downloads, 5 MB limit, extension/signature validation, review outcomes and rejection notes.
- Team invitation **drafts** with clear status. Saving one neither sends email nor grants membership.
- Profile/contact preferences, account JSON export, scoped audit history and sign-out.
- Working browser accessibility preferences: 100–200% text, original/dark/high-contrast themes, reduced motion, reading spacing, link underlines, stronger keyboard focus, larger targets and Arial option. Preferences persist in browser storage where available.
- Keyboard navigation, skip links, native modal confirmations, focusable help tips, labelled controls, polite status/error messages and responsive layouts.
- Official Firebase Authentication / Firestore / Cloud Storage adapters; bearer-authenticated versioned API for a future mobile client.
- Optimistic version checks prevent lost updates between tabs or clients. Form posts use anti-forgery protection; the API accepts Firebase bearer tokens only.

## Role matrix

| Capability | Customer | Fleet owner | Fleet manager | Administrator | Developer |
|---|---|---|---|---|---|
| Vehicles, requests, documents | Own records | Organisation | Organisation | Organisation | No |
| Fleet assignments / batch renewals | No | Yes | Yes | Yes | No |
| Quotes / receipts | Own | Organisation | No | Organisation | No |
| Simulate payment (demo only) | Own | Organisation | No | No | No |
| Prepare invitation drafts | No | Yes | No | Yes | No |
| Issue quotes / review files / progress work | No | No | No | Yes | No |
| Audit log | No | No | No | Organisation | No |
| Integration centre | No | No | No | Yes | Yes |
| Profile / accessibility / help | Yes | Yes | Yes | Yes | Yes |

An administrator is scoped to one organisation, not a superuser across all tenants. Developer does not mean administrator. Customer ownership follows the creator UID; a fleet-managed request can therefore belong to the fleet operator rather than the individual vehicle owner. Set the desired delegation policy before live rollout.

## A five-minute walkthrough

1. Sign in as **Customer**. Open Vehicles and add a sample vehicle.
2. Upload a sample registration PDF in Documents.
3. In Service requests, open the vehicle and submit a Licence renewal.
4. Switch to **Administrator**, open Admin work queue, review the document and issue a quote.
5. Switch back to **Customer**, open Quotes & payments and **Simulate payment**. Open the receipt.
6. Switch to **Administrator** and mark the request Ready, then Completed.
7. Switch to **FleetOwner** to see all vehicles, assign drivers/branches, prepare invitation drafts and try batch renewals.
8. Switch to **Developer** to see integration information without customer records.
9. In Profile & accessibility, change text size or contrast and move between pages to verify persistence.

## Packages

Package references are already included; manual installation is normally unnecessary. Equivalent Package Manager Console commands:

```powershell
Install-Package FirebaseAdmin -Version 3.7.0
Install-Package Google.Cloud.Firestore -Version 4.4.0
Install-Package Google.Cloud.Storage.V1 -Version 5.0.0
```

CLI:

```powershell
dotnet restore
dotnet build
dotnet run --launch-profile https
```

The optional Firebase sign-in page loads the pinned Firebase JavaScript SDK from Google's CDN. Demo mode does not need this SDK to run.

## Code map

| Location | Responsibility |
|---|---|
| `Models/WorkspaceModels.cs` | Shared data and command contracts |
| `Services/WorkspaceService.cs` | Business rules, role restrictions, record projection |
| `Services/WorkspaceRepository.cs` | Isolated demo storage and transactional Firestore adapter |
| `Services/IdentityService.cs` | UID, trusted membership and verified Firebase tokens |
| `Services/DocumentWorkflow.cs` | Shared upload/download authorisation and validation |
| `Services/DocumentStorage.cs` | Private local files or configured Cloud Storage bucket |
| `Controllers/WorkspaceController.cs` | Web actions |
| `Controllers/WorkspaceApiController.cs` | Future mobile API |
| `Views/Workspace` | Operational views and receipt |
| `wwwroot/css/workspace.css` | Brand-preserving layout and accessibility overrides |
| `wwwroot/js/accessibility.js` | Browser-wide accessibility preferences |
| `docs/FIREBASE.md` | Configuration, membership provisioning and deployment boundaries |
| `docs/API.md` | API examples and mobile synchronisation contract |
| `docs/VALIDATION.md` | Checks actually completed and remaining checks |

The previous hard-coded portal controllers/views/scripts remain as reference material but are bypassed by the compatibility redirect in `Program.cs`. New development should target the workspace files above; do not extend the legacy hard-coded controllers. The public Home pages remain active.

## Deliberate prototype boundaries

This is a substantially expanded integration-ready prototype, **not a production-complete platform**.

- Demo records are held in an isolated memory cache for up to eight hours; a save resets that lifetime. Server restart clears them. Demo uploads remain in `App_Data/Documents` until you clean them up; stop the app before deleting that folder.
- Firebase code is included and compiled, but no live project or credentials were supplied. Live authentication, storage and transactions still require a configured-project acceptance test.
- Card/EFT processing, verified payment webhooks, refunds, external invoicing, email/SMS/push delivery, invitation acceptance, MFA enrolment, automatic reminders and government/vehicle-registry verification are not implemented.
- A Firebase-mode quote cannot move to paid/Processing through a client command. Connect a trusted payment webhook before enabling live payment processing.
- There is no automatic deletion workflow. Privacy requests go to Q4U for manual handling.
- The bounded Firestore prototype uses one JSON aggregate per organisation. Limits: 100 vehicles, 200 requests, 100 document metadata records, 50 invitation drafts, latest 200 audit entries/notices, 700 KB serialised workspace. This is intentionally simple for template evaluation; see the migration notes before a large fleet rollout.
- Document checks are not malware scanning. Add quarantine/scanning before accepting real uploads. Audit entries are useful prototype history, not a tamper-proof archive.
- Accessibility controls work, but this is not a claim of formal WCAG certification. Complete browser and assistive-technology acceptance testing before release.

## References

Official references used for the Firebase design:
- https://firebase.google.com/docs/admin/setup
- https://firebase.google.com/docs/auth/admin/verify-id-tokens
- https://firebase.google.com/docs/firestore/manage-data/transactions
- https://firebase.google.com/docs/firestore/security/rules-conditions

Read `docs/FIREBASE.md` before switching out of Demo mode.

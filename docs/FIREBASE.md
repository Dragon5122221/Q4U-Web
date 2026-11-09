# Connect Firebase later

## Architecture

Web MVC and future mobile API -> WorkspaceService -> repository / private document store.
Both clients use **the same Firebase Authentication UID and server-side organisation membership**. Firestore is the selected Firebase database; this implementation does not use Realtime Database. A mobile client must call the Q4U API; direct Firestore writes would bypass the application's workflow and are denied by the supplied rules.

Firebase server libraries bypass client Security Rules. Consequently, every API/MVC action checks the verified actor and the repository is never exposed directly. IAM still needs least-privilege configuration.

## Project setup

1. Create a Firebase project and register web and Android apps in that same project.
2. Enable Email/Password in Firebase Authentication. Create your initial users in the console. Public self-registration and invitation acceptance need a later trusted onboarding flow.
3. Create Cloud Firestore in Native mode and configure a private Cloud Storage bucket. Choose regions and retention according to your actual operating requirements.
4. Deploy the deny-by-default Firestore and Storage rules supplied in `firebase/`. They deny direct client SDK access; the server uses IAM/Application Default Credentials.
5. Give the server identity the required Firestore data access, Firebase authentication verification/user lookup access, and bucket-scoped object create/read/delete permissions. Do not give broad project Owner access for convenience.
6. Use Application Default Credentials in hosting. For local Visual Studio integration testing only, set `GOOGLE_APPLICATION_CREDENTIALS` to a service-account JSON file **outside the repository**. Never put credentials in wwwroot, appsettings, the mobile app or a downloadable archive.
7. Set `Q4U:Mode` to `Firebase`, plus `Firebase:ProjectId`, `Firebase:StorageBucket`, `Firebase:WebApiKey`, `Firebase:AuthDomain`. Use Visual Studio Manage User Secrets or environment variables; the project has a UserSecretsId. Restart the app after setting an environment variable.

Example User Secrets (replace all placeholders):

```json
{
  "Q4U": { "Mode": "Firebase" },
  "Firebase": {
    "ProjectId": "your-project-id",
    "StorageBucket": "your-exact-bucket-name",
    "WebApiKey": "your-firebase-web-api-key",
    "AuthDomain": "your-project-id.firebaseapp.com"
  }
}
```

Environment variable equivalents use double underscores, e.g. `Q4U__Mode` and `Firebase__ProjectId`. The web API key is public client configuration, not an admin credential. Protect its allowed uses and authorised domains. Keep service-account keys secret.

## Provision a trusted membership

Create Firestore document `memberships/{firebaseUid}` with:

```json
{ "active": true, "tenantId": "q4u-example-fleet", "role": "FleetOwner" }
```

`tenantId`: 1–80 letters, digits, underscores or hyphens. `role`: exactly `Customer`, `FleetOwner`, `FleetManager`, `Administrator` or `Developer`.

Create memberships only from a trusted administration environment. A user must never be allowed to write their role or tenant. There is currently one active organisation membership per UID. Team invitation drafts do not create this document.

A tenant with no workspace document starts empty. The first successful command writes `tenants/{tenantId}/workspace/state`. The trusted membership document is checked on each Firebase-authenticated request, including cookie validation. Deactivating membership takes effect on the next request. Revoked or expired tokens are rejected.

The web login verifies the Firebase ID token and places it inside an encrypted HttpOnly cookie for a fixed 50-minute session. There is no silent refresh in this prototype: sign in again after expiry. Firebase login credentials are held only in memory by the client SDK. Live cookies require HTTPS.

## Data layout and scaling

Current collections:
- `memberships/{uid}`: `active`, `tenantId`, `role`, provisioned outside client workflows.
- `tenants/{tenantId}/workspace/state`: `json`, integer `version`, `schemaVersion`, server `updatedAt`.
- Private bucket objects: `tenants/{tenantId}/{randomGuid}`. Document metadata contains the internal storage key; API/export responses remove it.

The JSON contains vehicles, requests, document metadata, invitation drafts, notices, profiles and recent audit entries. All mutations use an expected workspace version. Firestore transaction retries re-read the version; a conflicting update returns 409 rather than overwriting newer data. Uploads write the blob before metadata and attempt cleanup if metadata fails. Monitor/clean orphan objects if cleanup fails.

Before larger fleets or sustained concurrent traffic, implement a repository migration to per-entity subcollections with indexed queries, pagination, per-record versions and immutable audit/event storage. Avoid treating the aggregate as a permanent production database layout. Introduce an API migration/version policy when changing contracts.

## Mobile handover

Use the same Firebase project and UID. Obtain a fresh ID token from the Android Firebase Auth SDK and send it as a bearer token to `/api/v1/workspace`. The server determines tenant and permissions from membership, never from a user-supplied role. Use the current `version` in every command. On 409, re-fetch and let the user review the conflicting operation. Do not retry a stale mutation blindly.

Dates are ISO `yyyy-MM-dd`; instants are ISO-8601 UTC; amounts are decimal ZAR. UI date calculations use UTC+02:00. Entity IDs remain stable between devices. Accessibility settings are currently browser-local; shared profile/contact details live in the workspace.

Offline caching, push tokens, background scheduling and realtime subscriptions are future mobile work. The current API is online request/response. Do not introduce direct database writes to simulate synchronisation.

## Before live use

Validate with a dedicated Firebase test project: valid/expired/revoked tokens, inactive/missing membership, two separate tenants, concurrent version conflicts, denied cross-tenant downloads and private storage rules. Configure deployment secrets, protected persistent Data Protection keys, HTTPS, reverse-proxy handling, rate limits, backups, observability, incident response, file quarantine, retention and deletion. Demo mode intentionally refuses to start outside Development.

A payment provider must use a server-verified, idempotent webhook to record payment and progress a quote. Do not add a client-supplied `paid=true` endpoint. Invitation delivery/acceptance likewise needs trusted role assignment and single-use expiring tokens.

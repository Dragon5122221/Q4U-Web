# Q4U API v1

Base path: `/api/v1/workspace`
Authentication: `Authorization: Bearer <Firebase ID token>`. Demo cookies are not accepted. The app must be in Firebase mode. Never send a service-account key from a client.

| Method | Route | Result |
|---|---|---|
| GET | `/me` | Verified UID, tenant, role, name and demo flag |
| GET | `/` | Role-filtered workspace, schemaVersion and version; ETag mirrors version |
| POST | `/commands` | Applies an allowed command; re-fetch after success |
| POST | `/documents` | Multipart document upload using shared validation |
| GET | `/documents/{id}` | Authorised attachment download |

## Example: create a vehicle

Fetch current workspace, then send:

```json
{
  "action": "vehicle",
  "version": 0,
  "value": "JT 42 ZN GP",
  "details": "Volkswagen Polo · 2021",
  "date": "2027-08-22"
}
```

Omit `id` when adding; include the vehicle ID when editing. All examples assume the caller has the required role and the current version. The response does not return the updated version: GET the workspace again.

## Command fields

| Action | Fields besides `version` | Permission / behaviour |
|---|---|---|
| vehicle | id optional, value registration, details description, date expiry | Own or permitted organisation vehicle |
| archive | id vehicle | Toggles archive/restore; open requests block archive |
| assignment | id vehicle, value driver, extra branch | Fleet operations |
| request | id vehicle, value service, details notes/address, extra Collection/Delivery | Active visible vehicle |
| bulkRenewal | vehicleIds array (1–25 IDs) | Fleet operations; all-or-nothing; existing open renewal blocks batch |
| cancel | id request | Unpaid Submitted/Quoted only |
| quote | id request, amount positive decimal | Administrator; Submitted -> Quoted |
| payDemo | id request | Demo only, unavailable to live API |
| progress | id request, value Ready/Completed | Administrator; Processing -> Ready -> Completed |
| review | id document, value Verified/Rejected, details note | Administrator; rejection needs explanation |
| invite | value email, extra Customer/FleetManager | FleetOwner/Administrator; draft only |
| removeInvite | id invitation | FleetOwner/Administrator |
| profile | value name, extra phone, details address, id on/empty | Own profile; id saves reminder preference only |
| read | id notice or all | Own notices |

Allowed service names are in `WorkspaceService.Services`. The generic command `upload` is reserved for the multipart route and cannot fabricate metadata.

For upload, send multipart fields `Action=upload`, `Version`, `Id=<vehicle ID>`, `Value=<category>` and `file`. Categories: Identity, Proof of address, Registration, Licence disc, Other. Maximum file size 5 MB; maximum entire request 6 MB. Allowed file signatures and matching extensions: PDF, PNG, JPEG. The server generates object paths; caller-supplied paths are never used.

## Status and errors

- 200: accepted/read. Re-fetch before the next mutation.
- 400: invalid fields or workflow rule, with ProblemDetails on API routes.
- 401: absent/invalid/expired token.
- 403: role denied or inactive membership.
- 404: record unavailable to this actor or file no longer available.
- 409: version conflict or prototype capacity; refresh/review before retrying.
- 413: request body exceeds size limit.
- 503: temporary file-storage error.

Cookie forms use anti-forgery tokens. API routes use bearer authentication instead and never accept the demo preview identity. No CORS allowance is needed for a native mobile app; explicitly configure origins if a separate browser frontend is later introduced.

## Synchronisation and payment boundaries

This is a shared online API, not an offline-sync engine. Never blindly replay commands after network failures: refresh and inspect the result first. IDs and version checks prevent stale overwrites, but arbitrary replay idempotency keys are not implemented. Introduce command IDs/idempotency storage before offline queues or automatic retries.

No live client command marks a request paid. Payment integration must validate the provider's webhook signature, amount, currency and request reference, with an idempotent server transaction. Offline payments need an authorised reconciliation process too.

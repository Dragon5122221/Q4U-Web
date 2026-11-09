# Part2 testing and deployment

The live service is https://q4u-web-626614350043.europe-west1.run.app.

## Workflow

GitHub Actions builds the app and runs the xUnit project on branch pushes and pull requests into Part2. After a successful test job, a push or merged pull request into Part2 builds a Docker image, uploads it to Artifact Registry and deploys q4u-web in europe-west1. Other branches and pull requests cannot deploy. A failed test stops deployment.

Part2 is the integration/deployment branch; main contains the original prototype. For later changes, create a feature branch from Part2, push changes, open a pull request with Part2 as its base, inspect the test results, then merge. The merge starts the deployment workflow. Requiring the test check before merging needs a repository branch rule; the workflow alone does not prevent a merge or direct push.

## Tests

From the repository root:

```powershell
dotnet test tests/Q4UPrototypeWeb.Tests/Q4UPrototypeWeb.Tests.csproj --configuration Release
```

The tests cover vehicle creation and ownership, invalid and duplicate registrations, stale versions, customer data filtering, archiving with open requests, quote permissions, administrator quotes, disabled simulated payments for Firebase users, delivery address validation, document content and extension validation, file size, upload cleanup after a failed metadata save, and private downloads.

They call the real WorkspaceService and DocumentWorkflow with in-memory repository/storage substitutes. They require no Firebase key and never write to the live database. These are service-level unit tests, not browser, Firebase integration or authentication-token tests. Results are saved as a TRX artifact on the Actions run.

The package was statically reviewed when prepared. Successful compilation and execution must be confirmed in Visual Studio or GitHub Actions; no passing run is claimed in this document.

## Hosting and access

Cloud Run runs the ASP.NET container and scales down when idle. Firebase supplies authentication, Firestore and document storage. Persistent records therefore survive replacing a container. The runtime uses q4u-runtime; GitHub uses a separate q4u-github-deploy account through Workload Identity Federation, without a downloaded private key. Federation accepts only this repository's Part2 push workflow. The deploy account can deploy Cloud Run, push images to the deployment repository and act as the runtime account.

The deployment account cannot change public-access policy; it preserves the service's existing public access. Runtime settings remain 512 MiB, one CPU, zero minimum instances and one maximum instance. These settings do not impose a hard spending cap. Container images and Actions runs can also consume storage/build allowances.

The workflow checks the public home page after deployment. If that check fails, inspect the revision and application logs; there is no automatic rollback. To roll back, Cloud Run > q4u-web > Revisions > Manage traffic can send traffic to an earlier healthy revision. Use a tested commit on Part2 for the next deployment.

ASP.NET cookie encryption keys are currently container-local, so a new revision may require users to log in again. Authentication, vehicle persistence and document upload/download were manually checked on the initial hosted revision. Repeat a short login/save check after the first automated deployment.

## Evidence for submission

Capture the completed GitHub Actions run with both jobs green, the expanded unit-test output and count, a feature pull request targeting Part2, the new Cloud Run revision, and the live URL. Do not describe the pipeline as working until an actual run succeeds.

## Setup and troubleshooting

The supplied setup-github-cloud.sh is run once in Cloud Shell by a project owner before the first push. It creates the deployment account and repository/branch-restricted federation. No GitHub secret or Firebase key needs adding.

If federation reports a new provider is unavailable immediately after setup, allow several minutes for IAM propagation and re-run the failed job. If Actions is blocked by campus policy, ask the repository/organisation administrator to enable the required Actions; do not move the project or bypass that policy. Keep the already-working manual deployment available while resolving access.

References:
- https://github.com/google-github-actions/auth
- https://docs.cloud.google.com/iam/docs/workload-identity-federation-with-deployment-pipelines
- https://docs.cloud.google.com/run/docs/deploying
- https://github.com/actions/setup-dotnet

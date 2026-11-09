#!/usr/bin/env bash
# Run once in Google Cloud Shell as a project owner, before pushing the workflow.
set -euo pipefail
project='q4u-licensing-2354'
number='626614350043'
repo='EMGPSD/insy7315-g2-2026-insy7315-2026-project-st10438564'
deployer="q4u-github-deploy@${project}.iam.gserviceaccount.com"
runtime="q4u-runtime@${project}.iam.gserviceaccount.com"

gcloud projects describe "$project" --format='value(projectNumber)' | tr -d '\r' | grep -qx "$number"
gcloud services enable iam.googleapis.com iamcredentials.googleapis.com sts.googleapis.com --project="$project"

if ! gcloud iam service-accounts describe "$deployer" --project="$project" >/dev/null 2>&1; then
  gcloud iam service-accounts create q4u-github-deploy --display-name='Q4U GitHub deployment' --project="$project"
fi

gcloud projects add-iam-policy-binding "$project" \
  --member="serviceAccount:$deployer" --role=roles/run.developer --condition=None --quiet

gcloud artifacts repositories add-iam-policy-binding cloud-run-source-deploy \
  --project="$project" --location=europe-west1 \
  --member="serviceAccount:$deployer" --role=roles/artifactregistry.writer --quiet

gcloud iam service-accounts add-iam-policy-binding "$runtime" \
  --project="$project" --member="serviceAccount:$deployer" --role=roles/iam.serviceAccountUser --quiet

if ! gcloud iam workload-identity-pools describe q4u-github --project="$project" --location=global >/dev/null 2>&1; then
  gcloud iam workload-identity-pools create q4u-github --project="$project" --location=global --display-name='Q4U GitHub Actions'
fi

condition="assertion.repository=='${repo}' && assertion.repository_owner=='EMGPSD' && assertion.ref=='refs/heads/Part2' && assertion.event_name=='push' && assertion.workflow_ref=='${repo}/.github/workflows/q4u-ci-cd.yml@refs/heads/Part2'"
mapping='google.subject=assertion.sub,attribute.repository=assertion.repository'
if gcloud iam workload-identity-pools providers describe github --project="$project" --location=global --workload-identity-pool=q4u-github >/dev/null 2>&1; then
  gcloud iam workload-identity-pools providers update-oidc github \
    --project="$project" --location=global --workload-identity-pool=q4u-github \
    --issuer-uri=https://token.actions.githubusercontent.com \
    --attribute-mapping="$mapping" --attribute-condition="$condition"
else
  gcloud iam workload-identity-pools providers create-oidc github \
    --project="$project" --location=global --workload-identity-pool=q4u-github \
    --issuer-uri=https://token.actions.githubusercontent.com \
    --attribute-mapping="$mapping" --attribute-condition="$condition"
fi

gcloud iam service-accounts add-iam-policy-binding "$deployer" \
  --project="$project" --role=roles/iam.workloadIdentityUser \
  --member="principalSet://iam.googleapis.com/projects/${number}/locations/global/workloadIdentityPools/q4u-github/attribute.repository/${repo}" --quiet

printf '\nSETUP COMPLETE. You can now push the workflow and tests to Part2.\n'

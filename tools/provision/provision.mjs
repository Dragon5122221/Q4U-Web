#!/usr/bin/env node
// Q4U provisioning tool. Run it on a trusted computer only: it uses a Firebase Admin credential,
// which bypasses every security rule. Never ship it, or its credential, with the website or mobile app.
//
// Credentials: set GOOGLE_APPLICATION_CREDENTIALS to the service-account JSON file (kept outside the repository),
// or run `gcloud auth application-default login` first.
import { initializeApp, applicationDefault } from 'firebase-admin/app';
import { getAuth } from 'firebase-admin/auth';
import { getFirestore, FieldValue } from 'firebase-admin/firestore';
import { parseArgs } from 'node:util';

const ROLES = ['Customer', 'FleetOwner', 'FleetManager', 'Administrator', 'Developer'];
const TENANT_ID = /^[a-zA-Z0-9_-]{1,80}$/;
const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const USAGE = `Q4U provisioning tool

Usage:
  node provision.mjs <command> [arguments] --project <firebase-project-id>

Commands:
  tenant <tenantId> "<Organisation name>"
      Create or rename an organisation. Example: tenant q4u "Q4U Licensing Services"

  user <email> <role> <tenantId> [--name "Full Name"] [--password "..."]
      Create the Firebase user if needed and give them a membership.
      Roles: ${ROLES.join(', ')}
      Without --password a new user gets a password-reset link to send them.
      Running it again for an existing user changes their role or tenant.

  deactivate <email>
      Switch off the membership and revoke the user's sessions immediately.

  list [tenantId]
      Show memberships, optionally for one tenant.

Options:
  --project   Firebase project ID (or set FIREBASE_PROJECT_ID). Required, so you never write to the wrong project.
  --name      Display name for the user.
  --password  Initial password (8+ characters). Prefer the reset link for real people.
  --help      Show this help.`;

function fail(message) {
  console.error(`\n✖ ${message}\n`);
  process.exit(1);
}

let parsed;
try {
  parsed = parseArgs({
    allowPositionals: true,
    options: {
      project: { type: 'string' },
      name: { type: 'string' },
      password: { type: 'string' },
      help: { type: 'boolean', short: 'h' }
    }
  });
} catch (error) {
  fail(`${error.message}\n\n${USAGE}`);
}
const { values: options, positionals } = parsed;
const [command, ...args] = positionals;

if (!command || options.help) {
  console.log(USAGE);
  process.exit(command || options.help ? 0 : 1);
}
if (!['tenant', 'user', 'deactivate', 'list'].includes(command)) fail(`Unknown command "${command}".\n\n${USAGE}`);

const projectId = options.project ?? process.env.FIREBASE_PROJECT_ID;
if (!projectId) fail('Add --project <firebase-project-id> (or set FIREBASE_PROJECT_ID).');

initializeApp({ credential: applicationDefault(), projectId });
const auth = getAuth();
const db = getFirestore();

async function findUser(email) {
  try {
    return await auth.getUserByEmail(email);
  } catch (error) {
    if (error.code === 'auth/user-not-found') return null;
    throw error;
  }
}

async function createTenant([tenantId, ...nameParts]) {
  const organisation = nameParts.join(' ').trim();
  if (!TENANT_ID.test(tenantId ?? '')) fail('Tenant ID: 1–80 letters, digits, underscores or hyphens, e.g. q4u.');
  if (!organisation || organisation.length > 120) fail('Give the organisation a name of up to 120 characters.');
  // No version field is written here: the web application owns it and treats a missing value as 0.
  await db.doc(`tenants/${tenantId}`).set(
    { organisation, schemaVersion: 2, updatedAt: FieldValue.serverTimestamp() },
    { merge: true }
  );
  console.log(`✔ Tenant "${tenantId}" is ready: ${organisation}`);
}

async function provisionUser([email, role, tenantId]) {
  email = (email ?? '').trim().toLowerCase();
  if (!EMAIL.test(email)) fail('Give a valid email address.');
  if (!ROLES.includes(role)) fail(`Role must be exactly one of: ${ROLES.join(', ')}`);
  if (!TENANT_ID.test(tenantId ?? '')) fail('Tenant ID: 1–80 letters, digits, underscores or hyphens.');
  if (options.password !== undefined && options.password.length < 8) fail('Use a password of at least 8 characters.');
  if (!(await db.doc(`tenants/${tenantId}`).get()).exists) {
    fail(`Tenant "${tenantId}" does not exist yet. Run: node provision.mjs tenant ${tenantId} "Organisation name" --project ${projectId}`);
  }

  let user = await findUser(email);
  const created = !user;
  if (created) {
    const request = { email };
    if (options.password) request.password = options.password;
    if (options.name) request.displayName = options.name;
    user = await auth.createUser(request);
  } else {
    const changes = {};
    if (options.name && user.displayName !== options.name) changes.displayName = options.name;
    if (options.password) changes.password = options.password;
    if (Object.keys(changes).length) user = await auth.updateUser(user.uid, changes);
  }

  // The web application reads this document on every request. Only this tool (or Customer self-registration)
  // may write it: a user must never be able to choose their own role or organisation.
  await db.doc(`memberships/${user.uid}`).set({
    active: true,
    tenantId,
    role,
    email,
    source: 'provisioning',
    updatedAt: FieldValue.serverTimestamp()
  });

  console.log(`✔ ${created ? 'Created' : 'Updated'} ${email}`);
  console.log(`  uid: ${user.uid}  role: ${role}  tenant: ${tenantId}`);
  if (created && !options.password) {
    const link = await auth.generatePasswordResetLink(email);
    console.log('\n  Send this one-time link so they can choose a password:\n');
    console.log(`  ${link}\n`);
  }
}

async function deactivate([email]) {
  const user = await findUser((email ?? '').trim().toLowerCase());
  if (!user) fail(`No Firebase user has the email ${email}.`);
  await db.doc(`memberships/${user.uid}`).set({ active: false, updatedAt: FieldValue.serverTimestamp() }, { merge: true });
  // The server verifies tokens with revocation checks, so open sessions end on their next request.
  await auth.revokeRefreshTokens(user.uid);
  console.log(`✔ Deactivated ${user.email} and signed them out everywhere.`);
}

async function list([tenantId]) {
  if (tenantId !== undefined && !TENANT_ID.test(tenantId)) fail('Tenant ID: 1–80 letters, digits, underscores or hyphens.');
  let query = db.collection('memberships');
  if (tenantId) query = query.where('tenantId', '==', tenantId);
  const snapshot = await query.get();
  if (snapshot.empty) return console.log('No memberships found.');
  console.table(snapshot.docs.map(doc => {
    const m = doc.data();
    return { email: m.email ?? '', role: m.role, tenant: m.tenantId, active: m.active, uid: doc.id };
  }));
}

function report(error) {
  const credentialProblem = /default credentials|invalid_grant|PERMISSION_DENIED|UNAUTHENTICATED/i.test(String(error?.message ?? error));
  const hint = credentialProblem
    ? '\nSet GOOGLE_APPLICATION_CREDENTIALS to this project\'s service-account key file (docs/FIREBASE.md, step 6), then open a new terminal.'
    : '';
  fail(`${error?.code ? error.code + ': ' : ''}${error?.message ?? error}${hint}`);
}
// Some credential errors surface outside the awaited call, so catch those too.
process.on('uncaughtException', report);
process.on('unhandledRejection', report);

try {
  await { tenant: createTenant, user: provisionUser, deactivate, list }[command](args);
  process.exit(0);
} catch (error) {
  report(error);
}

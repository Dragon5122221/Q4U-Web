# Validation record

## Completed

- .NET 10 build with the included NuGet package versions: **0 errors, 0 warnings**.
- JavaScript syntax checks for the new workspace and accessibility scripts passed.
- Focused HTTP checks against the locally running Development app passed:

1. Customer pages and own-record filtering.
2. Customer blocked from admin.
3. CSRF protection.
4. API rejects demo cookies.
5. Vehicle create.
6. Request create.
7. Cannot archive vehicle with open request.
8. Upload and authorised download.
9. Admin quote and document review.
10. Demo payment and receipt.
11. Request progression.
12. Fleet manager billing restriction.
13. Owner invitation draft.
14. Developer has no customer record access.
15. Browser workspace isolation.
16. Fleet batch renewal.
17. Duplicate batch renewal blocked.
18. Stale version cannot overwrite.
19. Cross-tenant file access denied.
20. Extension/content mismatch rejected.
21. Public pages and new assets respond.

These checks exercised the real application routes and server logic, using isolated sample demo sessions. They did not create or connect a Firebase project.

## Not completed here

- Rendered desktop/mobile browser inspection and screen-reader testing: the browser runtime download failed in the execution environment. The responsive CSS and accessibility code are present, but a visual pass is still required on your machine.
- Live Firebase authentication, revocation, Firestore transactions, Cloud Storage IAM and rules: no live project or credentials supplied.
- Payment, email/SMS, invitation acceptance, MFA, push or scheduled jobs: intentionally unconnected and labelled in the UI.

## Quick Visual Studio acceptance pass

1. Follow the README's five-minute walkthrough.
2. Check desktop and a narrow phone viewport. Open/close the Menu with keyboard and touch.
3. Try all text sizes, original/dark/high contrast, extra spacing and large targets; navigate to a second page and reload to confirm persistence. Reset settings.
4. Tab from the skip link through the sidebar and forms. Verify visible focus, help tips, Escape, and confirmation dialog focus.
5. Try empty required fields, invalid file extensions, oversized files and stale tabs.
6. Confirm public service buttons open the portal prompt and all service sections are reachable.
7. Run the configured-Firebase checks listed in FIREBASE.md before using real accounts.

This validation record is not a formal accessibility certification or production security assessment.

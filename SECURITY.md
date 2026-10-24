# Security Policy

startlist.club is an open-source flight log for gliding clubs, run as a hosted
service at https://startlist.club. Club flight logs include pilot names and
contact details, so we take security reports seriously and appreciate your help.

## Supported versions

There are no separately released versions. Security fixes are made on `master`
and deployed to the hosted service.

| Target | Supported |
| ------ | --------- |
| https://startlist.club (production) | :white_check_mark: |
| `master` branch | :white_check_mark: |
| Forks and self-hosted copies | :x: (please apply fixes from `master`) |

## Reporting a vulnerability

**Please do not open a public issue, pull request or discussion for security problems.**

Report privately via GitHub: go to the **Security** tab of this repository and
choose **"Report a vulnerability"**. Include:

- what you found and where (URL or endpoint, affected file if known)
- steps to reproduce, and the impact you believe it has
- whether you accessed any data that is not your own (see below)

### What to expect

This is a volunteer-maintained project, so these are targets, not guarantees:

- **Acknowledgement** within 5 working days.
- **Initial assessment** (accepted / not a vulnerability / need more info) within 14 days.
- **Fix:** high-severity issues are prioritised and deployed as soon as practical.
  We'll keep you updated in the private advisory.
- **Disclosure:** we coordinate disclosure with you. Details are published only
  after the fix is deployed to production. With your permission we credit you
  in the advisory.

## Scope

In scope: the startlist.club web application and its source code in this repository.

Out of scope:
- denial-of-service, load or rate-limit testing, and automated scanning that
  produces significant traffic
- social engineering, phishing and physical attacks
- third-party services we integrate with (Open Glider Network / APRS, login
  providers, SMS and e-mail providers). Report those to the provider.
- reports without a demonstrated security impact (e.g. missing headers on
  their own, version banners)

## Testing guidelines (safe harbour)

We will not pursue legal action for good-faith research that follows these rules:

- Prefer the demo site at http://demo.startlist.club for testing.
- Only use accounts you own. Never access, modify or delete other users' or
  clubs' data. If you accidentally encounter personal data, stop, don't keep
  a copy, and tell us in your report.
- Don't degrade the service for gliding clubs using it.
- Give us reasonable time to fix the issue before any public disclosure.

Thank you for helping keep club and pilot data safe.

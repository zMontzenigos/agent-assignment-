# Functional Specification

> Auto-maintained by the Spec Sync Agent. Run `dotnet run` to populate.

## Overview

This document describes the Password Reset via Email feature, which allows registered users to regain access to their accounts by requesting a secure, time-limited reset link sent to their registered email address.

## Features

### 1. Password Reset Request

A registered user may initiate a password reset by submitting their email address. The system looks up the email against known accounts and responds as follows:

- **Email found:** A reset email is dispatched containing a one-time link.
- **Email not found:** The request is silently declined (no email is sent). The system returns a "not found" outcome to the caller, but should not reveal to the end user whether the address exists, to prevent account enumeration.
- **Too many recent attempts:** If the user has exceeded the allowed number of reset requests within the rate-limit window, the request is rejected immediately and no email is sent (see Rate Limiting below).

### 2. Rate Limiting

To prevent abuse, reset requests are throttled on a per-email basis. When the limit is exceeded, the system returns a "rate limited" outcome. No further reset emails are sent until the window resets. *(The specific threshold — e.g., max 3 requests per hour — is defined in system configuration.)*

### 3. Password Policy Enforcement

When a user submits a new password via the reset form, the system validates it against the password policy before accepting the change. The policy requires:

- Minimum **8 characters** in length
- At least **one uppercase letter**
- At least **one numeric digit**

If the submitted password does not meet these requirements, the reset is rejected with a "policy violation" outcome, and the user is prompted to choose a stronger password. The password is not changed.

### 4. Confirmation Email

Upon a successful password reset, the system automatically sends the user a confirmation email notifying them that their password has been changed. The email advises the user to contact support immediately if they did not initiate the change, helping them respond quickly to any unauthorized access.

## Acceptance Criteria Coverage

| # | Criterion | Status |
|---|-----------|--------|
| 1 | User can request a password reset by submitting their email address | Covered |
| 2 | System sends a reset email only if the email belongs to an active account | Partially covered — "not found" case is handled; active/inactive account distinction not explicitly addressed |
| 3 | The reset link contains a unique, time-limited token (expires in 1 hour) | Not addressed in this change |
| 4 | Clicking the link takes the user to a form where they can set a new password | Not addressed in this change |
| 5 | The token is invalidated after it has been used once | Not addressed in this change |
| 6 | New password must meet password policy (min 8 chars, 1 uppercase, 1 number) | Covered |
| 7 | User receives a confirmation email after successfully resetting their password | Covered |
| 8 | Reset requests are rate-limited (max 3 per hour per email) | Covered |

## Open Items

- **AC 2 — Active account check:** The system correctly handles unknown email addresses, but it is unclear whether reset requests for accounts that exist but are inactive or suspended are also blocked. This should be clarified and, if required, implemented.
- **AC 3 — Token expiry:** Generation of a unique, time-limited token (1-hour expiry) is not reflected in the current changes. Token issuance and expiry logic must be implemented or confirmed as pre-existing.
- **AC 4 — Reset form UI:** The user-facing form where a new password is entered after clicking the reset link is not addressed in this change. End-to-end flow from link click to form submission remains to be implemented or confirmed.
- **AC 5 — Single-use token invalidation:** There is no indication that used tokens are invalidated after a successful reset. This must be implemented to prevent replay attacks.

## Changelog

| Date | Description |
|------|-------------|
| 2026-05-06 | Initial spec created: documents password reset request flow, rate limiting, password policy enforcement, and post-reset confirmation email. |
# User Story: Password Reset via Email

**As a** registered user,
**I want to** reset my password by receiving a one-time link via email,
**So that** I can regain access to my account if I forget my password.

## Acceptance Criteria

1. User can request a password reset by submitting their email address.
2. System sends a reset email only if the email belongs to an active account.
3. The reset link contains a unique, time-limited token (expires in 1 hour).
4. Clicking the link takes the user to a form where they can set a new password.
5. The token is invalidated after it has been used once.
6. The new password must meet the existing password policy (min 8 chars, 1 uppercase, 1 number).
7. User receives a confirmation email after successfully resetting their password.
8. Reset requests are rate-limited to prevent abuse (max 3 per hour per email).

# Technical Specification

> Auto-maintained by the Spec Sync Agent. Run `dotnet run` to populate.

## Overview

The Password Reset feature allows registered users to regain account access by requesting a time-limited, single-use reset link delivered via email. The core logic lives in `PasswordResetService`, which orchestrates token generation, email dispatch, password validation, and rate limiting.

## Architecture

The service layer follows a dependency-injected design. `PasswordResetService` takes no direct infrastructure dependencies; all I/O is abstracted behind interfaces injected at construction time.

```
Client
  └─► PasswordResetService
        ├─► IUserRepository      (user lookup by email / by ID)
        ├─► ITokenRepository     (token creation, lookup, invalidation)
        ├─► IEmailService        (reset link email + confirmation email)
        └─► IRateLimiter         (per-user request throttling)
```

**Request flow — `RequestResetAsync`:**
1. Check rate limit via `IRateLimiter`; return `RateLimited` if exceeded.
2. Look up user by email via `IUserRepository`; return `NotFound` if absent.
3. Generate a unique, time-limited token (1-hour TTL) and persist it.
4. Send reset-link email via `IEmailService`.
5. Return `Sent`.

**Reset flow — `ResetPasswordAsync`:**
1. Look up token via `ITokenRepository`; reject if missing or expired.
2. Validate new password against `PasswordPolicy`; return `PolicyViolation` if invalid.
3. Hash and persist the new password.
4. Invalidate the token (single-use enforcement).
5. Look up user by ID via `IUserRepository.FindByIdAsync` to obtain email address.
6. Send confirmation email advising the user to contact support if the change was unexpected.

## Components & Responsibilities

### `PasswordResetService`
Orchestrates the full reset lifecycle. Returns rich result enums to callers rather than raw booleans.

### `RequestResetResult` (enum)
Return type of `RequestResetAsync`.

| Value | Meaning |
|---|---|
| `Sent` | Token generated and email dispatched. |
| `NotFound` | No active account matches the provided email. |
| `RateLimited` | User has exceeded the allowed request frequency. |

### `ResetPasswordResult` (enum)
Return type of `ResetPasswordAsync`.

| Value | Meaning |
|---|---|
| `Success` | Password updated and confirmation email sent. |
| `InvalidToken` | Token not found, already used, or expired. |
| `PolicyViolation` | New password does not satisfy `PasswordPolicy`. |

### `PasswordPolicy`
Stateless validator encapsulating the password rules:
- Minimum 8 characters
- At least one uppercase letter
- At least one digit

### `IRateLimiter`
Abstraction over a sliding-window or fixed-window counter. Implementations decide storage (in-memory, Redis, etc.) and window size. The contract used by `PasswordResetService` enforces a maximum of **3 reset requests per hour per email address**.

### `IUserRepository`
| Method | Purpose |
|---|---|
| `FindByEmailAsync(email)` | Locate a user during reset request. |
| `FindByIdAsync(id)` | Retrieve user email for confirmation message after reset. |
| `UpdatePasswordAsync(userId, hashedPassword)` | Persist the new credential. |

### `ITokenRepository`
Manages lifecycle of reset tokens: creation with expiry, single-use lookup, and invalidation.

### `IEmailService`
Sends two distinct messages:
- **Reset link email** — contains the one-time token URL, sent on `RequestResetAsync`.
- **Confirmation email** — notifies the user of a successful change and advises contacting support if unexpected, sent on `ResetPasswordAsync` success.

## Design Decisions

1. **Rich result enums over booleans** — `RequestResetAsync` returns `RequestResetResult` rather than `bool` so callers can distinguish between "email not found", "rate limited", and "sent" without out-parameters or exceptions. This keeps the API surface honest about failure modes.

2. **Rate limiting as a first-class dependency** — `IRateLimiter` is injected rather than inlined, keeping `PasswordResetService` testable and allowing the throttle implementation to be swapped (e.g., from in-memory to distributed) without changing business logic.

3. **`FindByIdAsync` added to `IUserRepository`** — The confirmation email requires the user's address at reset time, but the reset token only carries a user ID. Rather than re-querying by email (which would require storing it redundantly on the token), a by-ID lookup was added to the existing repository interface.

4. **Password policy extracted to `PasswordPolicy` class** — Rules are co-located in one place rather than scattered across validation calls, making future policy changes (e.g., special characters, breach-list checks) a single-file edit.

5. **Confirmation email sent post-invalidation** — The token is invalidated before the confirmation email is dispatched, ensuring that even if the email step fails the token cannot be reused.

## Open Items

1. **`IRateLimiter` storage strategy unspecified** — The interface is defined but no concrete implementation is prescribed. In a multi-instance deployment, an in-memory counter will not share state across pods; a distributed store (Redis, database) will be required.

2. **Token expiry enforcement location unclear** — It is not specified whether token expiry is checked in `ITokenRepository` (filtered at query time) or in `PasswordResetService` after retrieval. This should be made explicit to avoid a TOCTOU window.

3. **`NotFound` response and user enumeration** — Returning `NotFound` to callers when an email does not exist allows an attacker to enumerate registered addresses. Consider whether `Sent` should be returned regardless (silent no-op when email is unknown).

4. **Password hashing algorithm not specified** — `UpdatePasswordAsync` accepts a hashed password, but the hashing algorithm (bcrypt, Argon2, PBKDF2) and responsibility for hashing (service vs. repository) are not defined.

5. **Confirmation email failure handling** — If `IEmailService` throws or returns a failure after the password has already been updated, the spec does not define retry behaviour or whether the overall result should surface an error.

## Changelog

| Date | Summary |
|---|---|
| 2026-05-06 | Initial population. Documented rate limiting (`IRateLimiter`), rich result enums (`RequestResetResult`), `PasswordPolicy` implementation, confirmation email on successful reset, and `IUserRepository.FindByIdAsync` addition. |
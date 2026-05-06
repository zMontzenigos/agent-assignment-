# User Story: Account Security Audit & Alerting

**As a** security officer,
**I want** all password reset attempts to be tracked and suspicious patterns flagged,
**So that** I can detect and respond to account takeover attempts before they succeed.

## Acceptance Criteria

1. Every password reset request (successful or not) is written to an audit log with: timestamp, email, outcome (sent / not found / rate limited), and source IP.
2. If 3 or more failed reset attempts are made against the same email within 10 minutes, a security alert is sent to the account owner and the security team.
3. Reset requests originating from a known blocklisted IP are rejected immediately and logged with a `blocked` outcome.
4. Audit logs are immutable — entries cannot be modified or deleted after creation.
5. A security dashboard can query all reset events for a given email within a date range.
6. Rate limit state (current count and window expiry) is observable via an admin API endpoint without exposing tokens.
7. All security events are emitted as structured log entries (JSON) compatible with the existing SIEM integration.

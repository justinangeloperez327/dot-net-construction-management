# Security Hardening

Group 22 hardens the application runtime, authentication lifecycle, sensitive endpoints, and file uploads.

## Authentication lifecycle

Interactive Server circuits revalidate Identity every 5 minutes.

Identity cookie security-stamp validation runs every 4 minutes.

User deactivation and role changes already update the security stamp, reducing the window for stale authorization.

## Identity and cookie policy

New passwords require 12 characters plus uppercase, lowercase, digit, and non-alphanumeric characters.

Lockout is 5 failed attempts for 30 minutes.

The application cookie uses:

- __Host-ConstructionManagement.Auth
- Secure = Always
- HttpOnly
- SameSite = Lax
- Path = /
- eight-hour absolute lifetime
- no sliding expiration

Open self-registration remains disabled.

## Antiforgery

Token-based antiforgery remains enabled after authentication and authorization middleware.

Login and logout POST endpoints explicitly validate the antiforgery token.

## Security headers

Responses include:

- Content-Security-Policy
- X-Content-Type-Options: nosniff
- X-Frame-Options: DENY
- Referrer-Policy: no-referrer
- restrictive Permissions-Policy
- Cross-Origin-Opener-Policy: same-origin
- Cross-Origin-Resource-Policy: same-origin
- X-Permitted-Cross-Domain-Policies: none

The CSP limits resources to self, blocks framing/objects, restricts form submissions, and permits the WebSocket connection required by Interactive Server.

## HSTS

Non-development environments use HSTS for 180 days with subdomains.

Preload is deliberately disabled until final production DNS/certificate topology is known.

## Host and forwarded-header validation

AllowedHosts defaults to:

localhost;127.0.0.1

Deployment environments must override it with real host names.

Forwarded headers are processed with ForwardLimit = 1 and only trusted proxy addresses are accepted.

Configure proxies with:

Security__ForwardedHeaders__KnownProxies__0

Never disable trusted-proxy validation merely to make forwarding work.

## Request limits

Kestrel request bodies are capped at the application file limit plus 1 MB and request headers must arrive within 15 seconds.

Form limits constrain value count, key/value length, multipart size, and multipart header size.

## Rate limiting

Authentication endpoints:

- 10 requests/minute per remote IP
- no queue

CSV export and protected file-download endpoints:

- 60 requests/minute sliding window
- partitioned by authenticated user ID, otherwise remote IP
- no queue

Rejected requests return HTTP 429.

The SignalR circuit isn't globally rate limited because that can disrupt legitimate Interactive Server traffic.

## Security events

SecurityEvents records sanitized:

- successful login
- failed login
- lockout
- logout
- rate-limit rejection

Stored metadata:

- user ID if known
- email/attempted email
- remote IP
- truncated User-Agent
- request path
- UTC timestamp

Passwords, request bodies, cookies, and authentication tokens are never stored.

Route:

/security-events

Permission:

security.events.view

## File signature validation

JPEG, PNG, WebP, and PDF uploads must pass extension, MIME, size, and magic-byte validation before storage.

This is file-type validation, not malware scanning.

## Data Protection

Application name:

ConstructionProjectManagement

Default local key path:

App_Data/keys

Production must map the Data Protection key path to durable protected storage.

## Persistence

New table:

SecurityEvents

Migration:

AddSecurityHardening

## Deferred

- MFA
- Microsoft Entra ID / OpenID Connect
- Key Vault integration
- Azure Blob key-ring storage
- WAF configuration
- malware scanning
- SIEM forwarding
- automated security-event retention
- CSP nonce generation

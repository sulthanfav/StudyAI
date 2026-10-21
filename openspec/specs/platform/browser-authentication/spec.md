# platform/browser-authentication Specification

## Purpose
Defines how the Next.js web client safely reaches the ASP.NET Core authentication API across development and production without weakening cookie or CSRF protections.

## Requirements

### Requirement: Same-origin browser routing is the default
Browser requests SHALL use a same-origin Next.js proxy path that forwards only the documented API routes to ASP.NET Core. Direct credentialed cross-origin access MUST remain disabled unless explicitly configured.

#### Scenario: Local development request
- **WHEN** the browser calls the documented API path on the Next.js development origin
- **THEN** Next.js proxies the request to the configured local ASP.NET Core origin while the browser observes one origin

#### Scenario: Production request
- **WHEN** the production web client calls an authentication API path
- **THEN** the public router presents one HTTPS origin and forwards the path without exposing an internal API origin to browser code

### Requirement: Browser cookies and antiforgery tokens survive proxying
The proxy SHALL preserve required cookie, `Set-Cookie`, antiforgery header, status, and Problem Details semantics. It MUST NOT log credentials, session cookies, antiforgery values, or request bodies.

#### Scenario: Antiforgery exchange through proxy
- **WHEN** the browser obtains a token and submits the matching header and cookie through the proxy
- **THEN** the API validates the pair and returns the original documented result

#### Scenario: Session cookie through proxy
- **WHEN** login succeeds through the proxy
- **THEN** the browser receives the secure HttpOnly session cookie with the approved attributes and can call authenticated same-origin routes

### Requirement: HTTPS and constrained forwarding
Production browser authentication SHALL use HTTPS. The proxy SHALL forward only approved methods, paths, and headers and MUST reject caller-controlled upstream destinations.

#### Scenario: Unapproved upstream or method
- **WHEN** a caller attempts to select an arbitrary upstream host or uses a method outside the route contract
- **THEN** the proxy rejects the request without contacting that destination

### Requirement: Optional direct cross-origin mode is origin-restricted
If direct browser-to-API mode is explicitly enabled, it SHALL allow credentials only for configured exact HTTPS origins and documented headers and methods. Wildcard origins MUST NOT be combined with credentials.

#### Scenario: Unlisted origin
- **WHEN** a credentialed request or preflight comes from an origin not in the configured allowlist
- **THEN** the API omits authorization for that origin and performs no state change

### Requirement: Proxy errors preserve the API contract
The web proxy SHALL preserve API status codes and safe Problem Details bodies instead of converting authentication failures into successful HTML or generic 500 responses.

#### Scenario: Authentication or rate-limit failure
- **WHEN** the API returns 401, 403, 409, 423, or 429
- **THEN** the browser receives the same status and public error contract without sensitive upstream details

# Secure Authentication with HTTP-Only Cookies

This document describes the implementation of secure refresh token handling using HTTP-only cookies and anti-forgery tokens.

## Overview

The authentication system uses a two-token approach:
1. **Access Token (JWT)**: Short-lived token returned in the API response
2. **Refresh Token**: Long-lived token stored in an HTTP-only, secure cookie

## Security Features

### 1. HTTP-Only Cookies
- Refresh tokens are stored in HTTP-only cookies, making them inaccessible to JavaScript
- Protects against XSS (Cross-Site Scripting) attacks
- Cookie settings:
  - `HttpOnly`: true
  - `Secure`: true (HTTPS only)
  - `SameSite`: Strict
  - `Path`: /

### 2. Anti-Forgery Tokens (CSRF Protection)
- Each login/register returns an anti-forgery token in the response header `X-XSRF-TOKEN`
- Client must include this token in subsequent requests to `/api/auth/refresh`
- Protects against CSRF (Cross-Site Request Forgery) attacks

### 3. Automatic Token Rotation
- Each refresh operation generates a new refresh token
- Old refresh token is invalidated
- Prevents token reuse attacks

## API Endpoints

### Login
**POST** `/api/auth/login`

**Request:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "user": {
    "userId": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-11-04T12:00:00Z"
}
```

**Response Headers:**
- `X-XSRF-TOKEN`: Anti-forgery token for refresh requests
- `Set-Cookie`: `X-Refresh-Token=...` (HTTP-only, secure)

### Register
**POST** `/api/auth/register`

**Request:**
```json
{
  "email": "user@example.com",
  "password": "password123",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:** Same as login

**Response Headers:** Same as login

### Refresh Token
**POST** `/api/auth/refresh`

**Request Headers:**
- `X-XSRF-TOKEN`: The anti-forgery token received during login/register
- `Cookie`: Automatically sent by browser (contains `X-Refresh-Token`)

**Request Body:** None (refresh token read from cookie)

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-11-04T13:00:00Z"
}
```

**Response Headers:**
- `Set-Cookie`: New `X-Refresh-Token` (HTTP-only, secure)

### Logout
**POST** `/api/auth/logout`

**Response:**
```json
{
  "message": "Logged out successfully"
}
```

**Response Headers:**
- `Set-Cookie`: Clears the `X-Refresh-Token` cookie

## Configuration

### appsettings.json

```json
{
  "CookieSettings": {
    "RefreshTokenCookieName": "X-Refresh-Token",
    "AntiForgeryTokenHeaderName": "X-XSRF-TOKEN",
    "SecureOnly": true,
    "SameSiteMode": "Strict"
  },
  "JwtSettings": {
    "TokenExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

## Security Best Practices

1. **Always use HTTPS in production**: Cookie `Secure` flag requires HTTPS
2. **Store access tokens in memory**: Avoid localStorage for access tokens if possible
3. **Implement token refresh before expiry**: Refresh 5 minutes before access token expires
4. **Clear tokens on logout**: Always clear both cookies and client-side storage
5. **Validate anti-forgery tokens**: Always send `X-XSRF-TOKEN` header in refresh requests
6. **Use SameSite=Strict**: Prevents cookies from being sent in cross-site requests
7. **Monitor for token reuse**: Server should invalidate old refresh tokens after rotation

## Troubleshooting

### CORS Issues
Ensure your CORS policy allows credentials:
```csharp
builder.WithOrigins("https://your-frontend.com")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials(); // Required for cookies
```

### Anti-Forgery Validation Failures
- Ensure you're sending the `X-XSRF-TOKEN` header
- Token must match the one received during login/register
- Check that cookies are being sent (`credentials: 'include'`)

### Cookies Not Being Set
- Verify HTTPS is enabled (Secure flag requires HTTPS)
- Check SameSite settings match your domain setup
- Ensure `credentials: 'include'` is set in fetch/axios

### Token Refresh Loops
- Implement exponential backoff for failed refreshes
- Redirect to login after max retry attempts
- Check that new tokens are being stored correctly

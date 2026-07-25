# JWT Authentication Deep Dive

## How JWT Works

```
┌────────────────────────────────────────────────────────────────┐
│                    JWT STRUCTURE                                │
└────────────────────────────────────────────────────────────────┘

eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkFobWVkIiwiaWF0IjoxNTE2MjM5MDIyfQ.
SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c

     ↓                      ↓                      ↓
  HEADER              PAYLOAD                SIGNATURE
  (Algorithm)         (Claims/Data)          (Verification)
```

---

## JWT Components

### Header
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

### Payload (Claims)
```json
{
  "sub": "1234567890",
  "name": "Ahmed",
  "email": "ahmed@example.com",
  "role": "Admin",
  "iat": 1516239022,
  "exp": 1516242622
}
```

### Signature
```
HMACSHA256(
  base64UrlEncode(header) + "." +
  base64UrlEncode(payload),
  secret
)
```

---

## JWT in ASP.NET Core

### Step 1: Setup in Program.cs

```csharp
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])
            )
        };
    });

app.UseAuthentication();
app.UseAuthorization();
```

### Step 2: appsettings.json

```json
{
  "Jwt": {
    "SecretKey": "your-very-secret-key-min-32-characters-long",
    "Issuer": "your-app",
    "Audience": "your-app-users",
    "ExpirationMinutes": 60
  }
}
```

### Step 3: Generate JWT Token

```csharp
public class TokenService
{
    private readonly IConfiguration _config;
    
    public string GenerateToken(User user)
    {
        var secretKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(_config["Jwt:ExpirationMinutes"])),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Step 4: Use in Login Endpoint

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly TokenService _tokenService;
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userService.GetUserByEmailAsync(request.Email);
        
        if (user == null || !user.VerifyPassword(request.Password))
            return Unauthorized("Invalid credentials");
        
        var token = _tokenService.GenerateToken(user);
        
        return Ok(new { token });
    }
}
```

### Step 5: Use Token in Request

```
GET /api/users/profile HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Step 6: Protect Endpoint

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require JWT token
public class UserController : ControllerBase
{
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(new { userId });
    }
    
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")] // Require Admin role
    public IActionResult AdminOnly()
    {
        return Ok("Admin access granted");
    }
}
```

---

## Access Token vs Refresh Token

### Single Token Issue
```
Login → Get Token → Token expires after 60 minutes
         ↓
User re-enters password (bad UX!)
```

### With Refresh Token
```
Login → Get AccessToken (15 min) + RefreshToken (7 days)
         ↓
AccessToken expires
         ↓
Send RefreshToken → Get new AccessToken
         ↓
User continues seamlessly
```

### Implementation

```csharp
public class TokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
}

public class TokenService
{
    public TokenResponse GenerateTokens(User user)
    {
        // Short-lived access token
        var accessToken = GenerateAccessToken(user); // 15 minutes
        
        // Long-lived refresh token
        var refreshToken = GenerateRefreshToken(user); // 7 days
        
        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 15 * 60 // 15 minutes in seconds
        };
    }
    
    private string GenerateRefreshToken(User user)
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var user = await _userService.ValidateRefreshTokenAsync(request.RefreshToken);
        
        if (user == null)
            return Unauthorized("Invalid refresh token");
        
        var tokens = _tokenService.GenerateTokens(user);
        return Ok(tokens);
    }
}
```

---

## Claims, Role, Policy

### Claims (User Information)

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, "123"),
    new Claim(ClaimTypes.Email, "user@example.com"),
    new Claim(ClaimTypes.Role, "Admin"),
    new Claim("Department", "Engineering"),
    new Claim("Level", "Senior")
};
```

### Accessing Claims

```csharp
public IActionResult GetUserInfo()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var email = User.FindFirst(ClaimTypes.Email)?.Value;
    var department = User.FindFirst("Department")?.Value;
    
    return Ok(new { userId, email, department });
}
```

### Policy-Based Authorization

```csharp
// In Program.cs
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("SeniorOnly", policy =>
        policy.RequireClaim("Level", "Senior", "Lead"));
        
    options.AddPolicy("EngineeringDept", policy =>
        policy.RequireClaim("Department", "Engineering"));
});

// In Controller
[HttpGet("senior-features")]
[Authorize(Policy = "SeniorOnly")]
public IActionResult SeniorFeatures()
{
    return Ok("Senior-only features");
}
```

---

## Why Bearer Token?

```
Authorization: Bearer <token>
               ↑
         "Bearer" indicates token-based authentication
         
Examples:
- Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
- Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Interview Q&A

**Q: Is JWT secure? Can I modify it?**

A: JWT is signed but NOT encrypted:
```
Can decode:  echo "eyJh..." | base64 --decode
Cannot modify: Changes invalidate signature
Cannot decrypt: Payload is readable

Solution: Use HTTPS to encrypt in transit
```

**Q: Where to store JWT token in client?**

A:
```
❌ localStorage - XSS vulnerable
✅ httpOnly cookie - XSS safe, automatic with requests
✅ Memory - Lost on refresh, but safest

Best: httpOnly cookie with automatic refresh
```

**Q: What's exp and iat claims?**

A:
```json
{
  "iat": 1516239022,    // Issued at timestamp
  "exp": 1516242622     // Expiration timestamp
}
```
Server validates: `current_time < exp`

**Q: Refresh token vs Access token expiry?**

A:
- Access token: 15 minutes (short, frequent refresh)
- Refresh token: 7 days (long, rarely needs re-login)
- Trade-off: Security vs UX

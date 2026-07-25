# Authentication vs Authorization

## Authentication (Who are you?)

**Definition:** Verifying identity

```csharp
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class AuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // 1. Find user
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return new LoginResponse { Success = false }; // User not found
        
        // 2. Verify password
        if (!user.VerifyPassword(request.Password))
            return new LoginResponse { Success = false }; // Wrong password
        
        // 3. Issue token (authenticated)
        var token = _tokenService.GenerateToken(user);
        
        return new LoginResponse
        {
            Success = true,
            Token = token
        };
    }
}
```

---

## Authorization (What can you do?)

**Definition:** Checking permissions

```csharp
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    // Require JWT token (authenticated)
    [Authorize]
    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
        return Ok("Authenticated users only");
    }
    
    // Require Admin role (authorized)
    [Authorize(Roles = "Admin")]
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteAsync(id);
        return Ok();
    }
    
    // Policy-based authorization
    [Authorize(Policy = "SeniorOrManager")]
    [HttpGet("reports")]
    public IActionResult ViewReports()
    {
        return Ok("Senior or Manager only");
    }
}

// Program.cs - Define policy
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("SeniorOrManager", policy =>
        policy.RequireRole("Senior", "Manager")
    );
});
```

---

## Difference Example

```
User Scenario:

1. Ahmed enters email & password
   ↓
   AuthService verifies credentials
   ↓
   Ahmed AUTHENTICATED ✅ (proven identity)

2. Ahmed requests DELETE /users/5
   ↓
   AuthService checks role
   ↓
   Ahmed is "User" role, not "Admin"
   ↓
   Ahmed NOT AUTHORIZED ❌ (no permission)
```

---

## OAuth2 vs JWT

### OAuth2 (Third-party login)

```
User → Your App → Google
                   ↓
                 (user logs in)
                   ↓
Your App ← Google (auth code)
↓
User authenticated via Google
```

### JWT (Token-based)

```
User → Your App → Auth endpoint
                   ↓
                 (verify credentials)
                   ↓
Your App ← Token (JWT)
↓
User uses token for subsequent requests
```

---

## Interview Q&A

**Q: Authentication vs Authorization?**

A:
- Authentication: "Who are you?" (login, verify password)
- Authorization: "What can you do?" (check role/permissions)

**Q: OAuth2 vs JWT?**

A:
- OAuth2: Delegate login to provider (Google, Microsoft)
- JWT: Self-contained token with claims

**Q: Role-based vs Policy-based authorization?**

A:
- Role-based: Simple, user has "Admin" role
- Policy-based: Complex, multiple conditions and claims

# REST API Best Practices

## HTTP Verbs

```csharp
// GET - Retrieve (Safe, Idempotent)
[HttpGet("api/users/{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var user = await _userService.GetUserAsync(id);
    return Ok(user);
}

// POST - Create (Not idempotent)
[HttpPost("api/users")]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    var user = await _userService.CreateUserAsync(request);
    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
}

// PUT - Replace entire resource (Idempotent)
[HttpPut("api/users/{id}")]
public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
{
    var user = await _userService.UpdateUserAsync(id, request);
    return Ok(user);
}

// PATCH - Partial update (Idempotent)
[HttpPatch("api/users/{id}")]
public async Task<IActionResult> PartialUpdateUser(int id, [FromBody] JsonPatchDocument<User> patch)
{
    var user = await _userService.GetUserAsync(id);
    patch.ApplyTo(user);
    await _userService.UpdateUserAsync(id, user);
    return Ok(user);
}

// DELETE - Remove resource (Idempotent)
[HttpDelete("api/users/{id}")]
public async Task<IActionResult> DeleteUser(int id)
{
    await _userService.DeleteUserAsync(id);
    return NoContent();
}
```

---

## Status Codes

| Code | Meaning | Use Case |
|------|---------|----------|
| 200 | OK | Successful GET/PUT/PATCH |
| 201 | Created | Successful POST (resource created) |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Invalid input (validation failed) |
| 401 | Unauthorized | Missing/invalid authentication |
| 403 | Forbidden | Authenticated but no permission |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Business logic violation |
| 500 | Internal Server Error | Server error |

```csharp
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    // Validate input
    if (!ModelState.IsValid)
        return BadRequest(ModelState); // 400
    
    // Check if user exists
    var existing = await _userService.GetByEmailAsync(request.Email);
    if (existing != null)
        return Conflict("Email already exists"); // 409
    
    var user = await _userService.CreateUserAsync(request);
    
    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user); // 201
}
```

---

## Validation

```csharp
public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [MinLength(8)]
    public string Password { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
}

// Custom validator
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

---

## Pagination, Filtering, Sorting

```csharp
[HttpGet("api/users")]
public async Task<IActionResult> GetUsers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string search = null,
    [FromQuery] string sortBy = "createdAt",
    [FromQuery] string sortOrder = "desc")
{
    var result = await _userService.GetUsersAsync(
        page: page,
        pageSize: pageSize,
        search: search,
        sortBy: sortBy,
        sortOrder: sortOrder
    );
    
    return Ok(result);
}

// Response
{
    "data": [
        { "id": 1, "name": "Ahmed", "email": "ahmed@example.com" },
        { "id": 2, "name": "Ali", "email": "ali@example.com" }
    ],
    "total": 100,
    "page": 1,
    "pageSize": 10,
    "totalPages": 10
}
```

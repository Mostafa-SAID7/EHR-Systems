namespace Identity.Domain.Entities;

using Identity.Domain.Enums;
using Identity.Domain.ValueObjects;

/// <summary>
/// Entity representing a user in the system
/// </summary>
public sealed class User : AggregateRoot<Guid>
{
    private readonly List<UserRole> _roles = new();

    /// <summary>
    /// Initializes a new instance of the User class
    /// </summary>
    private User(
        Guid id,
        Email email,
        string firstName,
        string lastName,
        Password passwordHash,
        UserStatus status)
        : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        IsEmailVerified = false;
        FailedLoginAttempts = 0;
        LastLoginAt = null;
    }

    /// <summary>
    /// Gets the user email
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Gets the user first name
    /// </summary>
    public string FirstName { get; private set; }

    /// <summary>
    /// Gets the user last name
    /// </summary>
    public string LastName { get; private set; }

    /// <summary>
    /// Gets the password hash
    /// </summary>
    public Password PasswordHash { get; private set; }

    /// <summary>
    /// Gets the user account status
    /// </summary>
    public UserStatus Status { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the email is verified
    /// </summary>
    public bool IsEmailVerified { get; private set; }

    /// <summary>
    /// Gets the number of failed login attempts
    /// </summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>
    /// Gets the last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// Gets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the last modification timestamp
    /// </summary>
    public DateTime? ModifiedAt { get; private set; }

    /// <summary>
    /// Gets the user roles
    /// </summary>
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="email">The user email</param>
    /// <param name="firstName">The user first name</param>
    /// <param name="lastName">The user last name</param>
    /// <param name="passwordHash">The password hash</param>
    /// <returns>A new User instance</returns>
    public static User Create(string email, string firstName, string lastName, string passwordHash)
    {
        var emailVo = new Email(email);
        var passwordVo = new Password(passwordHash);

        var user = new User(
            Guid.NewGuid(),
            emailVo,
            firstName,
            lastName,
            passwordVo,
            UserStatus.PendingEmailVerification);

        user.RaiseDomainEvent(new UserCreatedEvent(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAt));

        return user;
    }

    /// <summary>
    /// Changes the user password
    /// </summary>
    /// <param name="newPasswordHash">The new password hash</param>
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = new Password(newPasswordHash);
        ModifiedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserPasswordChangedEvent(Id, Email, ModifiedAt.Value));
    }

    /// <summary>
    /// Records a successful login
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LastLoginAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;

        if (Status == UserStatus.LockedOut)
        {
            Status = UserStatus.Active;
        }

        RaiseDomainEvent(new UserLoggedInEvent(Id, Email, LastLoginAt.Value));
    }

    /// <summary>
    /// Records a failed login attempt
    /// </summary>
    /// <param name="maxFailedAttempts">The maximum number of failed attempts before lockout</param>
    public void RecordFailedLoginAttempt(int maxFailedAttempts = 5)
    {
        FailedLoginAttempts++;
        ModifiedAt = DateTime.UtcNow;

        if (FailedLoginAttempts >= maxFailedAttempts)
        {
            Status = UserStatus.LockedOut;
        }
    }

    /// <summary>
    /// Verifies the user email
    /// </summary>
    public void VerifyEmail()
    {
        IsEmailVerified = true;
        if (Status == UserStatus.PendingEmailVerification)
        {
            Status = UserStatus.Active;
        }
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Suspends the user account
    /// </summary>
    public void Suspend()
    {
        if (Status != UserStatus.Suspended)
        {
            Status = UserStatus.Suspended;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Reactivates the user account
    /// </summary>
    public void Reactivate()
    {
        if (Status == UserStatus.Suspended || Status == UserStatus.LockedOut)
        {
            Status = UserStatus.Active;
            FailedLoginAttempts = 0;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Disables the user account
    /// </summary>
    public void Disable()
    {
        Status = UserStatus.Disabled;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a role to the user
    /// </summary>
    /// <param name="role">The role to add</param>
    public void AddRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (_roles.Any(r => r.RoleId == role.Id))
            return;

        var userRole = new UserRole(Id, role.Id);
        _roles.Add(userRole);
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a role from the user
    /// </summary>
    /// <param name="roleId">The role ID to remove</param>
    public void RemoveRole(Guid roleId)
    {
        var role = _roles.FirstOrDefault(r => r.RoleId == roleId);
        if (role != null)
        {
            _roles.Remove(role);
            ModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the user is active
    /// </summary>
    public bool IsActive => Status == UserStatus.Active;
}

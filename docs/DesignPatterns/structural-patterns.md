# Structural Design Patterns in C#

## Decorator — Add Behavior Without Modifying Existing Code
```csharp
public interface ICodingService { Task<string[]> SuggestCodesAsync(string notes); }

public class BaseCodingService : ICodingService
{
    public Task<string[]> SuggestCodesAsync(string notes) => Task.FromResult(new[] { "E11.9" });
}

// Decorator adds audit logging transparently
public class AuditLoggingCodingDecorator : ICodingService
{
    private readonly ICodingService _inner;
    private readonly ILogger<AuditLoggingCodingDecorator> _logger;

    public AuditLoggingCodingDecorator(ICodingService inner, ILogger<AuditLoggingCodingDecorator> logger)
    { _inner = inner; _logger = logger; }

    public async Task<string[]> SuggestCodesAsync(string notes)
    {
        _logger.LogInformation("Suggesting codes for notes length={Length}", notes.Length);
        var result = await _inner.SuggestCodesAsync(notes);
        _logger.LogInformation("Suggested {Count} codes", result.Length);
        return result;
    }
}
```

## Facade — Simplify Complex Subsystems
```csharp
public class BillingFacade
{
    private readonly ICodingService _coding;
    private readonly IClaimSubmissionService _claims;
    private readonly IAuditService _audit;

    public BillingFacade(ICodingService coding, IClaimSubmissionService claims, IAuditService audit)
    { _coding = coding; _claims = claims; _audit = audit; }

    public async Task ProcessVisitAsync(VisitRecord visit)
    {
        var codes = await _coding.SuggestCodesAsync(visit.Notes);
        var claim = await _claims.SubmitAsync(visit.PatientId, codes);
        await _audit.RecordAsync(visit.Id, codes, claim.Id);
    }
}
```

## Proxy — Control Access (Lazy Loading / Auth Guard)
```csharp
public class AuthorizationProxy : ICodingService
{
    private readonly ICodingService _inner;
    private readonly ICurrentUserService _user;

    public AuthorizationProxy(ICodingService inner, ICurrentUserService user)
    { _inner = inner; _user = user; }

    public async Task<string[]> SuggestCodesAsync(string notes)
    {
        if (!_user.HasPermission("coding:suggest"))
            throw new UnauthorizedAccessException("Missing permission: coding:suggest");
        return await _inner.SuggestCodesAsync(notes);
    }
}
```

## Adapter — Bridge Incompatible Interfaces
```csharp
// Legacy insurance API returns XML; new system expects JSON DTO
public class LegacyInsuranceAdapter : IInsuranceGateway
{
    private readonly LegacyXmlInsuranceClient _legacyClient;

    public LegacyInsuranceAdapter(LegacyXmlInsuranceClient legacyClient)
    { _legacyClient = legacyClient; }

    public async Task<ClaimResult> SubmitClaimAsync(Claim claim)
    {
        var xmlPayload = XmlSerializer.Serialize(claim);
        var xmlResponse = await _legacyClient.PostXmlAsync(xmlPayload);
        return XmlDeserializer.Deserialize<ClaimResult>(xmlResponse);
    }
}
```

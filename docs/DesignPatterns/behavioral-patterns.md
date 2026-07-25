# Behavioral Design Patterns in C#

## Strategy — Swap Algorithms at Runtime
```csharp
public interface ICodingStrategy { string[] Suggest(string notes); }

public class AiModelStrategy : ICodingStrategy
{
    public string[] Suggest(string notes) => new[] { "E11.9", "99213" }; // ML model
}

public class RuleBasedStrategy : ICodingStrategy
{
    public string[] Suggest(string notes) => new[] { "Z00.00" }; // fallback rules
}

public class CodingContext
{
    private ICodingStrategy _strategy;

    public CodingContext(ICodingStrategy strategy) { _strategy = strategy; }
    public void SetStrategy(ICodingStrategy strategy) { _strategy = strategy; }
    public string[] Execute(string notes) => _strategy.Suggest(notes);
}

// Usage — switch to fallback when AI model is down
var context = new CodingContext(new AiModelStrategy());
if (aiModelDown) context.SetStrategy(new RuleBasedStrategy());
var codes = context.Execute(visitNotes);
```

## Observer — Notify Multiple Subscribers on State Change
```csharp
public delegate void ClaimStatusChangedHandler(int claimId, ClaimStatus newStatus);

public class ClaimService
{
    public event ClaimStatusChangedHandler? OnStatusChanged;

    public async Task UpdateStatusAsync(int claimId, ClaimStatus status)
    {
        // update DB ...
        OnStatusChanged?.Invoke(claimId, status);
    }
}

// Subscribers
claimService.OnStatusChanged += (id, status) => auditService.LogAsync(id, status);
claimService.OnStatusChanged += (id, status) => notificationService.NotifyAsync(id, status);
```

## Mediator (MediatR) — Decouple Handlers
```csharp
// Command
public record SubmitClaimCommand(int PatientId, string[] Codes) : IRequest<ClaimResult>;

// Handler — isolated, no direct coupling to controller
public class SubmitClaimHandler : IRequestHandler<SubmitClaimCommand, ClaimResult>
{
    private readonly IClaimRepository _repo;
    public SubmitClaimHandler(IClaimRepository repo) { _repo = repo; }

    public async Task<ClaimResult> Handle(SubmitClaimCommand cmd, CancellationToken ct)
    {
        var claim = new Claim(cmd.PatientId, cmd.Codes);
        await _repo.SaveAsync(claim, ct);
        return new ClaimResult(claim.Id, ClaimStatus.Submitted);
    }
}

// Controller — only knows about MediatR, not the handler
[HttpPost]
public async Task<IActionResult> Submit([FromBody] SubmitClaimCommand cmd)
    => Ok(await _mediator.Send(cmd));
```

## Chain of Responsibility — Validation Pipeline
```csharp
public abstract class ValidationHandler
{
    protected ValidationHandler? _next;
    public ValidationHandler SetNext(ValidationHandler next) { _next = next; return next; }

    public abstract Task<bool> HandleAsync(VisitRecord visit);
}

public class PatientExistsHandler : ValidationHandler
{
    public override async Task<bool> HandleAsync(VisitRecord visit)
    {
        if (visit.PatientId <= 0) return false;
        return _next == null || await _next.HandleAsync(visit);
    }
}

public class NotesNotEmptyHandler : ValidationHandler
{
    public override async Task<bool> HandleAsync(VisitRecord visit)
    {
        if (string.IsNullOrWhiteSpace(visit.Notes)) return false;
        return _next == null || await _next.HandleAsync(visit);
    }
}

// Compose chain
var chain = new PatientExistsHandler();
chain.SetNext(new NotesNotEmptyHandler());
bool valid = await chain.HandleAsync(visit);
```

## Command — Encapsulate Requests as Objects
```csharp
public interface ICommand { Task ExecuteAsync(); Task UndoAsync(); }

public class AssignCodeCommand : ICommand
{
    private readonly IVisitRepository _repo;
    private readonly int _visitId;
    private readonly string _code;

    public AssignCodeCommand(IVisitRepository repo, int visitId, string code)
    { _repo = repo; _visitId = visitId; _code = code; }

    public async Task ExecuteAsync() => await _repo.AssignCodeAsync(_visitId, _code);
    public async Task UndoAsync()    => await _repo.RemoveCodeAsync(_visitId, _code);
}
```

## Template Method — Define Algorithm Skeleton
```csharp
public abstract class ClaimProcessor
{
    // Template method — defines the steps
    public async Task ProcessAsync(Claim claim)
    {
        await ValidateAsync(claim);
        await EnrichAsync(claim);
        await SubmitAsync(claim);
        await AuditAsync(claim);
    }

    protected abstract Task ValidateAsync(Claim claim);
    protected abstract Task EnrichAsync(Claim claim);
    protected abstract Task SubmitAsync(Claim claim);

    protected virtual Task AuditAsync(Claim claim)
    {
        Console.WriteLine($"Claim {claim.Id} processed.");
        return Task.CompletedTask;
    }
}
```

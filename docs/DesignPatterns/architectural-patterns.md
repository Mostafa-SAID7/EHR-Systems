# Architectural Patterns — DI, Specification & CQRS

## Specification Pattern — Encapsulate Business Rules
```csharp
public interface ISpecification<T> { bool IsSatisfiedBy(T entity); }

public class ActivePatientSpecification : ISpecification<Patient>
{
    public bool IsSatisfiedBy(Patient p) => p.Status == PatientStatus.Active && !p.IsArchived;
}

public class EligibleForCodingSpecification : ISpecification<VisitRecord>
{
    public bool IsSatisfiedBy(VisitRecord v) =>
        !string.IsNullOrWhiteSpace(v.Notes) && v.AssignedCodes.Count == 0;
}

// Composite: AND, OR, NOT
public class AndSpecification<T> : ISpecification<T>
{
    private readonly ISpecification<T> _left, _right;
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    { _left = left; _right = right; }

    public bool IsSatisfiedBy(T entity) =>
        _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);
}
```

## CQRS — Separate Read and Write Models
```csharp
// Command side: normalized write model
public record CreateVisitCommand(int PatientId, string Notes) : IRequest<int>;

public class CreateVisitHandler : IRequestHandler<CreateVisitCommand, int>
{
    private readonly IVisitRepository _repo;
    public CreateVisitHandler(IVisitRepository repo) { _repo = repo; }

    public async Task<int> Handle(CreateVisitCommand cmd, CancellationToken ct)
    {
        var visit = new VisitRecord { PatientId = cmd.PatientId, Notes = cmd.Notes };
        await _repo.AddAsync(visit, ct);
        return visit.Id;
    }
}

// Query side: optimized read model (direct SQL via Dapper for performance)
public record GetVisitSummaryQuery(int VisitId) : IRequest<VisitSummaryDto>;

public class GetVisitSummaryHandler : IRequestHandler<GetVisitSummaryQuery, VisitSummaryDto>
{
    private readonly IDbConnection _db;
    public GetVisitSummaryHandler(IDbConnection db) { _db = db; }

    public async Task<VisitSummaryDto> Handle(GetVisitSummaryQuery q, CancellationToken ct)
        => await _db.QuerySingleOrDefaultAsync<VisitSummaryDto>(
            "SELECT v.Id, p.Name, v.Notes, COUNT(c.Id) AS CodeCount FROM Visits v " +
            "JOIN Patients p ON p.Id = v.PatientId " +
            "LEFT JOIN AssignedCodes c ON c.VisitId = v.Id WHERE v.Id = @Id GROUP BY v.Id, p.Name, v.Notes",
            new { Id = q.VisitId });
}
```

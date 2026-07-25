# Creational Design Patterns in C#

## Singleton
```csharp
public sealed class CodingRuleEngine
{
    private static readonly Lazy<CodingRuleEngine> _instance =
        new(() => new CodingRuleEngine());

    public static CodingRuleEngine Instance => _instance.Value;
    private CodingRuleEngine() { }
}
```

## Factory Method
```csharp
public interface INotificationSender { Task SendAsync(string message); }
public class EmailSender : INotificationSender { public Task SendAsync(string msg) => Task.CompletedTask; }
public class SmsSender : INotificationSender { public Task SendAsync(string msg) => Task.CompletedTask; }

public static class NotificationFactory
{
    public static INotificationSender Create(string channel) => channel switch
    {
        "email" => new EmailSender(),
        "sms"   => new SmsSender(),
        _ => throw new ArgumentException($"Unknown channel: {channel}")
    };
}
```

## Builder
```csharp
public class MedicalReportBuilder
{
    private readonly MedicalReport _report = new();

    public MedicalReportBuilder WithPatient(int patientId) { _report.PatientId = patientId; return this; }
    public MedicalReportBuilder WithDiagnosis(string icd10Code) { _report.IcdCode = icd10Code; return this; }
    public MedicalReportBuilder WithProcedure(string cptCode) { _report.CptCode = cptCode; return this; }
    public MedicalReport Build() => _report;
}

// Usage
var report = new MedicalReportBuilder()
    .WithPatient(42)
    .WithDiagnosis("E11.9")
    .WithProcedure("99213")
    .Build();
```

## Object Pool (ArrayPool)
```csharp
// Reuse byte[] buffers instead of allocating new arrays on Large Object Heap
var pool = ArrayPool<byte>.Shared;
byte[] buffer = pool.Rent(4096);
try { /* use buffer */ }
finally { pool.Return(buffer); }
```

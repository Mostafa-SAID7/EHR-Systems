# Performance Testing

Performance tests for the EHR Platform using BenchmarkDotNet and NBomber.

## Test Categories

### Load Testing (`Load/`)
Concurrent user simulation and throughput analysis:
- Peak load scenarios
- Sustained load testing
- Gradual ramp-up testing
- Multi-user coordination

### Stress Testing (`Stress/`)
System limits and extreme conditions:
- Memory pressure
- Connection pool exhaustion
- Database resource limits
- Recovery from failure
- System stability verification

### Benchmarks (`Benchmark/`)
Performance benchmarks for critical code paths:
- Query performance
- Business logic execution
- Serialization/deserialization
- Caching effectiveness
- API endpoint latency

## Running Performance Tests

```bash
# Run all performance tests
dotnet test tests/EHRPlatform.Tests.Performance/EHRPlatform.Tests.Performance.csproj

# Run specific benchmark
dotnet test tests/EHRPlatform.Tests.Performance/EHRPlatform.Tests.Performance.csproj --filter "FullyQualifiedName~PatientQueryBenchmarks"

# Run with verbose output
dotnet test tests/EHRPlatform.Tests.Performance/EHRPlatform.Tests.Performance.csproj -v detailed
```

## Performance Targets

| Scenario | Target | Notes |
|----------|--------|-------|
| API Response | <100ms | 95th percentile |
| Database Query | <50ms | Single record lookup |
| Cache Hit | <1ms | Redis lookup |
| Report Generation | <5s | Full report |
| Bulk Import | <1000 records/s | With validation |

## Metrics Collected

- **Response Time**: Min, Max, Mean, P50, P95, P99
- **Throughput**: Requests per second
- **Resource Usage**: CPU, Memory, Thread count
- **Error Rate**: Failed requests percentage
- **Availability**: Uptime percentage

## Example Performance Test

```csharp
[MemoryDiagnoser]
public class PatientQueryBenchmarks
{
    private IPatientService _service;

    [GlobalSetup]
    public void Setup()
    {
        _service = new PatientService();
    }

    [Benchmark]
    public async Task GetPatientById()
    {
        await _service.GetPatientByIdAsync("patient-id-123");
    }

    [Benchmark]
    public async Task ListPatients()
    {
        await _service.ListPatientsAsync(0, 100);
    }
}
```

## Integration with CI/CD

Performance tests run on:
- Nightly builds (full stress testing)
- Release builds (baseline establishment)
- On-demand (performance regression detection)

Results are tracked and compared against baselines.

## Performance Analysis Tools

- BenchmarkDotNet: Code-level performance measurement
- NBomber: Load and stress testing at API level
- Application Insights: Production performance monitoring

## Tips for Writing Good Performance Tests

1. **Realistic Scenarios**: Test with real-world data patterns
2. **Isolation**: Ensure tests don't interfere with each other
3. **Measurement**: Warm up benchmarks before measurement
4. **Comparison**: Compare against baseline metrics
5. **Documentation**: Record why targets were set

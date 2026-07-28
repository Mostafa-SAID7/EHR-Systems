#nullable enable

using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EHRPlatform.Tests.Common.Builders;
using EHRPlatform.Tests.Common.Helpers;

namespace EHRPlatform.Tests.Performance.Benchmark;

/// <summary>
/// Performance benchmarks for critical PatientService operations using BenchmarkDotNet.
/// Measures memory allocation, execution time, and throughput.
/// Target: <10ms for single operations, minimal allocations.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
[Config(typeof(BenchmarkConfig))]
public class PatientServiceBenchmarks
{
    private PatientBuilder _patientBuilder = null!;

    [GlobalSetup]
    public void Setup()
    {
        _patientBuilder = new PatientBuilder();
    }

    [Benchmark]
    public void CreatePatientBuilder()
    {
        var patient = new PatientBuilder()
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmail("john@test.com")
            .Build();
    }

    [Benchmark]
    public void BuildPatientFromBuilder()
    {
        var patient = _patientBuilder
            .WithFirstName("Jane")
            .WithLastName("Smith")
            .Build();
    }

    [Benchmark]
    public void GenerateSyntheticPatientData()
    {
        var (firstName, lastName) = TestDataGenerator.GenerateName();
        var email = TestDataGenerator.GenerateEmail();
        var phone = TestDataGenerator.GeneratePhoneNumber();
        var mrn = TestDataGenerator.GenerateMRN();
        var dob = TestDataGenerator.GenerateDateOfBirth();
    }

    [Benchmark]
    public string GeneratePatientEmail()
    {
        return TestDataGenerator.GenerateEmail();
    }

    [Benchmark]
    public string GeneratePhoneNumber()
    {
        return TestDataGenerator.GeneratePhoneNumber();
    }

    [Benchmark]
    public string GenerateMRN()
    {
        return TestDataGenerator.GenerateMRN();
    }

    [Benchmark]
    public Guid GenerateId()
    {
        return TestDataGenerator.GenerateId();
    }

    [Benchmark]
    public (string FirstName, string LastName) GenerateName()
    {
        return TestDataGenerator.GenerateName();
    }

    [Benchmark]
    public DateTime GenerateDateOfBirth()
    {
        return TestDataGenerator.GenerateDateOfBirth();
    }

    [Benchmark]
    public (string Street, string City, string State, string ZipCode) GenerateAddress()
    {
        return TestDataGenerator.GenerateAddress();
    }

    [Benchmark]
    public string GeneratePassword()
    {
        return TestDataGenerator.GeneratePassword();
    }

    [Benchmark]
    public string GenerateJwtToken()
    {
        return MockHelper.GenerateJwtToken(
            userId: Guid.NewGuid().ToString(),
            email: TestDataGenerator.GenerateEmail(),
            roles: new[] { "User" }
        );
    }

    [Benchmark]
    public bool IsPhiField()
    {
        return HipaaComplianceHelper.IsPHIField("phone");
    }

    [Benchmark]
    public string MaskPHI()
    {
        return HipaaComplianceHelper.MaskPHI("1234567890");
    }

    [Benchmark]
    public (byte[] Key, byte[] IV) GenerateEncryptionKeyPair()
    {
        return HipaaComplianceHelper.GenerateEncryptionKeyPair();
    }

    [Benchmark]
    public void ValidateEmail()
    {
        var email = TestDataGenerator.GenerateEmail();
        var isValid = email.Contains("@") && email.Contains(".");
    }

    [Benchmark]
    public void ValidatePhone()
    {
        var phone = TestDataGenerator.GeneratePhoneNumber();
        var isValid = phone.StartsWith("+") && phone.Length >= 10;
    }

    [Benchmark]
    public void ValidateMRN()
    {
        var mrn = TestDataGenerator.GenerateMRN();
        var isValid = System.Text.RegularExpressions.Regex.IsMatch(mrn, @"^\d{6}-\d{3}$");
    }
}

/// <summary>
/// Benchmark configuration for consistent test results.
/// </summary>
public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddDiagnoser(new BenchmarkDotNet.Diagnostics.MemoryDiagnoser());
    }
}

/// <summary>
/// Query benchmarks for database operations.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
public class QueryBenchmarks
{
    [Benchmark]
    public void SimpleStringSearch()
    {
        var query = "SELECT * FROM Patients WHERE Email = @email";
        var result = !string.IsNullOrEmpty(query);
    }

    [Benchmark]
    public void PatternMatching()
    {
        var email = "test@test.com";
        var isMatch = System.Text.RegularExpressions.Regex.IsMatch(
            email,
            @"^[^\s@]+@[^\s@]+\.[^\s@]+$"
        );
    }

    [Benchmark]
    public void DateComparison()
    {
        var dob = DateTime.Now.AddYears(-40);
        var age = DateTime.Now.Year - dob.Year;
    }

    [Benchmark]
    public void DictionaryLookup()
    {
        var dict = new System.Collections.Generic.Dictionary<string, object>
        {
            { "patient_id", Guid.NewGuid() },
            { "email", "test@test.com" }
        };

        var found = dict.TryGetValue("patient_id", out var value);
    }
}

/// <summary>
/// Encryption performance benchmarks.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
public class EncryptionBenchmarks
{
    private byte[]? _key;
    private byte[]? _iv;
    private string _plainText = "Patient data to encrypt";

    [GlobalSetup]
    public void Setup()
    {
        (_key, _iv) = HipaaComplianceHelper.GenerateEncryptionKeyPair();
    }

    [Benchmark]
    public byte[] EncryptData()
    {
        return HipaaComplianceHelper.EncryptPHI(_plainText, _key!, _iv!);
    }

    [Benchmark]
    public string DecryptData()
    {
        var encrypted = HipaaComplianceHelper.EncryptPHI(_plainText, _key!, _iv!);
        return HipaaComplianceHelper.DecryptPHI(encrypted, _key!, _iv!);
    }

    [Benchmark]
    public string MaskData()
    {
        return HipaaComplianceHelper.MaskPHI("1234567890");
    }
}

/// <summary>
/// Helper class to run benchmarks.
/// </summary>
public class BenchmarkRunner
{
    public static void RunBenchmarks()
    {
        var summary = BenchmarkRunner.Run<PatientServiceBenchmarks>();
        var querySummary = BenchmarkRunner.Run<QueryBenchmarks>();
        var encryptSummary = BenchmarkRunner.Run<EncryptionBenchmarks>();
    }
}

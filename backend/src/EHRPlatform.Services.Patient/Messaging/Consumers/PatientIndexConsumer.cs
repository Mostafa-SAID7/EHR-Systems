using EHRPlatform.BuildingBlocks.Common.Search;
using EHRPlatform.BuildingBlocks.Observability.Telemetry;
using EHRPlatform.Services.Patient.Messaging.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Messaging.Consumers;

/// <summary>
/// RabbitMQ consumer: indexes a patient into Elasticsearch after create/update.
///
/// Uses <see cref="ISearchService"/> from EHRPlatform.Common to upsert
/// the patient document. Idempotent: upsert by PatientId.
/// </summary>
public sealed class PatientIndexConsumer : IConsumer<PatientIndexMessage>
{
    private readonly ISearchService _search;
    private readonly ILogger<PatientIndexConsumer> _logger;

    public PatientIndexConsumer(ISearchService search, ILogger<PatientIndexConsumer> logger)
    {
        _search = search;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PatientIndexMessage> context)
    {
        var msg = context.Message;

        using var activity = EHRTelemetry.StartActivity(
            "PatientIndexConsumer.Consume",
            correlationId: msg.CorrelationId);

        activity?.SetTag(EHRTelemetry.TagPatientId, msg.PatientId.ToString());

        _logger.LogInformation(
            "Indexing PatientId={PatientId} in Elasticsearch",
            msg.PatientId);

        try
        {
            var document = new
            {
                id          = msg.PatientId,
                firstName   = msg.FirstName,
                lastName    = msg.LastName,
                fullName    = $"{msg.FirstName} {msg.LastName}",
                email       = msg.Email,
                mrn         = msg.MRN,
                gender      = msg.Gender,
                dateOfBirth = msg.DateOfBirth,
                status      = msg.Status,
                indexedAt   = DateTime.UtcNow
            };

            await _search.IndexAsync(msg.PatientId.ToString(), document, context.CancellationToken);

            _logger.LogInformation(
                "PatientId={PatientId} indexed in Elasticsearch successfully",
                msg.PatientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to index PatientId={PatientId} in Elasticsearch",
                msg.PatientId);
            activity.RecordException(ex);
            throw;
        }
    }
}



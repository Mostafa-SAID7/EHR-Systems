using Mapster;

namespace EHRPlatform.Services.Billing.Application.Payments.Mappers;

/// <summary>
/// Mapster registration profile for Payments feature.
/// Handles conversion between Payment domain model and DTOs.
/// Single Responsibility: Configure Payments-related type mappings only.
/// </summary>
public class PaymentMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Payment → PaymentResponseDto
        config.NewConfig<Payment, PaymentResponseDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.InvoiceId, src => src.InvoiceId)
            .Map(dest => dest.Amount, src => src.Amount)
            .Map(dest => dest.Method, src => src.Method)
            .Map(dest => dest.ReceivedAt, src => src.ReceivedAt);

        // PaymentResponseDto → Payment (for updates/inserts)
        config.NewConfig<PaymentResponseDto, Payment>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.InvoiceId, src => src.InvoiceId)
            .Map(dest => dest.Amount, src => src.Amount)
            .Map(dest => dest.Method, src => src.Method)
            .Map(dest => dest.ReceivedAt, src => src.ReceivedAt);
    }
}

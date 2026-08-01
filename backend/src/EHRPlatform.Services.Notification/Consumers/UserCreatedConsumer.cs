using MassTransit;
using Microsoft.Extensions.Logging;
using EHRPlatform.BuildingBlocks.Contracts.DTOs;

namespace EHRPlatform.Services.Notification.Consumers
{
    /// <summary>
    /// Consumes UserCreatedEvent from Identity Service.
    /// Sends welcome email to newly created users.
    /// </summary>
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly ILogger<UserCreatedConsumer> _logger;
        private readonly IEmailService _emailService;

        public UserCreatedConsumer(
            ILogger<UserCreatedConsumer> logger,
            IEmailService emailService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var @event = context.Message;
            
            _logger.LogInformation(
                "Consuming UserCreatedEvent: UserId={UserId}, Email={Email}",
                @event.UserId,
                @event.Email
            );

            try
            {
                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(
                    @event.Email,
                    @event.FirstName,
                    @event.LastName,
                    context.CancellationToken
                );

                _logger.LogInformation(
                    "Welcome email sent successfully to {Email}",
                    @event.Email
                );

                // Publish notification sent event for audit
                var notificationEvent = new EmailNotificationSentEvent
                {
                    NotificationId = Guid.NewGuid(),
                    RecipientEmail = @event.Email,
                    Type = "WelcomeEmail",
                    Status = "Sent",
                    CorrelationId = @event.CorrelationId,
                    Timestamp = DateTime.UtcNow,
                    InitiatedBy = "system"
                };

                await context.Publish(notificationEvent, context.CancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(
                    ex,
                    "Invalid email service operation for user {UserId}",
                    @event.UserId
                );
                
                // Publish failed notification event
                var failedEvent = new NotificationFailedEvent
                {
                    NotificationId = Guid.NewGuid(),
                    RecipientEmail = @event.Email,
                    Reason = "Invalid email service operation",
                    CorrelationId = @event.CorrelationId,
                    Timestamp = DateTime.UtcNow
                };

                await context.Publish(failedEvent, context.CancellationToken);
                throw; // Trigger retry
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send welcome email to {Email} for user {UserId}",
                    @event.Email,
                    @event.UserId
                );

                // Publish failed notification event
                var failedEvent = new NotificationFailedEvent
                {
                    NotificationId = Guid.NewGuid(),
                    RecipientEmail = @event.Email,
                    Reason = ex.Message,
                    CorrelationId = @event.CorrelationId,
                    Timestamp = DateTime.UtcNow
                };

                await context.Publish(failedEvent, context.CancellationToken);
                throw; // Trigger retry
            }
        }
    }

    /// <summary>
    /// Email service interface for sending notifications.
    /// </summary>
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(
            string email,
            string firstName,
            string lastName,
            CancellationToken cancellationToken);

        Task SendAppointmentReminderAsync(
            string email,
            string appointmentDetails,
            CancellationToken cancellationToken);

        Task SendInvoiceAsync(
            string email,
            string invoiceId,
            CancellationToken cancellationToken);
    }
}


namespace Application.DTOs.Response;

public class PaymentResponse
{
    public Guid TransactionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? ApprovalCode { get; set; }
}
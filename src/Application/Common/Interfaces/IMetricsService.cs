// src/Application/Common/Interfaces/IMetricsService.cs
namespace Application.Common.Interfaces;

public interface IMetricsService
{
    void RecordPaymentProcessed(string merchantId, string status, double amount);
    void RecordPaymentDuration(string merchantId, double durationMs);
    void RecordRateLimitHit(string merchantId, string plan);
    void IncrementActiveMerchants();
    void DecrementActiveMerchants();
}
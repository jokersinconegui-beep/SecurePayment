// src/Infrastructure/Services/MetricsService.cs
using Prometheus;
using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class MetricsService : IMetricsService
{
    // Métricas de negocio
    private static readonly Counter _paymentsTotal = Metrics
        .CreateCounter("payments_total_total", "Total number of payment requests",
            new CounterConfiguration
            {
                LabelNames = ["merchant_id", "status"]
            });
    
    private static readonly Histogram _paymentDuration = Metrics
        .CreateHistogram("payment_duration_seconds", "Duration of payment processing",
            new HistogramConfiguration
            {
                LabelNames = ["merchant_id"],
                Buckets = [0.01, 0.05, 0.1, 0.5, 1, 2, 5]
            });
    
    private static readonly Counter _rateLimitHits = Metrics
        .CreateCounter("rate_limit_hits_total", "Number of rate limit hits",
            new CounterConfiguration
            {
                LabelNames = ["merchant_id", "plan"]
            });
    
    private static readonly Gauge _activeMerchants = Metrics
        .CreateGauge("active_merchants", "Number of active merchants");
    
    private static readonly Counter _totalAmount = Metrics
        .CreateCounter("payments_amount_total", "Total amount processed",
            new CounterConfiguration
            {
                LabelNames = ["currency"]
            });
    
    // Métricas HTTP automáticas (ya las maneja UseHttpMetrics)
    
    public void RecordPaymentProcessed(string merchantId, string status, double amount)
    {
        _paymentsTotal.WithLabels(merchantId, status).Inc();
        _totalAmount.WithLabels("USD").Inc(amount);
    }
    
    public void RecordPaymentDuration(string merchantId, double durationMs)
    {
        _paymentDuration.WithLabels(merchantId).Observe(durationMs / 1000);
    }
    
    public void RecordRateLimitHit(string merchantId, string plan)
    {
        _rateLimitHits.WithLabels(merchantId, plan).Inc();
    }
    
    public void IncrementActiveMerchants()
    {
        _activeMerchants.Inc();
    }
    
    public void DecrementActiveMerchants()
    {
        _activeMerchants.Dec();
    }
}
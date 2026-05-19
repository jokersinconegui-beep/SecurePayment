# 💳 SecurePaymentGateway

<!-- Badges de CI/CD -->
[![Build & Test](https://github.com/jokersinconegui-beep/SecurePaymentGateway/actions/workflows/build.yml/badge.svg)](https://github.com/jokersinconegui-beep/SecurePaymentGateway/actions/workflows/build.yml)
[![Security Scan](https://github.com/jokersinconegui-beep/SecurePaymentGateway/actions/workflows/security-scan.yml/badge.svg)](https://github.com/jokersinconegui-beep/SecurePaymentGateway/actions/workflows/security-scan.yml)
[![Code Quality](https://github.com/jokersinconegui-beep/SecurePaymentGateway/actions/workflows/code-quality.yml/badge.svg)](https://github.com/jokersinconegui-beep/SecurePaymentGateway/actions/workflows/code-quality.yml)
[![Performance](https://github.com/jokersinconegui-beep/SecurePaymentGateway/actions/workflows/performance.yml/badge.svg)](https://github.com/jokersinconegui-beep/SecurePaymentGateway/actions/workflows/performance.yml)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Tests](https://img.shields.io/badge/Tests-43%20passing-brightgreen)](./tests)
[![Architecture](https://img.shields.io/badge/Architecture-Clean-6B3E9B)](src/)
[![Rate Limit](https://img.shields.io/badge/Rate%20Limit-Token%20Bucket-orange)](src/WebApi/RateLimiters)

**Pasarela de pagos profesional** con arquitectura limpia, CQRS, rate limiting y cumplimiento PCI-DSS.

---

## ✨ Características

| Área | Tecnología | Implementación |
|------|-----------|----------------|
| 🏗️ **Arquitectura** | Clean Architecture + DDD | Separación Domain/Application/Infrastructure/WebApi |
| 🔐 **Seguridad** | PCI-DSS Compliance | Luhn algorithm, CVV no almacenado, enmascaramiento |
| ⚡ **Rendimiento** | Token Bucket Algorithm | Rate limiting: 100 req/min por IP |
| 📦 **Patrones** | CQRS + MediatR | Commands, Queries, Pipeline Behaviors |
| ✅ **Validación** | FluentValidation | Validación automática con pipeline |
| 🧪 **Testing** | xUnit + Moq | 43 tests unitarios |

---

## 🚀 Endpoints

| Método | Endpoint | Descripción | Rate Limit |
|--------|----------|-------------|------------|
| `POST` | `/api/Payments/process` | Procesar pago | 100/min |
| `GET` | `/api/Payments/status/{id}` | Consultar estado | 100/min |
| `GET` | `/api/Test/ping` | Verificar API | 100/min |
| `GET` | `/health` | Health check | Ilimitado |

### Ejemplo de pago exitoso

```bash
curl -X POST http://localhost:5077/api/Payments/process \
  -H "Content-Type: application/json" \
  -d '{
    "cardNumber": "4532015112830366",
    "cvv": "123",
    "amount": 99.99,
    "currency": "USD",
    "merchantId": "MERCHANT001",
    "idempotencyKey": "abc-123"
  }'
# 💳 SecurePaymentGateway

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Tests](https://img.shields.io/badge/Tests-43%20passing-brightgreen)](./tests)
[![Architecture](https://img.shields.io/badge/Architecture-Clean-6B3E9B)](src/)
[![Rate Limit](https://img.shields.io/badge/Rate%20Limit-Merchant%20Plan-orange)](src/WebApi/RateLimiters)
[![Metrics](https://img.shields.io/badge/Metrics-Prometheus-red)](https://prometheus.io)

**Pasarela de pagos profesional** con arquitectura limpia, rate limiting por merchant, autenticación JWT, refresh tokens, logging estructurado, caché Redis, contenerización Docker y métricas Prometheus.

---

## ✨ Características

| Área | Tecnología | Implementación |
|------|-----------|----------------|
| 🏗️ **Arquitectura** | Clean Architecture + DDD | Domain / Application / Infrastructure / WebApi |
| 🔐 **Seguridad** | JWT + Refresh Tokens | Autenticación stateless, BCrypt para passwords |
| ⚡ **Rendimiento** | Rate Limiting por Merchant | Basic (100 req/min) / Premium (1000 req/min) |
| 💰 **Pagos** | PCI-DSS Compliance | Algoritmo de Luhn, CVV no almacenado |
| 📊 **Monitoreo** | Prometheus + Serilog | Métricas HTTP + logs estructurados |
| 🗄️ **Persistencia** | Entity Framework + SQLite | Base de datos local (sin instalación) |
| 🚀 **Despliegue** | Docker Compose | API + Redis contenerizados |

---

## 🚀 Inicio rápido (2 minutos)

### Requisitos previos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (opcional, para Redis)

### Clonar y ejecutar

```bash
# 1. Clonar el repositorio
git clone https://github.com/tuusuario/SecurePaymentGateway.git
cd SecurePaymentGateway

# 2. Restaurar paquetes y compilar
dotnet restore
dotnet build

# 3. Ejecutar la API
dotnet run --project src/WebApi
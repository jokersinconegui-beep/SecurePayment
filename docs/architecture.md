# Decisiones de Arquitectura (ADRs)

## ADR-001: Clean Architecture

### Contexto
Necesitamos un sistema mantenible, testeable e independiente de frameworks.

### Decisión
Usar Clean Architecture con capas:
- Domain (core, sin dependencias)
- Application (casos de uso)
- Infrastructure (implementaciones)
- WebApi (presentación)

### Consecuencias
- Código más testeable
- Independencia de frameworks
- Mayor curva de aprendizaje inicial

## ADR-002: Rate Limiting con Token Bucket

### Contexto
Proteger la API contra abusos y DoS.

### Decisión
Implementar Token Bucket Algorithm:
- 100 tokens por IP
- Recarga de 1.667 tokens/segundo

### Consecuencias
- Permite ráfagas controladas
- Implementación eficiente O(1)
- Sin dependencias externas

## ADR-003: Result Pattern en lugar de Excepciones

### Contexto
Manejar errores de dominio sin lanzar excepciones.

### Decisión
Usar Result<T> pattern para operaciones que pueden fallar.

### Consecuencias
- Flujo explícito (sin try-catch)
- Más verboso pero predecible
- Mejor rendimiento
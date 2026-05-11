# TS01 - Implementación de Endpoints de Monitoreo de Salud (Health Checks)
# Epic: E09 - Arquitectura de Software y Atributos de Calidad Técnica

Feature: Endpoints de monitoreo de salud del sistema
  Como Developer
  Quiero implementar un sistema de Health Checks en la API REST
  Para asegurar que la infraestructura detecte y reporte caídas de servicios automáticamente

  Background:
    Given el servicio de la API de Grotix está en ejecución

  Scenario: Verificación de estado activo del servidor
    When el sistema de monitoreo realiza una petición GET al endpoint /health/live
    Then la API debe responder con status 200 OK
    And el cuerpo de la respuesta debe indicar que el proceso del servidor está activo

  Scenario: Verificación de dependencias críticas con base de datos disponible
    Given la base de datos se encuentra accesible
    When se consulta el endpoint /health/ready
    Then el sistema debe verificar la conexión activa con la base de datos
    And debe responder con status 200 OK

  Scenario: Respuesta 503 cuando la base de datos está caída
    Given la conexión a la base de datos está caída
    When se consulta el endpoint /health/ready
    Then la API debe retornar status 503 Service Unavailable
    And no debe procesar peticiones que dependan de la base de datos

  Scenario: Registro de disponibilidad del microcontrolador
    Given un dispositivo IoT está conectado y activo
    When el hardware envía una señal periódica de disponibilidad al backend
    Then el backend debe actualizar el estado del dispositivo en la base de datos
    And el sistema debe reflejar que el dispositivo sigue en línea

  Scenario: Alerta técnica ante fallos persistentes
    Given un componente crítico reporta un estado de error
    When el fallo persiste por más de 3 chequeos consecutivos
    Then el sistema debe disparar una notificación automática hacia el equipo de desarrollo

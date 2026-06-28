Feature: US21 - Generación y descarga de reportes históricos
  Como usuario
  Quiero generar y descargar reportes detallados de riego
  Para analizar el consumo de agua y la eficiencia del sistema

  Background:
    Given el usuario está autenticado como administrador
    And existe una zona con id 1 en el sistema

  Scenario: Escenario 1 - Reporte semanal
    When el usuario solicita el reporte de la zona 1 para los últimos 7 días
    Then la respuesta del reporte es 200 OK
    And el reporte contiene información del periodo solicitado

  Scenario: Escenario 1 - Reporte mensual
    When el usuario solicita el reporte de la zona 1 para los últimos 30 días
    Then la respuesta del reporte es 200 OK
    And el reporte contiene información del periodo solicitado

  Scenario: Escenario 1 - Reporte trimestral
    When el usuario solicita el reporte de la zona 1 para los últimos 90 días
    Then la respuesta del reporte es 200 OK
    And el reporte contiene información del periodo solicitado

  Scenario: Escenario 2 - Vista previa de métricas
    When el usuario solicita el reporte de la zona 1 para los últimos 30 días
    Then la respuesta del reporte es 200 OK
    And el reporte incluye telemetría promedio e irrigación

  Scenario: Escenario 3 - Exportación PDF
    When el usuario exporta el PDF de la zona 1 para los últimos 30 días
    Then la respuesta es un archivo PDF

  Scenario: Zona inexistente retorna error
    Given la zona 99999 no existe en el sistema
    When el usuario solicita el reporte de la zona 99999 para los últimos 30 días
    Then la respuesta del reporte es 404 Not Found
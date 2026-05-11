# TS08 - Dashboard web con indicadores clave del sistema
# Epic: E10 - Gestión de Clientes y Contratos

Feature: Dashboard web con indicadores clave del sistema
  Como Administrador
  Quiero visualizar un panel de control centralizado con los KPIs más relevantes de Grotix
  Para obtener en tiempo real una visión global del estado de la plataforma

  Background:
    Given el administrador ha iniciado sesión en el portal web administrativo

  Scenario: Visualización del dashboard principal con tarjetas de resumen
    When el administrador accede a la pantalla principal
    And los datos terminan de cargar
    Then el sistema debe mostrar tarjetas de resumen con número de clientes activos
    And debe mostrar contratos vigentes y próximos a vencer
    And debe mostrar dispositivos en línea y offline

  Scenario: Resaltado visual de alertas críticas en el dashboard
    Given existen contratos vencidos o alertas críticas en el sistema
    When el administrador visualiza el dashboard
    Then las tarjetas con alertas deben resaltarse en color rojo
    And deben permitir navegar directamente al detalle al presionarlas


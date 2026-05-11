# TS10 - Registro de contratos externos en campo
# Epic: E10 - Gestión de Clientes y Contratos

Feature: Registro de contratos externos en campo
  Como Administrador
  Quiero registrar los contratos firmados con clientes incluyendo fechas y planes
  Para mantener la trazabilidad contractual de forma inmediata incluso en campo

  Background:
    Given el administrador ha iniciado sesión en el portal web administrativo

  Scenario: Registro exitoso de nuevo contrato
    Given el administrador ha firmado un contrato físico con un cliente
    When registra los datos del contrato incluyendo fechas, plan y cliente en la app
    Then el sistema debe crear el contrato con estado "Activo"
    And debe habilitar los servicios IoT correspondientes para ese cliente


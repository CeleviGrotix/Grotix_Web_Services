Feature: US27 - Visualización del estado de servicios
  Como usuario de Grotix
  Quiero visualizar el estado de mis servicios inteligentes
  Para estar informado sobre la disponibilidad de la analítica y el riego automático

  Background:
    Given el usuario está autenticado como user_admin con asociación 1

  Scenario: Escenario 1 - Servicio activo y habilitado
    Given la asociación 1 tiene un contrato activo y no suspendido
    When el usuario consulta el estado de sus servicios
    Then la respuesta es 200 OK
    And el contrato muestra el servicio como disponible

  Scenario: Escenario 3 - Servicio suspendido
    Given la asociación 1 tiene un contrato suspendido
    When el usuario consulta el estado de sus servicios
    Then la respuesta es 200 OK
    And el contrato muestra el servicio como suspendido

  Scenario: Escenario 2 - Sin contrato activo
    Given la asociación 1 no tiene contratos
    When el usuario consulta el estado de sus servicios
    Then la respuesta es 200 OK
    And la lista de contratos está vacía
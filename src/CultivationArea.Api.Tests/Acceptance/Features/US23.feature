Feature: US23 - Clasificación del estado fenológico mediante IA
  Como usuario de Grotix
  Quiero que el sistema identifique automáticamente la etapa de crecimiento de mi planta
  Para conocer su progreso biológico sin necesidad de ser experto en botánica

  Background:
    Given el usuario está autenticado como administrador
    And existe una zona con id 1 en el sistema

  Scenario: Escenario 1 - Categorización exitosa de estado de crecimiento
    When el sistema registra el resultado de IA con fase "Germinacion" y score 85
    Then la respuesta del servidor es 201 Created
    And el reporte contiene la fase detectada "Germinacion"

  Scenario: Escenario 2 - Resultado con confianza baja marcado como Indeterminado
    When el sistema registra el resultado de IA con fase "Indeterminado" y score 50
    Then la respuesta del servidor es 201 Created
    And el reporte contiene la fase detectada "Indeterminado"

  Scenario: Escenario 3 - Zona inexistente retorna error
    Given la zona 99999 no existe en el sistema
    When el sistema registra el resultado de IA en zona 99999 con fase "Germinacion" y score 85
    Then la respuesta del servidor es 404 Not Found
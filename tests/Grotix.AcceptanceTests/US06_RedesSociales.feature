# US06 - Enlaces a redes sociales
# Epic: E01 - Presencia Digital y Propuesta de Valor

Feature: Enlaces a redes sociales oficiales
  Como visitante
  Quiero encontrar los accesos a las redes sociales oficiales de Grotix
  Para mantenerme actualizado sobre las novedades y el desarrollo del proyecto

  Background:
    Given el visitante se encuentra en la landing page de Grotix

  Scenario: Redes sociales visibles en el pie de página
    When el visitante se desplaza hasta el final de la landing page
    Then el sistema debe mostrar enlaces a Instagram, LinkedIn y Facebook
    And los enlaces deben estar ubicados en el área del footer

  Scenario: Iconografía oficial y reconocible
    When el visitante observa la sección de redes sociales
    Then los iconos deben corresponder a los logotipos oficiales de cada plataforma
    And deben ser fácilmente identificables por el visitante

  Scenario: Validación de enlaces externos activos hacia perfiles oficiales
    Given el sitio está en producción
    When el administrador actualiza las URLs de las redes sociales
    Then los enlaces deben dirigir exactamente a los perfiles oficiales de Grotix
    And no deben apuntar a páginas de inicio genéricas de las plataformas

  Scenario: Apertura de enlaces en ventana nueva
    When el visitante selecciona un ícono de red social
    Then el sistema debe abrir la red social en una pestaña nueva
    And el visitante debe permanecer en la landing page de Grotix

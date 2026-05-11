# US08 - Implementación de Identidad y Consistencia Visual
# Epic: E02 - Optimización de Usabilidad y Rendimiento Web

Feature: Identidad y consistencia visual de la landing page
  Como visitante de la landing page
  Quiero una interfaz visual coherente
  Para navegar de forma clara y agradable

  Background:
    Given el visitante navega por la landing page de Grotix

  Scenario: Paleta de colores coherente con estándares de accesibilidad
    When el visitante visualiza los elementos de la interfaz en cualquier sección
    Then los colores deben seguir una jerarquía visual definida
    And los contrastes deben cumplir con los estándares de accesibilidad WCAG

  Scenario: Jerarquía tipográfica estandarizada
    When el navegador renderiza los textos de la landing page
    Then el sistema debe aplicar una escala tipográfica clara
    And la tipografía debe facilitar la lectura rápida del contenido

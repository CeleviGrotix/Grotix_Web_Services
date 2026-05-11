# US03 - Implementación de CTA
# Epic: E01 - Presencia Digital y Propuesta de Valor

Feature: Implementación de llamadas a la acción (CTA)
  Como visitante
  Quiero encontrar botones de acción claros y visibles
  Para interactuar con la plataforma sin tener que buscar los accesos

  Background:
    Given el visitante accede a la landing page de Grotix

  Scenario: CTA principal visible en la Hero Section
    When el visitante visualiza la primera sección de la página
    Then debe aparecer al menos un botón de acción principal "Comienza ahora"
    And el botón debe estar resaltado visualmente respecto al resto del contenido

  Scenario: CTA secundario persistente en la barra de navegación
    When el visitante se desplaza hacia abajo para leer más información
    Then la barra de navegación debe mantenerse fija en la parte superior
    And un botón de CTA secundario debe permanecer visible en todo momento

  Scenario: Página de error personalizada cuando el servidor está caído
    Given el visitante hace clic en el CTA "Comienza ahora"
    When el servidor de la aplicación está bajo mantenimiento o caído
    Then el sistema debe mostrar una página de error personalizada
    And no debe mostrarse un error genérico del navegador

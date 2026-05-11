# US09 - Optimización de tiempos de respuesta y carga inicial
# Epic: E02 - Optimización de Usabilidad y Rendimiento Web

Feature: Optimización de tiempos de respuesta y carga inicial
  Como visitante
  Quiero que la landing page cargue de forma inmediata
  Para acceder a la información sin frustraciones

  Background:
    Given el visitante intenta acceder a la landing page de Grotix

  Scenario: Tiempo de carga menor a 1.5 segundos en condiciones normales
    When el visitante ingresa la URL en su navegador
    Then la página debe cargar completamente en menos de 1.5 segundos
    And el contenido principal debe ser visible sin esperas adicionales

  Scenario: Carga diferida de imágenes fuera de la vista inicial
    When el visitante abre la página por primera vez
    Then solo deben cargarse los recursos visibles en la pantalla inicial
    And las imágenes de las secciones inferiores deben cargarse al hacer scroll hacia ellas

  Scenario: Rendimiento aceptable en dispositivos móviles con datos
    Given el visitante accede desde una conexión de datos móviles
    When interactúa con la landing page
    Then el tiempo total de interacción no debe superar los 3.5 segundos

  Scenario: Prioridad del texto sobre scripts en conexiones lentas
    Given la red del visitante es lenta
    When el tiempo de carga supera los 5 segundos
    Then el sistema debe priorizar la carga del HTML y CSS
    And el visitante debe poder leer el contenido aunque las animaciones no estén listas

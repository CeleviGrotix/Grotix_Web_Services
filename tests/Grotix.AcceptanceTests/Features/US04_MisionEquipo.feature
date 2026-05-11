# US04 - Visualización de misión, visión y equipo
# Epic: E01 - Presencia Digital y Propuesta de Valor

Feature: Visualización de misión, visión y equipo
  Como visitante
  Quiero conocer la historia, misión y quiénes están detrás de Grotix
  Para generar confianza en la solución tecnológica que ofrecen

  Background:
    Given el visitante se encuentra en la landing page de Grotix

  Scenario: Exposición de la misión y visión del proyecto
    When el visitante navega hacia la sección "Nosotros"
    Then el sistema debe presentar de forma clara el nombre del startup
    And debe mostrarse una descripción del proyecto visible para el visitante

  Scenario: Presentación del equipo de trabajo con misión
    When la página carga los elementos de la sección de equipo
    Then el visitante puede leer la misión del startup a través de Grotix
    And la información debe estar organizada de forma legible

  Scenario: Placeholders cuando las imágenes de perfil fallan
    Given el servidor de imágenes tiene un problema de latencia
    When el visitante entra a la sección del equipo
    Then el sistema debe mostrar avatares genéricos en lugar de las fotos
    And cada placeholder debe incluir el nombre y cargo del integrante
    And la sección no debe verse vacía ni rota

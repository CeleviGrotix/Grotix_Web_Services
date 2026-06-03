Feature: Administracion de Zonas de Cultivo
  Como agricultor de Grotix
  Quiero organizar mis dispositivos por zonas y especies
  Para gestionar mi produccion de manera eficiente

  # US15 - Escenario 1: Creación y personalización de zonas
  Scenario: Creacion de nueva zona agricola
    Given una granja existente con ID 1
    When el usuario crea una zona para el cultivo 5 en las coordenadas 12.5 y -70.3
    Then el sistema debe guardar la zona exitosamente en la granja 1
    And la latitud de la zona debe ser 12.5

  # US15 - Escenario 2: Asignación de tipo de planta (Crop)
  Scenario: Reasignacion de especie a zona existente
    Given una zona de cultivo registrada con la especie 5
    When el usuario modifica la zona para sembrar la especie 10
    Then el identificador de la especie en la zona debe actualizarse a 10

  # US15 - Actualizacion de fase de crecimiento
  Scenario: Actualizacion de fase fenologica
    Given una zona de cultivo en fase "Siembra"
    When el usuario actualiza la fase a "Germinacion"
    Then el sistema debe registrar el estado actual como "Germinacion"
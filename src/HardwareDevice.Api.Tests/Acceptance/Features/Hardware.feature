Feature: Gestion de Mantenimiento de Dispositivos IoT
  Como Administrador
  Quiero gestionar el mantenimiento preventivo y correctivo de los dispositivos IoT
  Para asegurar la continuidad operativa de los sensores en campo

  # US12 - Escenario 1: Reporte de inicio de mantenimiento
  Scenario: Cambio de estado del dispositivo a Mantenimiento
    Given un dispositivo ESP32 registrado y activo
    When el administrador cambia su estado a "EN_MANTENIMIENTO"
    Then el sistema debe actualizar el estado del dispositivo exitosamente
    And el nuevo estado debe reflejarse como "EN_MANTENIMIENTO"

  # US12 - Escenario 2: Registro de acciones de mantenimiento
  Scenario: Guardado de bitacora tecnica
    Given un dispositivo en revision
    When el tecnico completa la bitacora con la descripcion "Limpieza de sensor"
    Then el sistema debe crear un registro tecnico con la fecha actual
    And la descripcion guardada debe ser "Limpieza de sensor"
    
  # US15 - Escenario 5: Reubicacion de dispositivos entre zonas
  Scenario: Desvinculacion de dispositivo de una zona
    Given un dispositivo asignado a la zona 5
    When el administrador ejecuta la desvinculacion
    Then el dispositivo no debe estar asociado a ninguna zona
Feature: US26 - Configuración y gestión de alertas de usuario
  Como usuario de Grotix
  Quiero personalizar mis preferencias de notificación
  Para recibir información relevante únicamente si lo deseo

  Background:
    Given el usuario autenticado tiene id 1

  Scenario: Escenario 1 - Activar notificaciones push
    When el usuario activa push y desactiva email
    Then la preferencia se guarda correctamente con código 200

  Scenario: Escenario 1 - Desactivar todos los canales
    When el usuario desactiva push y desactiva email
    Then la preferencia se guarda correctamente con código 200

  Scenario: Escenario 1 - Activar todos los canales
    When el usuario activa push y activa email
    Then la preferencia se guarda correctamente con código 200

  Scenario: Escenario 3 - Persistencia de preferencias
    When el usuario activa push y activa email
    Then la preferencia se guarda correctamente con código 200
    And el comando enviado contiene push true y email true
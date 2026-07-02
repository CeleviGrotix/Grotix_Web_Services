Feature: Perfiles y Contratos BDD
  Como usuario y administrador de Grotix
  Quiero gestionar perfiles y contratos
  Para mantener la plataforma actualizada y segura

  # US26 - Escenario 1: Edicion de campos basicos
  Scenario: Modificacion de perfil de usuario
    Given un usuario registrado con el nombre "Juan Perez"
    When el usuario actualiza su nombre a "Juan Martinez" y su telefono a "+51999888777"
    Then los datos del perfil deben reflejar el nombre "Juan Martinez"

  # Validacion de dominio de Contratos
  Scenario: Actualizacion de limites de un contrato
    Given un contrato comercial activo con limite de 10 zonas
    When el administrador actualiza el limite maximo a 25 zonas
    Then el contrato debe permitir hasta 25 zonas operativas

  # US11 - Escenario 3: Suspension manual inmediata
  Scenario: Suspension de un contrato
    Given un contrato comercial en estado "Active" no suspendido
    When el administrador ejecuta la suspension del contrato
    Then la bandera de suspension del contrato debe estar activa
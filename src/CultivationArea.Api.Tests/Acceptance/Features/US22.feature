Feature: US22 - Gestión de registro fotográfico de cultivos
  Como usuario
  Quiero que el sistema registre la URL de la fotografía de mi cultivo
  Para poder identificar visualmente mis zonas en la plataforma

  Background:
    Given el usuario está autenticado como administrador
    And existe una zona con id 1 en el sistema

  Scenario: Escenario 3 - Registrar URL de imagen en zona sin fotografía
    Given la zona 1 no tiene imagen registrada
    When el usuario actualiza la zona 1 con imageUrl "https://storage.azure.com/grotix/cultivo.jpg"
    Then la respuesta del servidor es 200 OK
    And la zona 1 tiene imageUrl "https://storage.azure.com/grotix/cultivo.jpg"

  Scenario: Escenario 3 - Actualizar URL de imagen existente
    Given la zona 1 tiene imageUrl "https://storage.azure.com/grotix/old.jpg"
    When el usuario actualiza la zona 1 con imageUrl "https://storage.azure.com/grotix/new.jpg"
    Then la respuesta del servidor es 200 OK
    And la zona 1 tiene imageUrl "https://storage.azure.com/grotix/new.jpg"

  Scenario: Escenario 2 - URL vacía no persiste imagen
    Given la zona 1 tiene imageUrl "https://storage.azure.com/grotix/cultivo.jpg"
    When el usuario actualiza la zona 1 con imageUrl vacía
    Then la zona 1 no tiene imagen registrada

  Scenario: Escenario 3 - Zona inexistente retorna error
    Given la zona 99999 no existe en el sistema
    When el usuario actualiza la zona 99999 con imageUrl "https://storage.azure.com/grotix/cultivo.jpg"
    Then la respuesta del servidor es 404 Not Found
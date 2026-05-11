# US02 - Enlaces de acceso a la aplicación móvil
# Epic: E01 - Presencia Digital y Propuesta de Valor

Feature: Enlaces de acceso a la aplicación móvil
  Como visitante
  Quiero tener botones claros de acceso
  Para dirigirme rápidamente a la plataforma donde gestionaré mis cultivos

  Background:
    Given la landing page de Grotix se encuentra disponible

  Scenario: Redirección a la aplicación móvil desde el botón GET NOW
    Given el visitante desea instalar la aplicación en su dispositivo
    When el visitante selecciona el botón "GET NOW!"
    Then el sistema debe redirigirlo a la tienda de aplicaciones oficial
    And la tienda debe corresponder al sistema operativo del smartphone del visitante

  Scenario: Validación de enlaces operativos sin errores
    When un visitante intenta usar cualquiera de los botones de redirección
    Then el sistema debe asegurar que no existan enlaces rotos
    And ningún enlace debe retornar error 404
    And el destino debe ser el entorno correcto de descarga

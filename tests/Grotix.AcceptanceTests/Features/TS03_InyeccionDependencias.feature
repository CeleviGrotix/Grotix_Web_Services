# TS03 - Desacoplamiento de Lógica de Negocio mediante Inyección de Dependencias
# Epic: E09 - Arquitectura de Software y Atributos de Calidad Técnica

Feature: Desacoplamiento mediante Inyección de Dependencias y patrón Repository
  Como Developer
  Quiero implementar el patrón de Inyección de Dependencias y el patrón Repository en .NET
  Para garantizar que la lógica de Grotix sea modificable y no dependa de implementaciones específicas

  Scenario: Abstracción de la persistencia de datos mediante interfaz de repositorio
    Given la lógica de negocio requiere guardar datos de telemetría
    When el servicio solicita la persistencia de un dato
    Then debe interactuar únicamente con la interfaz ITelemetryRepository
    And debe ser posible cambiar el motor de base de datos en Program.cs sin modificar la lógica de cálculo

  Scenario: El controlador actúa solo como mediador delegando al Domain Service
    Given se recibe un request para procesar una regla de riego
    When se ejecuta la validación del request
    Then el controlador de la API debe delegar la lógica a un Domain Service puro
    And el controlador no debe contener lógica de negocio directamente

  Scenario: Uso de DTOs para contratos de API estables ante cambios internos
    Given la estructura de las tablas de la base de datos puede cambiar
    When la API devuelve una respuesta al frontend
    Then debe usar Objetos de Transferencia de Datos (DTOs) en lugar de entidades de base de datos
    And los cambios internos en el modelo no deben romper la integración con el cliente

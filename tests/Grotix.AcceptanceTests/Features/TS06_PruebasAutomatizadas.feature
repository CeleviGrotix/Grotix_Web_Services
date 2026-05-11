# TS06 - Implementación de Infraestructura de Pruebas Automatizadas
# Epic: E09 - Arquitectura de Software y Atributos de Calidad Técnica

Feature: Infraestructura de pruebas automatizadas
  Como Developer
  Quiero configurar un entorno de pruebas unitarias e integrales
  Para garantizar la detección temprana de errores y asegurar la estabilidad del sistema

  Scenario: Pruebas unitarias validan lógica de negocio de forma aislada
    Given se ha desarrollado una nueva regla de cálculo para el riego automático
    When el desarrollador ejecuta el motor de pruebas con dotnet test
    Then el sistema debe validar los algoritmos de forma aislada
    And los resultados deben coincidir con los valores esperados
    And no debe requerirse conexión a la base de datos real

  Scenario: Uso de objetos simulados para dependencias externas
    Given un servicio de la API depende de Inteligencia Artificial o de la base de datos
    When se ejecuta una prueba técnica sobre ese servicio
    Then el sistema debe permitir el uso de mocks para sustituir esas dependencias
    And los tests deben ejecutarse rápidamente sin depender de conexión a internet

  Scenario: Pruebas de componentes en el frontend verifican renderizado correcto
    Given se ha creado un componente crítico como el indicador de humedad en tiempo real
    When se ejecuta la suite de pruebas del frontend
    Then el sistema debe verificar que el componente renderiza los datos correctamente
    And debe responder adecuadamente a los cambios de estado

  Scenario: Pipeline de CI ejecuta y bloquea despliegue si hay fallos
    Given el desarrollador sube un nuevo cambio al repositorio
    When se activa el pipeline de integración continua
    Then todos los tests deben ejecutarse automáticamente
    And si alguna prueba falla el despliegue debe ser bloqueado

# TS13 - Consulta rápida del catálogo de cultivos
# Epic: E11 - Supervisión y Operación de Infraestructura IoT

Feature: Consulta rápida del catálogo de cultivos
  Como Administrador
  Quiero consultar y ajustar los parámetros del catálogo de cultivos
  Para tener acceso a la información técnica de umbrales mientras superviso zonas de cultivo

  Background:
    Given el administrador ha iniciado sesión en el portal web administrativo
    And se encuentra en el módulo de cultivos

  Scenario: Visualización de rangos óptimos de cada cultivo
    When el administrador navega al módulo de cultivos
    Then la app debe mostrar una lista de cultivos con tarjetas de resumen
    And cada tarjeta debe incluir los rangos óptimos de humedad, luz y temperatura

  Scenario: Actualización de umbrales mediante controles deslizantes
    Given el administrador selecciona un cultivo para editar
    When edita el rango de humedad mediante controles deslizantes y guarda
    Then el sistema debe actualizar los valores del cultivo
    And debe notificar la propagación de los nuevos parámetros a los dispositivos asociados

  Scenario: Búsqueda dinámica de cultivo por nombre
    When el administrador ingresa el nombre de un cultivo en la barra de búsqueda
    Then la app debe filtrar la lista instantáneamente
    And debe mostrar solo los cultivos que coincidan con la búsqueda

  Scenario: Manejo de error al actualizar parámetros sin conexión
    Given existe una falla de conexión temporal
    When el administrador edita un umbral y presiona guardar
    Then la app debe mostrar un mensaje informando que los cambios no pudieron aplicarse
    And debe revertir los controles a su valor original

# TS09 - Gestión web de clientes agricultores
# Epic: E10 - Gestión de Clientes y Contratos

Feature: Gestión web de clientes agricultores
  Como Administrador
  Quiero consultar y editar perfiles de clientes agricultores
  Para mantener actualizado el directorio de usuarios finales y su estado de servicio

  Background:
    Given el administrador ha iniciado sesión en el portal web administrativo
    And se encuentra en la sección de gestión de clientes

  Scenario: Búsqueda y filtrado en tiempo real de clientes
    When el administrador ingresa un nombre en la barra de búsqueda
    Then el sistema debe filtrar los resultados en tiempo real
    And debe mostrar una lista con nombre, ubicación e indicador semafórico del estado contractual



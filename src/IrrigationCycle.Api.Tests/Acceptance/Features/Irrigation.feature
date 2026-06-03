Feature: Control Hidrico y Riego BDD
  Como usuario de Grotix
  Quiero que el sistema gestione el riego de forma manual y automatica
  Para optimizar el suministro de agua a mis cultivos

  # US20 - Escenario 2: Optimizacion del suministro de agua
  Scenario: Calculo automatico de volumen hidrico
    Given que el cultivo requiere una humedad objetivo de 60%
    When la humedad actual desciende a 40%
    Then el sistema debe calcular una necesidad de 50 litros
    And el tiempo estimado de riego debe ser 10 minutos

  # US19 - Escenario 1: Activacion remota del actuador
  Scenario: Registro de ciclo de riego manual
    Given un usuario que solicita un riego manual para la zona 1
    When se define un volumen de 30 litros y 6 minutos de duracion
    Then el estado del ciclo debe inicializarse como "IN_PROGRESS"

  # US19 - Escenario 2: Desactivacion manual
  Scenario: Aborto de un riego en progreso
    Given un ciclo de riego activo para la zona 1
    When el usuario detiene el riego con el motivo "Lluvia detectada"
    Then el estado del ciclo debe cambiar a "ABORTED"
    And la razon de cancelacion debe ser "Lluvia detectada"
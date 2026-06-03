Feature: Telemetry Ingest BDD
  Como usuario de Grotix
  Quiero que mi sistema detecte con precisión los cambios en mi cultivo y detecte anomalías
  Para confiar en que la información es el reflejo real de mis plantas

  # US11 - Escenario 5: Detección de lecturas anómalas
  Scenario: Deteccion de lectura fisicamente imposible
    Given un sensor con rango fisico entre 0 y 100
    When el sensor envia una lectura de 150
    Then el sistema debe marcar la lectura como invalida

  # US13 - Escenario 2: Filtrado de ruido electrico en la señal
  Scenario: Suavizado de lecturas mediante media movil
    Given que el sensor ha registrado valores previos de 40 y 42
    When llega una nueva lectura de 44 con una ventana de 3
    Then el sistema debe calcular un promedio suavizado de 42

  # US18 - Escenario 1: Alerta por nivel critico
  Scenario: Evaluacion de umbrales criticos
    Given que el umbral minimo es 20 y el maximo es 80
    When se evalua una lectura de 10
    Then el sistema debe determinar que esta fuera de rango
    And la direccion de la brecha debe ser "BELOW_MIN"
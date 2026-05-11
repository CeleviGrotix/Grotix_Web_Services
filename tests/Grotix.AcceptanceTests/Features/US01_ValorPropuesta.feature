# US01 - Visualización de propuesta de valor y servicios
# Epic: E01 - Presencia Digital y Propuesta de Valor

Feature: Visualización de propuesta de valor y servicios
  Como visitante
  Quiero visualizar la información principal del servicio
  Para comprender cómo Grotix soluciona mis problemas de germinación y riego mediante tecnología

  Background:
    Given el visitante accede a la URL de la landing page de Grotix

  Scenario: Explicación clara de la propuesta de valor en Hero Section
    When la página carga completamente
    Then el visitante debe visualizar un título principal (Headline) visible
    And debe aparecer una descripción que mencione que Grotix es una plataforma de riego inteligente con Inteligencia Artificial

  Scenario: Detalle de las funcionalidades principales
    When el visitante se dirige hacia la sección de Servicios
    Then el sistema debe mostrar al menos tres pilares clave
    And uno de los pilares debe ser "Monitoreo por sensores"
    And uno de los pilares debe ser "Reconocimiento de germinación con IA"
    And uno de los pilares debe ser "Riego automatizado"

  Scenario: Comprensibilidad del lenguaje para usuarios no técnicos
    When el visitante navega por las diferentes secciones de información
    Then el texto debe ser legible y sin tecnicismos excesivos
    And el contenido debe ser comprensible para un agricultor sin conocimientos técnicos

  Scenario: Fallo en la carga de recursos visuales por conexión lenta
    Given el visitante tiene una conexión a internet inestable
    When accede a la landing page y las imágenes no cargan
    Then el sistema debe mostrar un color de fondo sólido coherente
    And el texto debe ser legible de inmediato sin depender de los recursos visuales

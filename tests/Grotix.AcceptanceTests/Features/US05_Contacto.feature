# US05 - Implementación de formulario y canales de contacto
# Epic: E01 - Presencia Digital y Propuesta de Valor

Feature: Formulario y canales de contacto
  Como visitante
  Quiero tener un medio de comunicación directo
  Para enviar consultas, reportar problemas o solicitar información personalizada sobre Grotix

  Background:
    Given el visitante se encuentra en la sección de Contacto de la landing page

  Scenario: Envío exitoso del formulario de contacto
    When el visitante completa los campos Nombre, Correo, Asunto y Mensaje
    And envía el formulario
    Then el sistema debe procesar la información correctamente
    And debe mostrar un mensaje de éxito indicando que el mensaje fue enviado

  Scenario: Validación de campos obligatorios vacíos
    When el visitante intenta enviar el formulario con campos obligatorios vacíos
    Then el sistema debe impedir el envío
    And debe resaltar los campos faltantes con un mensaje de error descriptivo

  Scenario: Disponibilidad de métodos de contacto alternativos
    When el visitante visualiza la sección de contacto
    Then el sistema debe mostrar el correo corporativo de Grotix
    And debe mostrar un enlace directo a WhatsApp u otro canal alternativo

  Scenario: Error en el envío por pérdida de conexión
    Given el visitante completa todos los campos del formulario
    When el servicio de mensajería falla por falta de internet
    Then el sistema debe mostrar un mensaje de error claro
    And no debe borrar los datos que el visitante ya escribió en el formulario

  Scenario: Validación de formato de correo electrónico
    Given el visitante ingresa un texto que no tiene formato de correo
    When intenta enviar el formulario
    Then el sistema debe detectar el formato inválido
    And debe pedir al visitante que ingrese un correo electrónico válido

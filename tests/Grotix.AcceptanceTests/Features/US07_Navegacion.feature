# US07 - Implementación de sistemas de navegación simplificada
# Epic: E02 - Optimización de Usabilidad y Rendimiento Web

Feature: Navegación simplificada en la landing page
  Como visitante
  Quiero disponer de elementos de navegación claros y accesibles
  Para encontrar la información que busco sin esfuerzo

  Background:
    Given el visitante se encuentra en la landing page de Grotix

  Scenario: Menú de navegación persistente al hacer scroll
    When el visitante se desplaza hacia abajo en la página
    Then la navegación principal debe mantenerse fija en el borde superior
    And el visitante debe poder acceder a cualquier sección sin volver al inicio

  Scenario: Menú hamburguesa en dispositivos móviles
    Given el visitante accede desde un dispositivo con pantalla reducida
    When el ancho de pantalla es inferior al breakpoint definido
    Then el menú horizontal debe colapsar en un ícono de hamburguesa
    And al presionarlo debe desplegar las opciones de forma vertical y legible

  Scenario: Desplazamiento suave entre secciones
    When el visitante selecciona un enlace del menú que apunta a una sección de la misma página
    Then el desplazamiento debe ser fluido
    And no debe producirse un salto brusco entre secciones

  Scenario: Resaltado de sección activa en el menú
    When el visitante se encuentra visualizando una sección específica
    Then el elemento del menú correspondiente debe cambiar su estilo visualmente
    And debe indicar claramente en qué parte de la página se encuentra el visitante

  Scenario: Cierre automático del menú móvil tras selección
    Given el visitante tiene abierto el menú hamburguesa en su móvil
    When hace clic en un enlace o toca el área fuera del menú
    Then el menú debe cerrarse automáticamente
    And debe mostrarse la sección seleccionada sin obstrucciones

  Scenario: El menú no es tapado por contenido de la página
    When el visitante se desplaza sobre secciones con elementos flotantes o imágenes
    Then el menú debe permanecer siempre en la capa superior
    And ningún contenido de la página debe superponerse sobre el menú

# TS05 - Implementación de Protocolos de Autenticación y Protección de Recursos
# Epic: E09 - Arquitectura de Software y Atributos de Calidad Técnica

Feature: Autenticación JWT y protección de recursos
  Como Developer
  Quiero implementar autenticación basada en JWT en el backend y guardias de navegación en el frontend
  Para asegurar que solo los usuarios y dispositivos autorizados accedan a la API y a los datos de Grotix

  Background:
    Given la API de Grotix está en ejecución y expuesta en la nube

  Scenario: Generación de JWT firmado al autenticarse con credenciales válidas
    Given un usuario tiene una cuenta registrada y activa
    When envía sus credenciales válidas al endpoint POST /api/v1/auth/sign-in
    Then el backend debe generar un JSON Web Token firmado digitalmente
    And el token debe tener un tiempo de expiración definido
    And la respuesta debe incluir el token con status 200 OK

  Scenario: Rechazo de peticiones desde dominios no autorizados por política CORS
    When la API recibe una petición desde un dominio no autorizado
    Then el backend debe rechazar la solicitud
    And debe retornar un error de política CORS

  Scenario: Redirección al Login cuando no existe token válido en el cliente
    Given un usuario no autenticado intenta acceder manualmente a una ruta protegida
    When el sistema detecta que no existe un token válido
    Then el frontend debe redirigir automáticamente al usuario a la pantalla de Login
    And no debe mostrarse ninguna vista privada de la aplicación

  Scenario: Contraseñas almacenadas con hashing robusto al registrar usuario
    Given un nuevo usuario completa el formulario de registro
    When el backend procesa la solicitud de creación de cuenta en POST /api/v1/auth/register
    Then el sistema debe aplicar un algoritmo de hashing a la contraseña
    And la contraseña nunca debe guardarse en texto plano en la base de datos

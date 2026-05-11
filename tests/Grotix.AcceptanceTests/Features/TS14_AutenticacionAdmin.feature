# TS14 - Autenticación y Control de Acceso para el Portal de Administración
# Epic: E11 - Supervisión y Operación de Infraestructura IoT

Feature: Autenticación y control de acceso para el portal de administración
  Como Administrador de Grotix
  Quiero contar con un sistema de inicio y cierre de sesión exclusivo para el portal web administrativo
  Para proteger la gestión de clientes y dispositivos

  Background:
    Given el portal web administrativo de Grotix está disponible

  Scenario: Login exitoso del administrador con privilegios elevados
    Given el administrador ingresa sus credenciales válidas en el portal web
    When los datos son validados contra el rol de "Admin" en el backend
    Then el sistema debe otorgar un token JWT con privilegios elevados
    And debe redirigir al administrador al Dashboard Administrativo

  Scenario: Cierre de sesión invalida el token y bloquea acceso
    Given el administrador ha finalizado su jornada de trabajo
    When selecciona la opción "Salir" en el portal
    Then el sistema debe invalidar el token de sesión actual
    And debe denegar el acceso a todas las rutas protegidas del portal
    And debe redirigir al administrador a la pantalla de login

#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

/// <summary>
/// Roles: admin, staff, user_admin, user_basic, user_advanced.
/// Permisos y vínculos role_permission según especificación de producto.
/// </summary>
public partial class UpdateRolesAndPermissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO role (RoleID, Name, Description)
            SELECT 4, 'user_basic', 'Agricultor básico: telemetría y riego manual.'
            WHERE NOT EXISTS (SELECT 1 FROM role WHERE RoleID = 4);

            INSERT INTO role (RoleID, Name, Description)
            SELECT 5, 'user_advanced', 'Agricultor avanzado: umbrales, exportación, análisis y dispositivos.'
            WHERE NOT EXISTS (SELECT 1 FROM role WHERE RoleID = 5);

            UPDATE user SET RoleID = 4 WHERE RoleID = 3;

            UPDATE role SET Name = 'user_admin', Description = 'Gestor dentro de una organización (invitaciones, miembros y roles).'
            WHERE RoleID = 3;

            UPDATE role SET Name = 'admin', Description = 'Administrador del sistema.'
            WHERE RoleID = 1;

            UPDATE role SET Name = 'staff', Description = 'Operador técnico.'
            WHERE RoleID = 2;
            """);

        migrationBuilder.Sql("""
            INSERT INTO permission (Code, Description)
            SELECT 'TELEMETRY_VIEW', 'Ver gráficas de sensores en tiempo real.'
            WHERE NOT EXISTS (SELECT 1 FROM permission WHERE Code = 'TELEMETRY_VIEW');

            INSERT INTO permission (Code, Description)
            SELECT 'TELEMETRY_EXPORT', 'Descargar reportes Excel/CSV de históricos.'
            WHERE NOT EXISTS (SELECT 1 FROM permission WHERE Code = 'TELEMETRY_EXPORT');

            INSERT INTO permission (Code, Description)
            SELECT 'ANALYSIS_VIEW', 'Ver estados de crecimiento y predicciones de IA.'
            WHERE NOT EXISTS (SELECT 1 FROM permission WHERE Code = 'ANALYSIS_VIEW');

            INSERT INTO permission (Code, Description)
            SELECT 'MANUAL_CONTROL_EXECUTE', 'Ejecutar riego manual (encender ahora).'
            WHERE NOT EXISTS (SELECT 1 FROM permission WHERE Code = 'MANUAL_CONTROL_EXECUTE');

            INSERT INTO permission (Code, Description)
            SELECT 'THRESHOLD_WRITE', 'Modificar umbrales de temperatura/humedad para riego automático.'
            WHERE NOT EXISTS (SELECT 1 FROM permission WHERE Code = 'THRESHOLD_WRITE');

            INSERT INTO permission (Code, Description)
            SELECT 'DEVICE_CONFIG', 'Cambiar nombre del sensor o calibración.'
            WHERE NOT EXISTS (SELECT 1 FROM permission WHERE Code = 'DEVICE_CONFIG');

            INSERT INTO permission (Code, Description)
            SELECT 'USER_INVITE', 'Enviar invitaciones por email a nuevos agricultores.'
            WHERE NOT EXISTS (SELECT 1 FROM permission WHERE Code = 'USER_INVITE');

            INSERT INTO permission (Code, Description)
            SELECT 'USER_DELETE', 'Quitar acceso a un trabajador de la organización.'
            WHERE NOT EXISTS (SELECT 1 FROM permission WHERE Code = 'USER_DELETE');

            INSERT INTO permission (Code, Description)
            SELECT 'ROLE_ASSIGN', 'Cambiar rol entre basic y advanced u otros permitidos.'
            WHERE NOT EXISTS (SELECT 1 FROM permission WHERE Code = 'ROLE_ASSIGN');

            INSERT INTO permission (Code, Description)
            SELECT 'SYSTEM_LOGS_VIEW', 'Ver errores del servidor o caídas de red.'
            WHERE NOT EXISTS (SELECT 1 FROM permission WHERE Code = 'SYSTEM_LOGS_VIEW');

            INSERT INTO permission (Code, Description)
            SELECT 'HARDWARE_DIAGNOSTIC', 'Ejecutar pruebas de ping o estado de batería en nodos.'
            WHERE NOT EXISTS (SELECT 1 FROM permission WHERE Code = 'HARDWARE_DIAGNOSTIC');
            """);

        // admin (1): todos los permisos
        migrationBuilder.Sql("""
            INSERT INTO role_permission (RoleID, PermissionID)
            SELECT 1, p.PermissionID FROM permission p
            WHERE p.Code IN (
                'TELEMETRY_VIEW','TELEMETRY_EXPORT','ANALYSIS_VIEW','MANUAL_CONTROL_EXECUTE','THRESHOLD_WRITE',
                'DEVICE_CONFIG','USER_INVITE','USER_DELETE','ROLE_ASSIGN','SYSTEM_LOGS_VIEW','HARDWARE_DIAGNOSTIC')
            AND NOT EXISTS (SELECT 1 FROM role_permission rp WHERE rp.RoleID = 1 AND rp.PermissionID = p.PermissionID);
            """);

        // staff (2): logs, hardware, dispositivos
        migrationBuilder.Sql("""
            INSERT INTO role_permission (RoleID, PermissionID)
            SELECT 2, p.PermissionID FROM permission p
            WHERE p.Code IN ('SYSTEM_LOGS_VIEW','HARDWARE_DIAGNOSTIC','DEVICE_CONFIG')
            AND NOT EXISTS (SELECT 1 FROM role_permission rp WHERE rp.RoleID = 2 AND rp.PermissionID = p.PermissionID);
            """);

        // user_admin (3): sin DEVICE_CONFIG ni logs/diagnóstico de staff
        migrationBuilder.Sql("""
            INSERT INTO role_permission (RoleID, PermissionID)
            SELECT 3, p.PermissionID FROM permission p
            WHERE p.Code IN (
                'TELEMETRY_VIEW','TELEMETRY_EXPORT','ANALYSIS_VIEW','MANUAL_CONTROL_EXECUTE','THRESHOLD_WRITE',
                'USER_INVITE','USER_DELETE','ROLE_ASSIGN')
            AND NOT EXISTS (SELECT 1 FROM role_permission rp WHERE rp.RoleID = 3 AND rp.PermissionID = p.PermissionID);
            """);

        // user_basic (4)
        migrationBuilder.Sql("""
            INSERT INTO role_permission (RoleID, PermissionID)
            SELECT 4, p.PermissionID FROM permission p
            WHERE p.Code IN ('TELEMETRY_VIEW','MANUAL_CONTROL_EXECUTE')
            AND NOT EXISTS (SELECT 1 FROM role_permission rp WHERE rp.RoleID = 4 AND rp.PermissionID = p.PermissionID);
            """);

        // user_advanced (5)
        migrationBuilder.Sql("""
            INSERT INTO role_permission (RoleID, PermissionID)
            SELECT 5, p.PermissionID FROM permission p
            WHERE p.Code IN (
                'TELEMETRY_VIEW','TELEMETRY_EXPORT','ANALYSIS_VIEW','MANUAL_CONTROL_EXECUTE','THRESHOLD_WRITE','DEVICE_CONFIG')
            AND NOT EXISTS (SELECT 1 FROM role_permission rp WHERE rp.RoleID = 5 AND rp.PermissionID = p.PermissionID);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE rp FROM role_permission rp
            INNER JOIN permission p ON p.PermissionID = rp.PermissionID
            WHERE p.Code IN (
                'TELEMETRY_VIEW','TELEMETRY_EXPORT','ANALYSIS_VIEW','MANUAL_CONTROL_EXECUTE','THRESHOLD_WRITE',
                'DEVICE_CONFIG','USER_INVITE','USER_DELETE','ROLE_ASSIGN','SYSTEM_LOGS_VIEW','HARDWARE_DIAGNOSTIC');

            DELETE FROM permission WHERE Code IN (
                'TELEMETRY_VIEW','TELEMETRY_EXPORT','ANALYSIS_VIEW','MANUAL_CONTROL_EXECUTE','THRESHOLD_WRITE',
                'DEVICE_CONFIG','USER_INVITE','USER_DELETE','ROLE_ASSIGN','SYSTEM_LOGS_VIEW','HARDWARE_DIAGNOSTIC');

            UPDATE user SET RoleID = 3 WHERE RoleID = 4;

            DELETE FROM role WHERE RoleID IN (4, 5);

            UPDATE role SET Name = 'Admin', Description = 'Administrador del sistema' WHERE RoleID = 1;
            UPDATE role SET Name = 'Staff', Description = 'Operador técnico' WHERE RoleID = 2;
            UPDATE role SET Name = 'User', Description = 'Usuario estándar' WHERE RoleID = 3;
            """);
    }
}

Feature: Autenticacion de usuarios
  Como usuario del sistema
  Quiero iniciar sesion con mis credenciales
  Para obtener acceso al sistema

  Background:
    Given que existe un rol "Mecanico" con ID 3
    And que existe un usuario "jperez" con contrasenna "Taller2024!" y rol "Mecanico"

  Scenario: Inicio de sesion exitoso
    When el usuario inicia sesion con "jperez" y "Taller2024!"
    Then recibe un token JWT valido

  Scenario: Contrasenna incorrecta
    When el usuario inicia sesion con "jperez" y "incorrecta"
    Then el sistema rechaza el acceso con "Credenciales invalidas."

  Scenario: Usuario inexistente
    When el usuario inicia sesion con "noexiste" y "cualquiera"
    Then el sistema rechaza el acceso con "Credenciales invalidas."

  Scenario: Usuario inactivo
    Given que el usuario "jperez" esta desactivado
    When el usuario inicia sesion con "jperez" y "Taller2024!"
    Then el sistema rechaza el acceso con "Credenciales invalidas."

  Scenario: Refrescar sesion con un token de refresco valido lo rota
    When el usuario inicia sesion con "jperez" y "Taller2024!"
    And el usuario refresca la sesion con el token de refresco recibido
    Then recibe un token JWT valido
    And el token de refresco recibido es distinto al anterior

  Scenario: Reproducir un token de refresco ya usado es rechazado
    When el usuario inicia sesion con "jperez" y "Taller2024!"
    And el usuario refresca la sesion con el token de refresco recibido
    And el usuario refresca la sesion con el token de refresco anterior
    Then el sistema rechaza el acceso con "Token de refresco invalido o vencido."

  Scenario: Refrescar sesion con un token de refresco vencido es rechazado
    When el usuario inicia sesion con "jperez" y "Taller2024!"
    And el token de refresco recibido esta vencido
    And el usuario refresca la sesion con el token de refresco recibido
    Then el sistema rechaza el acceso con "Token de refresco invalido o vencido."

  Scenario: Cerrar sesion revoca el token de refresco
    When el usuario inicia sesion con "jperez" y "Taller2024!"
    And el usuario cierra sesion
    And el usuario refresca la sesion con el token de refresco recibido
    Then el sistema rechaza el acceso con "Token de refresco invalido o vencido."

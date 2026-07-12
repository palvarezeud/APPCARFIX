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

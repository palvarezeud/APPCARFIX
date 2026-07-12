Feature: Gestion de clientes
  Como mecanico del taller
  Quiero gestionar clientes
  Para mantener un registro actualizado

  Scenario: Crear cliente exitosamente
    When creo un cliente con nombre "Juan Perez" y telefono "8888-1234"
    Then el cliente fue creado con ID mayor a cero

  Scenario: No se puede crear cliente sin nombre
    When creo un cliente sin nombre y telefono "8888-1234"
    Then la validacion falla con "El nombre del cliente es requerido."

  Scenario: No se puede crear cliente sin telefono
    When creo un cliente con nombre "Ana Gomez" y sin telefono
    Then la validacion falla con "El telefono principal es requerido."

  Scenario: Eliminar cliente sin facturas
    Given existe un cliente "Maria Lopez" con telefono "7777-0001"
    When elimino el cliente "Maria Lopez"
    Then la operacion fue exitosa

  Scenario: No se puede eliminar cliente con facturas asociadas
    Given existe un cliente "Carlos Mora" con telefono "6666-0002"
    And ese cliente tiene una factura registrada
    When elimino el cliente "Carlos Mora"
    Then la operacion falla con "No se puede eliminar un cliente que tiene facturas asociadas."

Feature: Gestion de facturas
  Como mecanico del taller
  Quiero gestionar el estado de las facturas
  Para controlar el cobro a los clientes

  Background:
    Given existe un cliente "Luis Blanco" con telefono "4444-0001"
    And ese cliente tiene un vehiculo "Honda" "Civic"
    And ese vehiculo tiene una factura en estado "Cotizacion"

  Scenario: Cambiar factura de Cotizacion a Pendiente
    When cambio el estado de la factura a 2
    Then la factura tiene estado 2

  Scenario: Cambiar factura a Pagada
    When cambio el estado de la factura a 3
    Then la factura tiene estado 3

  Scenario: Estado de factura invalido
    When cambio el estado de la factura a 99
    Then la validacion falla con "Estado de factura no valido. Los valores permitidos son del 1 al 3."

  Scenario: Agregar una reparacion a la factura
    When agrego una reparacion "Cambio de aceite" con costo 15000 a la factura
    Then la factura tiene total de reparaciones 15000

  Scenario: Agregar un repuesto a la factura
    When agrego un repuesto "Filtro de aceite" con costo 5000 a la factura
    Then la factura tiene total de repuestos 5000

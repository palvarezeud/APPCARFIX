Feature: Gestion de ordenes de servicio
  Como mecanico del taller
  Quiero gestionar el estado de las ordenes de servicio
  Para controlar el flujo de reparacion de vehiculos

  Background:
    Given existe un cliente "Pedro Vargas" con telefono "5555-0001"
    And ese cliente tiene un vehiculo "Toyota" "Corolla"
    And ese vehiculo tiene una orden de servicio en estado "Cotizacion"

  Scenario: Cambiar orden de Cotizacion a Recibido
    When cambio el estado de la orden a 2
    Then la orden tiene estado 2

  Scenario: Cambiar orden a Finalizado
    When cambio el estado de la orden a 4
    Then la orden tiene estado 4

  Scenario: Estado de orden invalido
    When cambio el estado de la orden a 99
    Then la validacion falla con "Estado de orden no valido. Los valores permitidos son del 1 al 5."

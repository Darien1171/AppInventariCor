using System;
using System.Collections.Generic;

namespace AppInventariCor.Models
{
    public class KpiItem
    {
        public string Titulo { get; set; }
        public string Valor { get; set; }
        public string Unidad { get; set; }
        public double? PorcentajeCambio { get; set; }
        public bool EsCambioPositivo { get; set; } = true;
        public string Icono { get; set; }
    }

    public class TransaccionReciente
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoTransaccion { get; set; }
        public string CodigoRepuesto { get; set; }
        public string NombreRepuesto { get; set; }
        public int Cantidad { get; set; }
        public string? VehiculoPlaca { get; set; }
    }

    public class AlertaStockBajo
    {
        public int RepuestoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public int CantidadActual { get; set; }
        public int StockMinimo { get; set; }
        public string NivelAlerta { get; set; } // Bajo, Crítico, Agotado
    }
}
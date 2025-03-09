using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppInventariCor.Models
{
    public class Transaccion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public TipoTransaccion Tipo { get; set; }

        [Required]
        public int Cantidad { get; set; }

        public string? Observaciones { get; set; }

        public string? ResponsableNombre { get; set; }

        public string? FirmaUrl { get; set; }

        // Relaciones
        public int RepuestoId { get; set; }

        [ForeignKey("RepuestoId")]
        public Repuesto Repuesto { get; set; }

        public int? VehiculoId { get; set; }

        [ForeignKey("VehiculoId")]
        public Vehiculo? Vehiculo { get; set; }

        // Imágenes de evidencia
        public List<string> EvidenciaImagenUrls { get; set; } = new List<string>();
    }

    public enum TipoTransaccion
    {
        Entrada,
        Salida,
        Devolucion,
        Ajuste
    }
}
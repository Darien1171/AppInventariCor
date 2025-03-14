using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

        // Campos mejorados para historial
        public int CantidadAnterior { get; set; }
        public int CantidadPosterior { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal ValorTotal { get; set; }

        public string? Observaciones { get; set; }
        public string? ResponsableNombre { get; set; }

        // Datos para entradas
        public string? ProveedorNombre { get; set; }
        public string? NumeroFactura { get; set; }

        public string? FirmaUrl { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioSistemaId { get; set; }

        // Relaciones por ID
        public int RepuestoId { get; set; }

        // Propiedades de navegación para UI, marcadas para ignorar en serialización JSON
        [JsonIgnore]
        public Repuesto? Repuesto { get; set; }

        // Datos de referencia rápida (evita necesidad de cargar todo el repuesto)
        public string RepuestoCodigo { get; set; }
        public string RepuestoNombre { get; set; }

        public int? VehiculoId { get; set; }
        public string? VehiculoPlaca { get; set; }

        [JsonIgnore]
        public Vehiculo? Vehiculo { get; set; }

        // Evidencia
        public List<string> EvidenciaImagenUrls { get; set; } = new List<string>();
    }
}
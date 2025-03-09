using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace AppInventariCor.Models
{
    public class Vehiculo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NumeroPlaca { get; set; }

        [Required]
        public string NumeroInterno { get; set; }

        [Required]
        public string Marca { get; set; }

        public string Modelo { get; set; }

        public string? NumeroSerie { get; set; }

        public string? NumeroMotor { get; set; }

        public int? Anio { get; set; }

        public string? Color { get; set; }

        public string? Propietario { get; set; }

        public DateTime? UltimoMantenimiento { get; set; }

        public string EstadoMantenimiento { get; set; } = "Normal"; // Normal, Pendiente, Urgente

        // Relaciones
        public List<string> ImagenesUrls { get; set; } = new List<string>();
        public List<Transaccion> HistorialTransacciones { get; set; } = new List<Transaccion>();

        public List<Documento> Documentos { get; set; } = new List<Documento>();
    }

    public class Documento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Tipo { get; set; } // Seguro, Revisión Técnica, etc.

        public string? Descripcion { get; set; }

        public DateTime FechaEmision { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        public string? ArchivoUrl { get; set; }

        public int VehiculoId { get; set; }

        [ForeignKey("VehiculoId")]
        public Vehiculo Vehiculo { get; set; }
    }
}
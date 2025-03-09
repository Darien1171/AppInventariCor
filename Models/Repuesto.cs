using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace AppInventariCor.Models
{
    public class Repuesto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Codigo { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string Categoria { get; set; }

        public string Marca { get; set; }

        public string Modelo { get; set; }

        [Required]
        public decimal Precio { get; set; }

        [Required]
        public int Cantidad { get; set; }

        public int StockMinimo { get; set; }

        public int StockOptimo { get; set; }

        public string Ubicacion { get; set; }

        public bool Disponible => Cantidad > 0;

        public bool StockBajo => Cantidad <= StockMinimo && Cantidad > 0;

        public string? CodigoBarras { get; set; }

        public string? CodigoQR { get; set; }

        // Relaciones
        public List<string> ImagenesUrls { get; set; } = new List<string>();

        public List<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    }
}
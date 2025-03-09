using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using AppInventariCor.Models;

namespace AppInventariCor.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public ObservableCollection<KpiItem> Kpis { get; set; }
        public ObservableCollection<TransaccionReciente> TransaccionesRecientes { get; set; }
        public ObservableCollection<AlertaStockBajo> AlertasStockBajo { get; set; }

        // Comandos
        public ICommand RefreshCommand { get; }
        public ICommand NuevaTransaccionCommand { get; }
        public ICommand VerDetalleRepuestoCommand { get; }
        public ICommand EscanearCodigoBarrasCommand { get; }

        public DashboardViewModel()
        {
            Title = "Dashboard";

            // Inicializar comandos (que no harán nada en esta versión simplificada)
            RefreshCommand = CreateCommand();
            NuevaTransaccionCommand = CreateCommand();
            VerDetalleRepuestoCommand = CreateCommand();
            EscanearCodigoBarrasCommand = CreateCommand();

            // Cargar datos de muestra
            CargarDatosMuestra();
        }

        private void CargarDatosMuestra()
        {
            // KPIs
            Kpis = new ObservableCollection<KpiItem>
            {
                new KpiItem { Titulo = "Total Repuestos", Valor = "358", Unidad = "items", Icono = "inventory" },
                new KpiItem { Titulo = "Valor Inventario", Valor = "$45,789.00", Unidad = "", Icono = "monetization_on" },
                new KpiItem { Titulo = "Alertas Stock", Valor = "12", Unidad = "items", Icono = "warning", EsCambioPositivo = false },
                new KpiItem { Titulo = "Vehículos", Valor = "27", Unidad = "vehículos", Icono = "directions_bus" }
            };

            // Transacciones recientes
            TransaccionesRecientes = new ObservableCollection<TransaccionReciente>
            {
                new TransaccionReciente { Id = 1, Fecha = DateTime.Now.AddDays(-1), TipoTransaccion = "Entrada", CodigoRepuesto = "FRN-2345", NombreRepuesto = "Filtro de aire", Cantidad = 5 },
                new TransaccionReciente { Id = 2, Fecha = DateTime.Now.AddDays(-2), TipoTransaccion = "Salida", CodigoRepuesto = "BDJ-5621", NombreRepuesto = "Bombilla delantera", Cantidad = 2, VehiculoPlaca = "ABC123" },
                new TransaccionReciente { Id = 3, Fecha = DateTime.Now.AddDays(-3), TipoTransaccion = "Entrada", CodigoRepuesto = "ACT-7845", NombreRepuesto = "Aceite transmisión", Cantidad = 10 },
                new TransaccionReciente { Id = 4, Fecha = DateTime.Now.AddDays(-5), TipoTransaccion = "Salida", CodigoRepuesto = "LQF-1267", NombreRepuesto = "Líquido de frenos", Cantidad = 1, VehiculoPlaca = "XYZ789" },
                new TransaccionReciente { Id = 5, Fecha = DateTime.Now.AddDays(-6), TipoTransaccion = "Entrada", CodigoRepuesto = "PLT-9023", NombreRepuesto = "Pastillas de freno", Cantidad = 8 }
            };

            // Alertas de stock bajo
            AlertasStockBajo = new ObservableCollection<AlertaStockBajo>
            {
                new AlertaStockBajo { RepuestoId = 1, Codigo = "FRN-2345", Nombre = "Filtro de aire", CantidadActual = 3, StockMinimo = 5, NivelAlerta = "Bajo" },
                new AlertaStockBajo { RepuestoId = 2, Codigo = "ACT-7845", Nombre = "Aceite transmisión", CantidadActual = 1, StockMinimo = 8, NivelAlerta = "Crítico" },
                new AlertaStockBajo { RepuestoId = 3, Codigo = "PLT-9023", Nombre = "Pastillas de freno", CantidadActual = 0, StockMinimo = 4, NivelAlerta = "Agotado" },
                new AlertaStockBajo { RepuestoId = 4, Codigo = "BLB-3478", Nombre = "Bombillas traseras", CantidadActual = 2, StockMinimo = 6, NivelAlerta = "Bajo" }
            };
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using AppInventariCor.Models;
using System.Linq;
using System.Diagnostics;

namespace AppInventariCor.ViewModels
{
    public class RepuestoDetalleViewModel : BaseViewModel
    {
        private Repuesto _repuesto;
        private ObservableCollection<Transaccion> _transacciones;
        private string _estadoStock;

        public Repuesto Repuesto
        {
            get => _repuesto;
            set
            {
                if (SetProperty(ref _repuesto, value))
                {
                    ActualizarEstadoStock();
                    CargarTransacciones();
                    OnPropertyChanged(nameof(IsTransaccionesEmpty));
                    Title = _repuesto?.Nombre ?? "Detalle de Repuesto";

                    // Agregar depuración para verificar que el repuesto se está estableciendo
                    Debug.WriteLine($"Repuesto establecido: {_repuesto?.Nombre}, ID: {_repuesto?.Id}");
                }
            }
        }

        public ObservableCollection<Transaccion> Transacciones
        {
            get => _transacciones;
            set => SetProperty(ref _transacciones, value);
        }

        public string EstadoStock
        {
            get => _estadoStock;
            set => SetProperty(ref _estadoStock, value);
        }

        public bool IsTransaccionesEmpty => Transacciones == null || Transacciones.Count == 0;

        // Comandos
        public ICommand EntradaCommand { get; }
        public ICommand SalidaCommand { get; }
        public ICommand AjustarCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }

        // Constructor sin parámetros
        public RepuestoDetalleViewModel()
        {
            Debug.WriteLine("RepuestoDetalleViewModel: Constructor vacío llamado");

            Transacciones = new ObservableCollection<Transaccion>();

            // Inicializar comandos
            EntradaCommand = new Command(OnEntrada);
            SalidaCommand = new Command(OnSalida);
            AjustarCommand = new Command(OnAjustar);
            EditarCommand = new Command(OnEditar);
            EliminarCommand = new Command(OnEliminar);

            Title = "Detalle de Repuesto";
        }

        // Constructor con repuesto
        public RepuestoDetalleViewModel(Repuesto repuesto) : this()
        {
            Debug.WriteLine($"RepuestoDetalleViewModel: Constructor con repuesto llamado. ID={repuesto?.Id}, Nombre={repuesto?.Nombre}");
            // Establecer el repuesto después de inicializar todo lo demás
            Repuesto = repuesto;
        }

        private void ActualizarEstadoStock()
        {
            if (Repuesto == null)
            {
                EstadoStock = "NO DISPONIBLE";
                return;
            }

            if (Repuesto.Cantidad == 0)
            {
                EstadoStock = "AGOTADO";
            }
            else if (Repuesto.Cantidad <= Repuesto.StockMinimo)
            {
                EstadoStock = "BAJO";
            }
            else if (Repuesto.Cantidad < Repuesto.StockOptimo)
            {
                EstadoStock = "NORMAL";
            }
            else
            {
                EstadoStock = "ÓPTIMO";
            }

            Debug.WriteLine($"Estado stock actualizado a: {EstadoStock}");
        }

        private void CargarTransacciones()
        {
            // En una implementación real, aquí se cargarían las transacciones desde una base de datos
            // Para este ejemplo, crearemos algunas transacciones de muestra

            if (Repuesto == null) return;

            Transacciones.Clear();

            // Datos de muestra para el historial de transacciones
            Transacciones.Add(new Transaccion
            {
                Id = 1,
                Fecha = DateTime.Now.AddDays(-14),
                Tipo = TipoTransaccion.Entrada,
                Cantidad = 10,
                RepuestoId = Repuesto.Id,
                Repuesto = Repuesto,
                ResponsableNombre = "Juan Perez"
            });

            Transacciones.Add(new Transaccion
            {
                Id = 2,
                Fecha = DateTime.Now.AddDays(-7),
                Tipo = TipoTransaccion.Salida,
                Cantidad = 3,
                RepuestoId = Repuesto.Id,
                Repuesto = Repuesto,
                ResponsableNombre = "Maria Lopez",
                VehiculoId = 1,
                Vehiculo = new Vehiculo { NumeroPlaca = "ABC-123" }
            });

            Transacciones.Add(new Transaccion
            {
                Id = 3,
                Fecha = DateTime.Now.AddDays(-2),
                Tipo = TipoTransaccion.Ajuste,
                Cantidad = 1,
                RepuestoId = Repuesto.Id,
                Repuesto = Repuesto,
                ResponsableNombre = "Carlos Rodriguez",
                Observaciones = "Ajuste por inventario físico"
            });

            Debug.WriteLine($"Cargadas {Transacciones.Count} transacciones para el repuesto {Repuesto.Id}");
            OnPropertyChanged(nameof(IsTransaccionesEmpty));
        }

        // Implementaciones de comandos (versiones simplificadas)
        private void OnEntrada()
        {
            Debug.WriteLine("Comando Entrada ejecutado");
        }

        private void OnSalida()
        {
            Debug.WriteLine("Comando Salida ejecutado");
        }

        private void OnAjustar()
        {
            Debug.WriteLine("Comando Ajustar ejecutado");
        }

        private void OnEditar()
        {
            Debug.WriteLine("Comando Editar ejecutado");
        }

        private void OnEliminar()
        {
            Debug.WriteLine("Comando Eliminar ejecutado");
        }
    }
}
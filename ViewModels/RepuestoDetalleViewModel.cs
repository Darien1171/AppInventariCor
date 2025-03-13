using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using AppInventariCor.Models;
using System.Linq;

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
                    Title = _repuesto?.Nombre;
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

        public RepuestoDetalleViewModel(Repuesto repuesto = null)
        {
            Repuesto = repuesto;
            Transacciones = new ObservableCollection<Transaccion>();

            // Inicializar comandos
            EntradaCommand = CreateCommand(OnEntrada);
            SalidaCommand = CreateCommand(OnSalida);
            AjustarCommand = CreateCommand(OnAjustar);
            EditarCommand = CreateCommand(OnEditar);
            EliminarCommand = CreateCommand(OnEliminar);

            // Si se proporciona un repuesto al constructor, cargar sus datos relacionados
            if (repuesto != null)
            {
                CargarTransacciones();
            }
        }

        private void ActualizarEstadoStock()
        {
            if (Repuesto == null) return;

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

            OnPropertyChanged(nameof(IsTransaccionesEmpty));
        }

        // Implementaciones de comandos
        private async void OnEntrada()
        {
            string cantidad = await Application.Current.MainPage.DisplayPromptAsync(
                "Entrada de Inventario",
                $"Ingrese la cantidad a añadir de {Repuesto.Nombre}:",
                "Confirmar",
                "Cancelar",
                "Cantidad",
                -1,
                Keyboard.Numeric);

            if (string.IsNullOrEmpty(cantidad)) return;

            if (int.TryParse(cantidad, out int cantidadNum) && cantidadNum > 0)
            {
                // En una aplicación real, aquí se registraría la transacción en la base de datos
                // Actualizamos localmente para la demostración

                Repuesto.Cantidad += cantidadNum;

                Transacciones.Insert(0, new Transaccion
                {
                    Id = Transacciones.Count + 1,
                    Fecha = DateTime.Now,
                    Tipo = TipoTransaccion.Entrada,
                    Cantidad = cantidadNum,
                    RepuestoId = Repuesto.Id,
                    Repuesto = Repuesto,
                    ResponsableNombre = "Usuario Actual"
                });

                ActualizarEstadoStock();
                OnPropertyChanged(nameof(IsTransaccionesEmpty));

                await Application.Current.MainPage.DisplayAlert(
                    "Entrada Registrada",
                    $"Se han añadido {cantidadNum} unidades al inventario.",
                    "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Por favor, ingrese un número válido mayor que cero.",
                    "OK");
            }
        }

        private async void OnSalida()
        {
            string cantidad = await Application.Current.MainPage.DisplayPromptAsync(
                "Salida de Inventario",
                $"Ingrese la cantidad a retirar de {Repuesto.Nombre}:",
                "Confirmar",
                "Cancelar",
                "Cantidad",
                -1,
                Keyboard.Numeric);

            if (string.IsNullOrEmpty(cantidad)) return;

            if (int.TryParse(cantidad, out int cantidadNum) && cantidadNum > 0)
            {
                if (cantidadNum > Repuesto.Cantidad)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"No hay suficiente stock. Stock actual: {Repuesto.Cantidad}",
                        "OK");
                    return;
                }

                // En una aplicación real, aquí se registraría la transacción en la base de datos
                // Actualizamos localmente para la demostración

                Repuesto.Cantidad -= cantidadNum;

                Transacciones.Insert(0, new Transaccion
                {
                    Id = Transacciones.Count + 1,
                    Fecha = DateTime.Now,
                    Tipo = TipoTransaccion.Salida,
                    Cantidad = cantidadNum,
                    RepuestoId = Repuesto.Id,
                    Repuesto = Repuesto,
                    ResponsableNombre = "Usuario Actual"
                });

                ActualizarEstadoStock();
                OnPropertyChanged(nameof(IsTransaccionesEmpty));

                await Application.Current.MainPage.DisplayAlert(
                    "Salida Registrada",
                    $"Se han retirado {cantidadNum} unidades del inventario.",
                    "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Por favor, ingrese un número válido mayor que cero.",
                    "OK");
            }
        }

        private async void OnAjustar()
        {
            string cantidad = await Application.Current.MainPage.DisplayPromptAsync(
                "Ajuste de Inventario",
                $"Ingrese la cantidad exacta en stock de {Repuesto.Nombre}:",
                "Confirmar",
                "Cancelar",
                "Cantidad",
                -1,
                Keyboard.Numeric);

            if (string.IsNullOrEmpty(cantidad)) return;

            if (int.TryParse(cantidad, out int cantidadNum) && cantidadNum >= 0)
            {
                int diferencia = cantidadNum - Repuesto.Cantidad;

                // En una aplicación real, aquí se registraría la transacción en la base de datos
                // Actualizamos localmente para la demostración

                Repuesto.Cantidad = cantidadNum;

                Transacciones.Insert(0, new Transaccion
                {
                    Id = Transacciones.Count + 1,
                    Fecha = DateTime.Now,
                    Tipo = TipoTransaccion.Ajuste,
                    Cantidad = Math.Abs(diferencia),
                    RepuestoId = Repuesto.Id,
                    Repuesto = Repuesto,
                    ResponsableNombre = "Usuario Actual",
                    Observaciones = diferencia >= 0 ? "Ajuste positivo" : "Ajuste negativo"
                });

                ActualizarEstadoStock();
                OnPropertyChanged(nameof(IsTransaccionesEmpty));

                await Application.Current.MainPage.DisplayAlert(
                    "Ajuste Registrado",
                    $"El stock se ha ajustado a {cantidadNum} unidades.",
                    "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Por favor, ingrese un número válido mayor o igual a cero.",
                    "OK");
            }
        }

        private async void OnEditar()
        {
            // En una implementación real, aquí navegaríamos a una página de edición
            await Application.Current.MainPage.DisplayAlert(
                "Editar Repuesto",
                "Esta funcionalidad se implementará próximamente.",
                "OK");
        }

        private async void OnEliminar()
        {
            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Eliminar Repuesto",
                $"¿Está seguro que desea eliminar {Repuesto.Nombre}? Esta acción no se puede deshacer.",
                "Eliminar",
                "Cancelar");

            if (confirmar)
            {
                // En una implementación real, aquí se eliminaría el repuesto de la base de datos
                // Para esta demostración, solo mostramos un mensaje

                await Application.Current.MainPage.DisplayAlert(
                    "Repuesto Eliminado",
                    $"{Repuesto.Nombre} ha sido eliminado del inventario.",
                    "OK");

                // Típicamente después de eliminar, volveríamos a la página de inventario
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}
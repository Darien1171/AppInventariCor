using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using AppInventariCor.Models;
using System.Linq;
using System.Diagnostics;
using AppInventariCor.Services;

namespace AppInventariCor.ViewModels
{
    [QueryProperty(nameof(RepuestoId), "RepuestoId")]
    public class RepuestoDetalleViewModel : BaseViewModel
    {
        private int _repuestoId;
        private Repuesto _repuesto;
        private ObservableCollection<Transaccion> _transacciones;
        private string _estadoStock;
        private bool _isLoading = true;

        public int RepuestoId
        {
            get => _repuestoId;
            set
            {
                _repuestoId = value;
                LoadRepuestoAsync(value);
            }
        }

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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Comandos
        public ICommand EntradaCommand { get; }
        public ICommand SalidaCommand { get; }
        public ICommand AjustarCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }

        // Constructor
        public RepuestoDetalleViewModel()
        {
            Debug.WriteLine("RepuestoDetalleViewModel: Constructor llamado");

            Transacciones = new ObservableCollection<Transaccion>();

            // Inicializar comandos
            EntradaCommand = new Command(OnEntrada);
            SalidaCommand = new Command(OnSalida);
            AjustarCommand = new Command(OnAjustar);
            EditarCommand = new Command(OnEditar);
            EliminarCommand = new Command(OnEliminar);

            Title = "Cargando repuesto...";
        }

        private async void LoadRepuestoAsync(int repuestoId)
        {
            try
            {
                IsLoading = true;

                // Cargar todos los repuestos desde JSON
                var repuestos = await RepuestoJson.ObtenerRepuestos();

                // Buscar el repuesto específico por ID
                var repuesto = repuestos.FirstOrDefault(r => r.Id == repuestoId);

                if (repuesto != null)
                {
                    Repuesto = repuesto;
                    Debug.WriteLine($"Repuesto cargado: {repuesto.Nombre}, ID: {repuesto.Id}");
                }
                else
                {
                    Debug.WriteLine($"No se encontró repuesto con ID: {repuestoId}");
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se encontró el repuesto solicitado.",
                        "OK");

                    // Volver a la página anterior si no se encuentra el repuesto
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar repuesto: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Ocurrió un error al cargar los datos: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
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
            // En una implementación real, aquí cargaríamos las transacciones relacionadas con este repuesto desde JSON
            // Por ahora, solo limpiamos la colección y creamos algunas de ejemplo basadas en la ID real

            if (Repuesto == null) return;

            Transacciones.Clear();

            // Datos de ejemplo para el historial de transacciones, pero usando el ID real del repuesto
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

            // Transacción más reciente
            Transacciones.Add(new Transaccion
            {
                Id = 2,
                Fecha = DateTime.Now.AddDays(-2),
                Tipo = TipoTransaccion.Ajuste,
                Cantidad = Repuesto.Cantidad, // Usamos la cantidad actual
                RepuestoId = Repuesto.Id,
                Repuesto = Repuesto,
                ResponsableNombre = "Sistema",
                Observaciones = "Registro inicial en sistema"
            });

            Debug.WriteLine($"Cargadas {Transacciones.Count} transacciones para el repuesto {Repuesto.Id}");
            OnPropertyChanged(nameof(IsTransaccionesEmpty));
        }

        // Implementaciones de comandos
        private async void OnEntrada()
        {
            // Placeholder: En una versión completa, esto mostraría un formulario para registrar entrada
            string resultado = await Application.Current.MainPage.DisplayPromptAsync(
                "Entrada de Stock",
                "Ingrese la cantidad a añadir:",
                accept: "Confirmar",
                cancel: "Cancelar",
                placeholder: "Cantidad",
                maxLength: 5,
                keyboard: Keyboard.Numeric);

            // Validar el resultado manualmente
            int cantidadEntrada = 0;
            bool success = !string.IsNullOrEmpty(resultado) && int.TryParse(resultado, out cantidadEntrada) && cantidadEntrada > 0;

            if (success && cantidadEntrada > 0)
            {
                try
                {
                    // Actualizar repuesto
                    Repuesto.Cantidad += cantidadEntrada;

                    // Guardar cambios
                    var repuestos = await RepuestoJson.ObtenerRepuestos();
                    var index = repuestos.FindIndex(r => r.Id == Repuesto.Id);
                    if (index >= 0)
                    {
                        repuestos[index] = Repuesto;
                        await RepuestoJson.GuardarRepuestos(repuestos);

                        // Actualizar interfaz
                        ActualizarEstadoStock();

                        // Agregar transacción a la lista
                        var transaccion = new Transaccion
                        {
                            Id = Transacciones.Count + 1,
                            Fecha = DateTime.Now,
                            Tipo = TipoTransaccion.Entrada,
                            Cantidad = cantidadEntrada,
                            RepuestoId = Repuesto.Id,
                            Repuesto = Repuesto,
                            ResponsableNombre = "Usuario"
                        };

                        Transacciones.Insert(0, transaccion);
                        OnPropertyChanged(nameof(IsTransaccionesEmpty));

                        await Application.Current.MainPage.DisplayAlert("Éxito", $"Se registró entrada de {cantidadEntrada} unidades", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo registrar la entrada: {ex.Message}", "OK");
                }
            }
        }

        private async void OnSalida()
        {
            // Placeholder: En una versión completa, esto mostraría un formulario para registrar salida
            if (Repuesto.Cantidad <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No hay stock disponible para realizar salidas", "OK");
                return;
            }

            string resultado = await Application.Current.MainPage.DisplayPromptAsync(
                "Salida de Stock",
                $"Ingrese la cantidad a retirar (máx. {Repuesto.Cantidad}):",
                accept: "Confirmar",
                cancel: "Cancelar",
                placeholder: "Cantidad",
                maxLength: 5,
                keyboard: Keyboard.Numeric);

            // Validar el resultado manualmente
            int cantidadSalida = 0;
            bool success = !string.IsNullOrEmpty(resultado) &&
                           int.TryParse(resultado, out cantidadSalida) &&
                           cantidadSalida > 0 &&
                           cantidadSalida <= Repuesto.Cantidad;

            if (success && cantidadSalida > 0)
            {
                try
                {
                    // Actualizar repuesto
                    Repuesto.Cantidad -= cantidadSalida;

                    // Guardar cambios
                    var repuestos = await RepuestoJson.ObtenerRepuestos();
                    var index = repuestos.FindIndex(r => r.Id == Repuesto.Id);
                    if (index >= 0)
                    {
                        repuestos[index] = Repuesto;
                        await RepuestoJson.GuardarRepuestos(repuestos);

                        // Actualizar interfaz
                        ActualizarEstadoStock();

                        // Agregar transacción a la lista
                        var transaccion = new Transaccion
                        {
                            Id = Transacciones.Count + 1,
                            Fecha = DateTime.Now,
                            Tipo = TipoTransaccion.Salida,
                            Cantidad = cantidadSalida,
                            RepuestoId = Repuesto.Id,
                            Repuesto = Repuesto,
                            ResponsableNombre = "Usuario"
                        };

                        Transacciones.Insert(0, transaccion);
                        OnPropertyChanged(nameof(IsTransaccionesEmpty));

                        await Application.Current.MainPage.DisplayAlert("Éxito", $"Se registró salida de {cantidadSalida} unidades", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo registrar la salida: {ex.Message}", "OK");
                }
            }
        }

        private async void OnAjustar()
        {
            // Placeholder: En una versión completa, esto mostraría un formulario para ajustar stock
            string resultado = await Application.Current.MainPage.DisplayPromptAsync(
                "Ajuste de Stock",
                "Ingrese la cantidad correcta en stock:",
                accept: "Confirmar",
                cancel: "Cancelar",
                placeholder: "Cantidad",
                initialValue: Repuesto.Cantidad.ToString(),
                maxLength: 5,
                keyboard: Keyboard.Numeric);

            // Validar el resultado manualmente
            int nuevoStock = 0;
            bool success = !string.IsNullOrEmpty(resultado) &&
                           int.TryParse(resultado, out nuevoStock) &&
                           nuevoStock >= 0;

            if (success)
            {
                try
                {
                    int diferencia = nuevoStock - Repuesto.Cantidad;

                    // Actualizar repuesto
                    Repuesto.Cantidad = nuevoStock;

                    // Guardar cambios
                    var repuestos = await RepuestoJson.ObtenerRepuestos();
                    var index = repuestos.FindIndex(r => r.Id == Repuesto.Id);
                    if (index >= 0)
                    {
                        repuestos[index] = Repuesto;
                        await RepuestoJson.GuardarRepuestos(repuestos);

                        // Actualizar interfaz
                        ActualizarEstadoStock();

                        // Agregar transacción a la lista
                        var transaccion = new Transaccion
                        {
                            Id = Transacciones.Count + 1,
                            Fecha = DateTime.Now,
                            Tipo = TipoTransaccion.Ajuste,
                            Cantidad = Math.Abs(diferencia),
                            RepuestoId = Repuesto.Id,
                            Repuesto = Repuesto,
                            ResponsableNombre = "Usuario",
                            Observaciones = diferencia >= 0 ? "Ajuste positivo" : "Ajuste negativo"
                        };

                        Transacciones.Insert(0, transaccion);
                        OnPropertyChanged(nameof(IsTransaccionesEmpty));

                        await Application.Current.MainPage.DisplayAlert("Éxito", "Stock ajustado correctamente", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo realizar el ajuste: {ex.Message}", "OK");
                }
            }
        }

        private async void OnEditar()
        {
            await Application.Current.MainPage.DisplayAlert("Editar", "La función de edición estará disponible próximamente", "OK");
        }

        private async void OnEliminar()
        {
            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Confirmar eliminación",
                $"¿Está seguro que desea eliminar el repuesto '{Repuesto.Nombre}'?",
                "Sí, eliminar",
                "Cancelar");

            if (confirmar)
            {
                try
                {
                    bool resultado = await RepuestoJson.EliminarRepuesto(Repuesto.Id);
                    if (resultado)
                    {
                        await Application.Current.MainPage.DisplayAlert("Éxito", "El repuesto ha sido eliminado", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "No se pudo eliminar el repuesto", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Error al eliminar: {ex.Message}", "OK");
                }
            }
        }
    }
}
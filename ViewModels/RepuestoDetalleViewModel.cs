using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using AppInventariCor.Models;
using System.Linq;
using System.Diagnostics;
using AppInventariCor.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppInventariCor.Views;

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

        private async void CargarTransacciones()
        {
            try
            {
                if (Repuesto == null) return;

                Transacciones.Clear();

                // Cargar transacciones reales desde JSON
                var transacciones = await TransaccionJson.ObtenerTransaccionesPorRepuesto(Repuesto.Id);

                if (transacciones.Any())
                {
                    // Ordenar por fecha descendente (más recientes primero)
                    foreach (var transaccion in transacciones.OrderByDescending(t => t.Fecha))
                    {
                        Transacciones.Add(transaccion);
                    }
                    Debug.WriteLine($"Cargadas {Transacciones.Count} transacciones para el repuesto {Repuesto.Id}");
                }
                else
                {
                    Debug.WriteLine($"No hay transacciones registradas para el repuesto {Repuesto.Id}");
                }

                OnPropertyChanged(nameof(IsTransaccionesEmpty));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar transacciones: {ex.Message}");
                // Aquí podríamos mostrar un mensaje al usuario, pero quizás sea mejor
                // simplemente loguear el error para no interrumpir la experiencia
            }
        }

        // Implementaciones de comandos
        private async void OnEntrada()
        {
            try
            {
                // Solicitar cantidad a añadir
                string resultado = await Application.Current.MainPage.DisplayPromptAsync(
                    "Entrada de Stock",
                    "Ingrese la cantidad a añadir:",
                    accept: "Confirmar",
                    cancel: "Cancelar",
                    placeholder: "Cantidad",
                    maxLength: 5,
                    keyboard: Keyboard.Numeric);

                // Validar el resultado
                int cantidadEntrada = 0;
                bool success = !string.IsNullOrEmpty(resultado) && int.TryParse(resultado, out cantidadEntrada) && cantidadEntrada > 0;

                if (success && cantidadEntrada > 0)
                {
                    // Registrar cantidades para historial
                    int cantidadAnterior = Repuesto.Cantidad;
                    int cantidadPosterior = cantidadAnterior + cantidadEntrada;

                    // Actualizar repuesto
                    Repuesto.Cantidad = cantidadPosterior;

                    // Guardar cambios del repuesto
                    var repuestos = await RepuestoJson.ObtenerRepuestos();
                    var index = repuestos.FindIndex(r => r.Id == Repuesto.Id);
                    if (index >= 0)
                    {
                        repuestos[index] = Repuesto;
                        await RepuestoJson.GuardarRepuestos(repuestos);

                        // Actualizar interfaz
                        ActualizarEstadoStock();

                        // Crear transacción
                        var transaccion = new Transaccion
                        {
                            Fecha = DateTime.Now,
                            Tipo = TipoTransaccion.Entrada,
                            Cantidad = cantidadEntrada,
                            CantidadAnterior = cantidadAnterior,
                            CantidadPosterior = cantidadPosterior,
                            PrecioUnitario = Repuesto.Precio,
                            ValorTotal = Repuesto.Precio * cantidadEntrada,
                            RepuestoId = Repuesto.Id,
                            RepuestoCodigo = Repuesto.Codigo,
                            RepuestoNombre = Repuesto.Nombre,
                            ResponsableNombre = "Usuario",
                            Observaciones = "Entrada manual de stock"
                        };

                        // Guardar transacción
                        bool resultado2 = await TransaccionJson.AgregarTransaccion(transaccion);

                        if (resultado2)
                        {
                            // Añadir la transacción a la lista de visualización
                            Transacciones.Insert(0, transaccion);
                            OnPropertyChanged(nameof(IsTransaccionesEmpty));

                            await Application.Current.MainPage.DisplayAlert(
                                "Éxito",
                                $"Se registró entrada de {cantidadEntrada} unidades",
                                "OK");
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Advertencia",
                                "El inventario se actualizó pero hubo un problema al guardar el registro de la transacción",
                                "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnEntrada: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo registrar la entrada: {ex.Message}", "OK");
            }
        }

        private async void OnSalida()
        {
            try
            {
                // Verificar stock disponible
                if (Repuesto.Cantidad <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No hay stock disponible para realizar salidas", "OK");
                    return;
                }

                // Solicitar cantidad a retirar
                string resultado = await Application.Current.MainPage.DisplayPromptAsync(
                    "Salida de Stock",
                    $"Ingrese la cantidad a retirar (máx. {Repuesto.Cantidad}):",
                    accept: "Confirmar",
                    cancel: "Cancelar",
                    placeholder: "Cantidad",
                    maxLength: 5,
                    keyboard: Keyboard.Numeric);

                // Validar el resultado
                int cantidadSalida = 0;
                bool success = !string.IsNullOrEmpty(resultado) &&
                               int.TryParse(resultado, out cantidadSalida) &&
                               cantidadSalida > 0 &&
                               cantidadSalida <= Repuesto.Cantidad;

                if (success && cantidadSalida > 0)
                {
                    // Registrar cantidades para historial
                    int cantidadAnterior = Repuesto.Cantidad;
                    int cantidadPosterior = cantidadAnterior - cantidadSalida;

                    // Actualizar repuesto
                    Repuesto.Cantidad = cantidadPosterior;

                    // Guardar cambios del repuesto
                    var repuestos = await RepuestoJson.ObtenerRepuestos();
                    var index = repuestos.FindIndex(r => r.Id == Repuesto.Id);
                    if (index >= 0)
                    {
                        repuestos[index] = Repuesto;
                        await RepuestoJson.GuardarRepuestos(repuestos);

                        // Actualizar interfaz
                        ActualizarEstadoStock();

                        // Crear transacción
                        var transaccion = new Transaccion
                        {
                            Fecha = DateTime.Now,
                            Tipo = TipoTransaccion.Salida,
                            Cantidad = cantidadSalida,
                            CantidadAnterior = cantidadAnterior,
                            CantidadPosterior = cantidadPosterior,
                            PrecioUnitario = Repuesto.Precio,
                            ValorTotal = Repuesto.Precio * cantidadSalida,
                            RepuestoId = Repuesto.Id,
                            RepuestoCodigo = Repuesto.Codigo,
                            RepuestoNombre = Repuesto.Nombre,
                            ResponsableNombre = "Usuario",
                            Observaciones = "Salida manual de stock"
                        };

                        // Guardar transacción
                        bool resultado2 = await TransaccionJson.AgregarTransaccion(transaccion);

                        if (resultado2)
                        {
                            // Añadir la transacción a la lista de visualización
                            Transacciones.Insert(0, transaccion);
                            OnPropertyChanged(nameof(IsTransaccionesEmpty));

                            await Application.Current.MainPage.DisplayAlert(
                                "Éxito",
                                $"Se registró salida de {cantidadSalida} unidades",
                                "OK");
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Advertencia",
                                "El inventario se actualizó pero hubo un problema al guardar el registro de la transacción",
                                "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnSalida: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo registrar la salida: {ex.Message}", "OK");
            }
        }

        private async void OnAjustar()
        {
            try
            {
                // Solicitar nuevo valor de stock
                string resultado = await Application.Current.MainPage.DisplayPromptAsync(
                    "Ajuste de Stock",
                    "Ingrese la cantidad correcta en stock:",
                    accept: "Confirmar",
                    cancel: "Cancelar",
                    placeholder: "Cantidad",
                    initialValue: Repuesto.Cantidad.ToString(),
                    maxLength: 5,
                    keyboard: Keyboard.Numeric);

                // Validar el resultado
                int nuevoStock = 0;
                bool success = !string.IsNullOrEmpty(resultado) &&
                               int.TryParse(resultado, out nuevoStock) &&
                               nuevoStock >= 0;

                if (success)
                {
                    // Registrar cantidades para historial
                    int cantidadAnterior = Repuesto.Cantidad;
                    int cantidadPosterior = nuevoStock;
                    int diferencia = nuevoStock - cantidadAnterior;

                    // Si no hay cambio, no hacer nada
                    if (diferencia == 0)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Información",
                            "No se realizó ningún cambio en el stock",
                            "OK");
                        return;
                    }

                    // Actualizar repuesto
                    Repuesto.Cantidad = nuevoStock;

                    // Guardar cambios del repuesto
                    var repuestos = await RepuestoJson.ObtenerRepuestos();
                    var index = repuestos.FindIndex(r => r.Id == Repuesto.Id);
                    if (index >= 0)
                    {
                        repuestos[index] = Repuesto;
                        await RepuestoJson.GuardarRepuestos(repuestos);

                        // Actualizar interfaz
                        ActualizarEstadoStock();

                        // Crear transacción
                        var transaccion = new Transaccion
                        {
                            Fecha = DateTime.Now,
                            Tipo = TipoTransaccion.Ajuste,
                            Cantidad = Math.Abs(diferencia),
                            CantidadAnterior = cantidadAnterior,
                            CantidadPosterior = cantidadPosterior,
                            PrecioUnitario = Repuesto.Precio,
                            ValorTotal = Repuesto.Precio * Math.Abs(diferencia),
                            RepuestoId = Repuesto.Id,
                            RepuestoCodigo = Repuesto.Codigo,
                            RepuestoNombre = Repuesto.Nombre,
                            ResponsableNombre = "Usuario",
                            Observaciones = diferencia > 0
                                ? $"Ajuste positivo (+{diferencia})"
                                : $"Ajuste negativo ({diferencia})"
                        };

                        // Guardar transacción
                        bool resultado2 = await TransaccionJson.AgregarTransaccion(transaccion);

                        if (resultado2)
                        {
                            // Añadir la transacción a la lista de visualización
                            Transacciones.Insert(0, transaccion);
                            OnPropertyChanged(nameof(IsTransaccionesEmpty));

                            await Application.Current.MainPage.DisplayAlert(
                                "Éxito",
                                $"Stock ajustado de {cantidadAnterior} a {cantidadPosterior} unidades",
                                "OK");
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Advertencia",
                                "El inventario se actualizó pero hubo un problema al guardar el registro de la transacción",
                                "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnAjustar: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo realizar el ajuste: {ex.Message}", "OK");
            }
        }

        private async void OnEditar()
        {
            try
            {
                if (Repuesto == null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se puede editar: Información del repuesto no disponible",
                        "OK");
                    return;
                }

                Debug.WriteLine($"[RepuestoDetalleViewModel] Iniciando edición del repuesto: {Repuesto.Id} - {Repuesto.Codigo}");

                // Es crucial establecer IsEditMode primero para que el ViewModel sepa que debe cargar el repuesto
                var navigationParameter = new Dictionary<string, object>
                {
                    { "IsEditMode", true },
                    { "RepuestoId", Repuesto.Id }
                };

                // Navegar a la página de agregar repuesto pero en modo edición
                await Shell.Current.GoToAsync($"{nameof(AgregarRepuestoPage)}", navigationParameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RepuestoDetalleViewModel] Error en OnEditar: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo iniciar la edición: {ex.Message}",
                    "OK");
            }
        }
    }
}
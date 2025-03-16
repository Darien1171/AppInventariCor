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
    [QueryProperty(nameof(VehiculoId), "VehiculoId")]
    public class VehiculoDetalleViewModel : BaseViewModel
    {
        private int _vehiculoId;
        private Vehiculo _vehiculo;
        private ObservableCollection<Transaccion> _historialRepuestos;
        private bool _isLoading = true;

        public int VehiculoId
        {
            get => _vehiculoId;
            set
            {
                _vehiculoId = value;
                LoadVehiculoAsync(value);
            }
        }

        public Vehiculo Vehiculo
        {
            get => _vehiculo;
            set
            {
                if (SetProperty(ref _vehiculo, value))
                {
                    CargarHistorialRepuestos();
                    OnPropertyChanged(nameof(IsHistorialEmpty));
                    Title = _vehiculo?.NumeroPlaca ?? "Detalle de Vehículo";
                }
            }
        }

        public ObservableCollection<Transaccion> HistorialRepuestos
        {
            get => _historialRepuestos;
            set => SetProperty(ref _historialRepuestos, value);
        }

        public bool IsHistorialEmpty => HistorialRepuestos == null || HistorialRepuestos.Count == 0;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Comandos
        public ICommand RegistrarRepuestoCommand { get; }
        public ICommand EditarCommand { get; }

        // Constructor
        public VehiculoDetalleViewModel()
        {
            Debug.WriteLine("VehiculoDetalleViewModel: Constructor llamado");

            HistorialRepuestos = new ObservableCollection<Transaccion>();

            // Inicializar comandos
            RegistrarRepuestoCommand = new Command(OnRegistrarRepuesto);
            EditarCommand = new Command(OnEditar);

            Title = "Cargando vehículo...";
        }

        private async void LoadVehiculoAsync(int vehiculoId)
        {
            try
            {
                IsLoading = true;

                // Cargar vehículo desde JSON
                var vehiculo = await VehiculoJson.ObtenerVehiculoPorId(vehiculoId);

                if (vehiculo != null)
                {
                    Vehiculo = vehiculo;
                    Debug.WriteLine($"Vehículo cargado: {vehiculo.NumeroPlaca}, ID: {vehiculo.Id}");
                }
                else
                {
                    Debug.WriteLine($"No se encontró vehículo con ID: {vehiculoId}");
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se encontró el vehículo solicitado.",
                        "OK");

                    // Volver a la página anterior si no se encuentra el vehículo
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar vehículo: {ex.Message}");
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

        private async void CargarHistorialRepuestos()
        {
            try
            {
                if (Vehiculo == null) return;

                HistorialRepuestos.Clear();

                // Cargar transacciones desde JSON relacionadas con este vehículo
                var transacciones = await TransaccionJson.ObtenerTransacciones();
                var historial = transacciones
                    .Where(t => t.VehiculoId == Vehiculo.Id && t.Tipo == TipoTransaccion.Salida)
                    .OrderByDescending(t => t.Fecha)
                    .ToList();

                if (historial.Any())
                {
                    foreach (var transaccion in historial)
                    {
                        HistorialRepuestos.Add(transaccion);
                    }
                    Debug.WriteLine($"Cargadas {HistorialRepuestos.Count} transacciones para el vehículo {Vehiculo.Id}");
                }
                else
                {
                    Debug.WriteLine($"No hay repuestos registrados para el vehículo {Vehiculo.Id}");
                }

                OnPropertyChanged(nameof(IsHistorialEmpty));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar historial de repuestos: {ex.Message}");
            }
        }

        // Implementaciones de comandos
        private async void OnRegistrarRepuesto()
        {
            try
            {
                if (Vehiculo == null) return;

                // Aquí navegaríamos a una página para registrar un nuevo repuesto para este vehículo
                // Por ahora mostraremos un mensaje
                await Application.Current.MainPage.DisplayAlert(
                    "Información",
                    "Esta funcionalidad permitirá registrar un repuesto para este vehículo.",
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnRegistrarRepuesto: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo iniciar el registro: {ex.Message}",
                    "OK");
            }
        }

        private async void OnEditar()
        {
            try
            {
                if (Vehiculo == null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se puede editar: Información del vehículo no disponible",
                        "OK");
                    return;
                }

                Debug.WriteLine($"[VehiculoDetalleViewModel] Iniciando edición del vehículo: {Vehiculo.Id} - {Vehiculo.NumeroPlaca}");

                // Este código sería para navegar a la página de edición de vehículo
                var navigationParameter = new Dictionary<string, object>
                {
                    { "IsEditMode", true },
                    { "VehiculoId", Vehiculo.Id }
                };

                // Por ahora mostramos un mensaje ya que la página de edición no está implementada
                await Application.Current.MainPage.DisplayAlert(
                    "Información",
                    "Esta funcionalidad permitirá editar los datos del vehículo.",
                    "OK");

                // Cuando tengamos la página implementada:
                // await Shell.Current.GoToAsync($"{nameof(AgregarVehiculoPage)}", navigationParameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VehiculoDetalleViewModel] Error en OnEditar: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo iniciar la edición: {ex.Message}",
                    "OK");
            }
        }
    }
}
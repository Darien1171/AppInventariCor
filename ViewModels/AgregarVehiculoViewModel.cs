using AppInventariCor.Models;
using AppInventariCor.Services;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AppInventariCor.ViewModels
{
    [QueryProperty(nameof(VehiculoId), "VehiculoId")]
    [QueryProperty(nameof(IsEditMode), "IsEditMode")]
    public class AgregarVehiculoViewModel : BaseViewModel
    {
        private int _vehiculoId;
        private bool _isEditMode;
        private Vehiculo _vehiculo;

        // Propiedades de UI
        private string _pageTitle = "Nuevo Vehículo";
        private string _actionButtonText = "Guardar Vehículo";

        // Propiedades para binding a los campos
        private string _numeroPlaca;
        private string _numeroInterno;
        private string _marca;
        private string _modelo;
        private string _numeroSerie;
        private string _numeroMotor;
        private int? _anio;
        private string _color;
        private string _propietario;

        // Propiedades de navegación
        public int VehiculoId
        {
            get => _vehiculoId;
            set
            {
                if (SetProperty(ref _vehiculoId, value))
                {
                    CheckAndLoadVehiculo();
                }
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetProperty(ref _isEditMode, value))
                {
                    // Actualizar título y texto del botón
                    PageTitle = value ? "Editar Vehículo" : "Nuevo Vehículo";
                    ActionButtonText = value ? "Actualizar Vehículo" : "Guardar Vehículo";

                    // Intentar cargar el vehículo si ya tenemos el ID
                    CheckAndLoadVehiculo();
                }
            }
        }

        // Método auxiliar para verificar si podemos cargar el vehículo
        private void CheckAndLoadVehiculo()
        {
            try
            {
                // Cargar el vehículo si tenemos el ID y estamos en modo edición
                if (_isEditMode && _vehiculoId > 0)
                {
                    LoadVehiculoAsync(_vehiculoId);
                    Debug.WriteLine($"[AgregarVehiculoViewModel] Cargando vehículo ID: {_vehiculoId} en modo edición");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en CheckAndLoadVehiculo: {ex.Message}");
                // No lanzar excepciones para evitar crashes
            }
        }

        // Propiedades para UI
        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public string ActionButtonText
        {
            get => _actionButtonText;
            set => SetProperty(ref _actionButtonText, value);
        }

        // Propiedades para campos del formulario
        public string NumeroPlaca
        {
            get => _numeroPlaca;
            set => SetProperty(ref _numeroPlaca, value);
        }

        public string NumeroInterno
        {
            get => _numeroInterno;
            set => SetProperty(ref _numeroInterno, value);
        }

        public string Marca
        {
            get => _marca;
            set => SetProperty(ref _marca, value);
        }

        public string Modelo
        {
            get => _modelo;
            set => SetProperty(ref _modelo, value);
        }

        public string NumeroSerie
        {
            get => _numeroSerie;
            set => SetProperty(ref _numeroSerie, value);
        }

        public string NumeroMotor
        {
            get => _numeroMotor;
            set => SetProperty(ref _numeroMotor, value);
        }

        public int? Anio
        {
            get => _anio;
            set => SetProperty(ref _anio, value);
        }

        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public string Propietario
        {
            get => _propietario;
            set => SetProperty(ref _propietario, value);
        }

        // Constructor
        public AgregarVehiculoViewModel()
        {
            try
            {
                Title = "Agregar/Editar Vehículo";
                Debug.WriteLine("[AgregarVehiculoViewModel] Constructor ejecutado");

                // Inicializaciones adicionales si son necesarias
                _numeroPlaca = string.Empty;
                _numeroInterno = string.Empty;
                _marca = string.Empty;
                _modelo = string.Empty;
                _numeroSerie = string.Empty;
                _numeroMotor = string.Empty;
                _color = string.Empty;
                _propietario = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AgregarVehiculoViewModel] Error en constructor: {ex.Message}");
                // No lanzar excepciones en el constructor para evitar crashes
            }
        }

        // Método para cargar un vehículo existente
        private async void LoadVehiculoAsync(int vehiculoId)
        {
            try
            {
                if (vehiculoId <= 0)
                {
                    Debug.WriteLine("[AgregarVehiculoViewModel] ID de vehículo inválido");
                    return;
                }

                Debug.WriteLine($"[AgregarVehiculoViewModel] Intentando cargar vehículo ID: {vehiculoId}");
                IsBusy = true;

                // Cargar el vehículo desde el servicio
                Vehiculo vehiculo = null;
                try
                {
                    vehiculo = await VehiculoJson.ObtenerVehiculoPorId(vehiculoId);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[AgregarVehiculoViewModel] Error al obtener vehículo: {ex.Message}");
                    vehiculo = null;
                }

                if (vehiculo != null)
                {
                    _vehiculo = vehiculo;

                    // Llenar las propiedades del formulario
                    NumeroPlaca = vehiculo.NumeroPlaca ?? string.Empty;
                    NumeroInterno = vehiculo.NumeroInterno ?? string.Empty;
                    Marca = vehiculo.Marca ?? string.Empty;
                    Modelo = vehiculo.Modelo ?? string.Empty;
                    NumeroSerie = vehiculo.NumeroSerie ?? string.Empty;
                    NumeroMotor = vehiculo.NumeroMotor ?? string.Empty;
                    Anio = vehiculo.Anio;
                    Color = vehiculo.Color ?? string.Empty;
                    Propietario = vehiculo.Propietario ?? string.Empty;

                    Debug.WriteLine($"[AgregarVehiculoViewModel] Vehículo cargado completamente: {vehiculo.NumeroPlaca}, ID: {vehiculo.Id}");

                    // Forzar actualización de todas las propiedades UI
                    OnPropertyChanged(string.Empty);
                }
                else
                {
                    Debug.WriteLine($"[AgregarVehiculoViewModel] No se encontró vehículo con ID: {vehiculoId}");
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se encontró el vehículo solicitado.",
                        "OK");

                    // Volver a la página anterior
                    try
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                    catch (Exception navEx)
                    {
                        Debug.WriteLine($"[AgregarVehiculoViewModel] Error de navegación: {navEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AgregarVehiculoViewModel] Error al cargar vehículo: {ex.Message}");
                // Intentar mostrar un mensaje al usuario
                try
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Ocurrió un error al cargar los datos: {ex.Message}",
                        "OK");
                }
                catch { /* Ignorar errores del DisplayAlert */ }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Método para guardar o actualizar un vehículo
        public async Task<bool> SaveVehiculoAsync()
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(NumeroPlaca) ||
                    string.IsNullOrWhiteSpace(NumeroInterno) ||
                    string.IsNullOrWhiteSpace(Marca))
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Validación",
                        "Los campos Número de Placa, Número Interno y Marca son obligatorios.",
                        "OK");
                    return false;
                }

                // Crear o actualizar el objeto vehículo
                if (IsEditMode && _vehiculo != null)
                {
                    // Actualizar el vehículo existente
                    _vehiculo.NumeroPlaca = NumeroPlaca;
                    _vehiculo.NumeroInterno = NumeroInterno;
                    _vehiculo.Marca = Marca;
                    _vehiculo.Modelo = Modelo ?? string.Empty;
                    _vehiculo.NumeroSerie = NumeroSerie;
                    _vehiculo.NumeroMotor = NumeroMotor;
                    _vehiculo.Anio = Anio;
                    _vehiculo.Color = Color;
                    _vehiculo.Propietario = Propietario;

                    // Guardar los cambios
                    bool resultado = false;
                    try
                    {
                        resultado = await VehiculoJson.ActualizarVehiculo(_vehiculo);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[AgregarVehiculoViewModel] Error actualizando vehículo: {ex.Message}");
                        resultado = false;
                    }

                    if (resultado)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Éxito",
                            "Vehículo actualizado correctamente.",
                            "OK");
                        return true;
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Error",
                            "No se pudo actualizar el vehículo.",
                            "OK");
                        return false;
                    }
                }
                else
                {
                    // Crear un nuevo vehículo
                    var nuevoVehiculo = new Vehiculo
                    {
                        NumeroPlaca = NumeroPlaca,
                        NumeroInterno = NumeroInterno,
                        Marca = Marca,
                        Modelo = Modelo ?? string.Empty,
                        NumeroSerie = NumeroSerie,
                        NumeroMotor = NumeroMotor,
                        Anio = Anio,
                        Color = Color,
                        Propietario = Propietario,
                        // Inicializar colecciones para evitar nulos
                        ImagenesUrls = new List<string>(),
                        HistorialTransacciones = new List<Transaccion>()
                    };

                    // Guardar el nuevo vehículo
                    bool resultado = false;
                    try
                    {
                        resultado = await VehiculoJson.AgregarVehiculo(nuevoVehiculo);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[AgregarVehiculoViewModel] Error agregando vehículo: {ex.Message}");
                        resultado = false;
                    }

                    if (resultado)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Éxito",
                            "Vehículo agregado correctamente.",
                            "OK");
                        return true;
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Error",
                            "No se pudo agregar el vehículo. La placa podría estar duplicada.",
                            "OK");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AgregarVehiculoViewModel] Error al guardar vehículo: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Ocurrió un error al guardar los datos: {ex.Message}",
                    "OK");
                return false;
            }
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using AppInventariCor.Models;
using AppInventariCor.Services;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AppInventariCor.ViewModels
{
    public class NuevaTransaccionViewModel : BaseViewModel
    {
        // Variables para seguimiento del wizard
        private int _currentStep = 1;
        private readonly int _totalSteps = 5;

        // Propiedades para selección de vehículo (Paso 1)
        private string _vehiculoSearchQuery;
        private ObservableCollection<Vehiculo> _vehiculosFiltrados;
        private Vehiculo _selectedVehiculo;

        // Propiedades para selección de repuesto (Paso 2)
        private string _repuestoSearchQuery;
        private ObservableCollection<Repuesto> _repuestosFiltrados;
        private Repuesto _selectedRepuesto;

        // Propiedades para datos específicos (Paso 3)
        private string _cantidadTransaccion;
        private decimal _precioUnitario;
        private string _observaciones;

        // Propiedades para evidencia fotográfica (Paso 4)
        private ObservableCollection<ImageSource> _evidenceImages;
        private bool _hasSignature;

        // Propiedades adicionales
        private bool _showStockWarning;

        #region Propiedades de Binding

        // Propiedades del wizard
        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                if (SetProperty(ref _currentStep, value))
                {
                    OnPropertyChanged(nameof(CurrentProgress));
                    OnPropertyChanged(nameof(CurrentStepTitle));
                    OnPropertyChanged(nameof(IsStep1Visible));
                    OnPropertyChanged(nameof(IsStep2Visible));
                    OnPropertyChanged(nameof(IsStep3Visible));
                    OnPropertyChanged(nameof(IsStep4Visible));
                    OnPropertyChanged(nameof(IsStep5Visible));
                    OnPropertyChanged(nameof(CanGoBack));
                    OnPropertyChanged(nameof(CanGoForward));
                }
            }
        }

        public double CurrentProgress => (double)CurrentStep / _totalSteps;

        public string CurrentStepTitle
        {
            get
            {
                return CurrentStep switch
                {
                    1 => "Selección de Vehículo",
                    2 => "Selección de Repuesto",
                    3 => "Detalles de Venta",
                    4 => "Evidencia Fotográfica",
                    5 => "Confirmación",
                    _ => "Venta a Vehículo"
                };
            }
        }

        // Visibilidad de pasos
        public bool IsStep1Visible => CurrentStep == 1;
        public bool IsStep2Visible => CurrentStep == 2;
        public bool IsStep3Visible => CurrentStep == 3;
        public bool IsStep4Visible => CurrentStep == 4;
        public bool IsStep5Visible => CurrentStep == 5;

        // Navegación
        public bool CanGoBack => CurrentStep > 1;
        public bool CanGoForward => CurrentStep < _totalSteps && ValidateCurrentStep();

        // Propiedades para selección de vehículo (Paso 1)
        public string VehiculoSearchQuery
        {
            get => _vehiculoSearchQuery;
            set
            {
                if (SetProperty(ref _vehiculoSearchQuery, value) && !string.IsNullOrWhiteSpace(value))
                {
                    SearchVehiculos();
                }
            }
        }

        public ObservableCollection<Vehiculo> VehiculosFiltrados
        {
            get => _vehiculosFiltrados;
            set => SetProperty(ref _vehiculosFiltrados, value);
        }

        public Vehiculo SelectedVehiculo
        {
            get => _selectedVehiculo;
            set => SetProperty(ref _selectedVehiculo, value);
        }

        // Propiedades para selección de repuesto (Paso 2)
        public string RepuestoSearchQuery
        {
            get => _repuestoSearchQuery;
            set
            {
                if (SetProperty(ref _repuestoSearchQuery, value) && !string.IsNullOrWhiteSpace(value))
                {
                    SearchRepuestos();
                }
            }
        }

        public ObservableCollection<Repuesto> RepuestosFiltrados
        {
            get => _repuestosFiltrados;
            set => SetProperty(ref _repuestosFiltrados, value);
        }

        public Repuesto SelectedRepuesto
        {
            get => _selectedRepuesto;
            set
            {
                if (SetProperty(ref _selectedRepuesto, value) && value != null)
                {
                    // Cuando se selecciona un repuesto, inicializar precio con valor del repuesto
                    PrecioUnitario = value.Precio;
                }
            }
        }

        // Propiedades para datos específicos (Paso 3)
        public string CantidadTransaccion
        {
            get => _cantidadTransaccion;
            set
            {
                if (SetProperty(ref _cantidadTransaccion, value))
                {
                    OnPropertyChanged(nameof(ValorTotal));

                    // Verificar si debemos mostrar advertencia de stock
                    CheckStockWarning();
                }
            }
        }

        public decimal PrecioUnitario
        {
            get => _precioUnitario;
            set
            {
                if (SetProperty(ref _precioUnitario, value))
                {
                    OnPropertyChanged(nameof(ValorTotal));
                }
            }
        }

        public string Observaciones
        {
            get => _observaciones;
            set
            {
                if (SetProperty(ref _observaciones, value))
                {
                    OnPropertyChanged(nameof(HasObservaciones));
                }
            }
        }

        // Propiedades para evidencia fotográfica (Paso 4)
        public ObservableCollection<ImageSource> EvidenceImages
        {
            get => _evidenceImages;
            set
            {
                if (SetProperty(ref _evidenceImages, value))
                {
                    OnPropertyChanged(nameof(HasEvidenceImages));
                }
            }
        }

        public bool HasSignature
        {
            get => _hasSignature;
            set => SetProperty(ref _hasSignature, value);
        }

        // Propiedades adicionales
        public decimal ValorTotal
        {
            get
            {
                if (string.IsNullOrEmpty(_cantidadTransaccion) || !int.TryParse(_cantidadTransaccion, out int cantidad))
                {
                    return 0;
                }
                return _precioUnitario * cantidad;
            }
        }

        public bool HasObservaciones => !string.IsNullOrWhiteSpace(_observaciones);
        public bool HasEvidenceImages => _evidenceImages != null && _evidenceImages.Count > 0;

        public bool ShowStockWarning
        {
            get => _showStockWarning;
            set => SetProperty(ref _showStockWarning, value);
        }

        #endregion

        #region Comandos
        public ICommand PreviousStepCommand { get; }
        public ICommand NextStepCommand { get; }
        public ICommand VehiculoSearchCommand { get; }
        public ICommand RepuestoSearchCommand { get; }
        public ICommand ScanCommand { get; }
        public ICommand TakePictureCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand ClearSignatureCommand { get; }
        public ICommand ConfirmTransactionCommand { get; }
        #endregion

        // Constructor
        public NuevaTransaccionViewModel()
        {
            Title = "Nueva Venta";

            // Inicializar colecciones
            _vehiculosFiltrados = new ObservableCollection<Vehiculo>();
            _repuestosFiltrados = new ObservableCollection<Repuesto>();
            _evidenceImages = new ObservableCollection<ImageSource>();

            // Inicializar comandos
            PreviousStepCommand = new Command(PreviousStep);
            NextStepCommand = new Command(NextStep);
            VehiculoSearchCommand = new Command(SearchVehiculos);
            RepuestoSearchCommand = new Command(SearchRepuestos);
            ScanCommand = new Command(ScanRepuesto);
            TakePictureCommand = new Command(TakePicture);
            PickImageCommand = new Command(PickImage);
            RemoveImageCommand = new Command<ImageSource>(RemoveImage);
            ClearSignatureCommand = new Command(ClearSignature);
            ConfirmTransactionCommand = new Command(ConfirmTransaction);

            // Cargar datos iniciales
            LoadInitialData();
        }

        #region Implementación de métodos

        private async void LoadInitialData()
        {
            try
            {
                IsBusy = true;

                // Cargar vehículos para el primer paso
                var vehiculos = await VehiculoJson.ObtenerVehiculos();
                if (vehiculos != null && vehiculos.Any())
                {
                    foreach (var vehiculo in vehiculos.Take(10))
                    {
                        VehiculosFiltrados.Add(vehiculo);
                    }
                }

                // Precargar algunos repuestos para el segundo paso
                var repuestos = await RepuestoJson.ObtenerRepuestos();
                if (repuestos != null && repuestos.Any())
                {
                    foreach (var repuesto in repuestos.Where(r => r.Disponible).Take(10))
                    {
                        RepuestosFiltrados.Add(repuesto);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NuevaTransaccionViewModel] Error en LoadInitialData: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Ocurrió un error al cargar los datos iniciales: " + ex.Message,
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void PreviousStep()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
            }
        }

        private void NextStep()
        {
            if (ValidateCurrentStep() && CurrentStep < _totalSteps)
            {
                CurrentStep++;
            }
        }

        private bool ValidateCurrentStep()
        {
            // Validar según el paso actual
            switch (CurrentStep)
            {
                case 1: // Selección de vehículo
                    return SelectedVehiculo != null;

                case 2: // Selección de repuesto
                    return SelectedRepuesto != null && SelectedRepuesto.Disponible;

                case 3: // Detalles de venta
                    if (string.IsNullOrWhiteSpace(CantidadTransaccion))
                        return false;

                    if (!int.TryParse(CantidadTransaccion, out int cantidad) || cantidad <= 0)
                        return false;

                    // Verificar que no exceda el stock disponible
                    if (SelectedRepuesto != null)
                    {
                        return cantidad <= SelectedRepuesto.Cantidad;
                    }

                    return false;

                case 4: // Evidencia fotográfica
                    // No hay validación específica, la evidencia es opcional
                    return true;

                default:
                    return true;
            }
        }

        private async void SearchVehiculos()
        {
            try
            {
                IsBusy = true;

                var vehiculos = await VehiculoJson.ObtenerVehiculos();
                var resultado = vehiculos;

                // Aplicar filtro de búsqueda si hay texto
                if (!string.IsNullOrWhiteSpace(VehiculoSearchQuery))
                {
                    string query = VehiculoSearchQuery.ToLower();
                    resultado = vehiculos.Where(v =>
                        (v.NumeroPlaca?.ToLower().Contains(query) ?? false) ||
                        (v.NumeroInterno?.ToLower().Contains(query) ?? false) ||
                        (v.Marca?.ToLower().Contains(query) ?? false) ||
                        (v.Propietario?.ToLower().Contains(query) ?? false)).ToList();
                }

                // Actualizar la colección
                VehiculosFiltrados.Clear();
                foreach (var vehiculo in resultado)
                {
                    VehiculosFiltrados.Add(vehiculo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NuevaTransaccionViewModel] Error en SearchVehiculos: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void SearchRepuestos()
        {
            try
            {
                IsBusy = true;

                var repuestos = await RepuestoJson.ObtenerRepuestos();
                var resultado = repuestos;

                // Aplicar filtro de búsqueda si hay texto
                if (!string.IsNullOrWhiteSpace(RepuestoSearchQuery))
                {
                    string query = RepuestoSearchQuery.ToLower();
                    resultado = repuestos.Where(r =>
                        (r.Nombre?.ToLower().Contains(query) ?? false) ||
                        (r.Codigo?.ToLower().Contains(query) ?? false) ||
                        (r.Categoria?.ToLower().Contains(query) ?? false)).ToList();
                }

                // Actualizar la colección
                RepuestosFiltrados.Clear();
                foreach (var repuesto in resultado)
                {
                    RepuestosFiltrados.Add(repuesto);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NuevaTransaccionViewModel] Error en SearchRepuestos: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CheckStockWarning()
        {
            if (SelectedRepuesto == null || string.IsNullOrWhiteSpace(CantidadTransaccion))
            {
                ShowStockWarning = false;
                return;
            }

            if (int.TryParse(CantidadTransaccion, out int cantidad))
            {
                int stockRestante = SelectedRepuesto.Cantidad - cantidad;
                ShowStockWarning = stockRestante < SelectedRepuesto.StockMinimo && stockRestante >= 0;
            }
            else
            {
                ShowStockWarning = false;
            }
        }

        private async void ScanRepuesto()
        {
            // Aquí implementaríamos la funcionalidad de escaneo de código de barras o QR
            await Application.Current.MainPage.DisplayAlert(
                "Escaneo",
                "La funcionalidad de escaneo sería implementada aquí, integrándose con el API de cámara del dispositivo.",
                "OK");

            // Simulamos encontrar un repuesto (en una implementación real, buscaríamos por el código escaneado)
            if (RepuestosFiltrados.Any())
            {
                SelectedRepuesto = RepuestosFiltrados.First();
            }
        }

        private async void TakePicture()
        {
            // Aquí implementaríamos la funcionalidad para tomar una foto
            await Application.Current.MainPage.DisplayAlert(
                "Tomar Foto",
                "La funcionalidad de captura de imagen sería implementada aquí, integrándose con el API de cámara del dispositivo.",
                "OK");

            // En una implementación real, se tomaría la foto y se guardaría en EvidenceImages
            // Por ahora, para simular, añadimos una "imagen vacía"
            EvidenceImages.Add(null);
            OnPropertyChanged(nameof(HasEvidenceImages));
        }

        private async void PickImage()
        {
            // Aquí implementaríamos la funcionalidad para seleccionar una imagen de la galería
            await Application.Current.MainPage.DisplayAlert(
                "Seleccionar Imagen",
                "La funcionalidad de selección de imagen sería implementada aquí, integrándose con el API de la galería del dispositivo.",
                "OK");

            // En una implementación real, se seleccionaría la imagen y se guardaría en EvidenceImages
            // Por ahora, para simular, añadimos una "imagen vacía"
            EvidenceImages.Add(null);
            OnPropertyChanged(nameof(HasEvidenceImages));
        }

        private void RemoveImage(ImageSource imageSource)
        {
            // Remover una imagen de la colección de evidencias
            if (EvidenceImages.Contains(imageSource))
            {
                EvidenceImages.Remove(imageSource);
                OnPropertyChanged(nameof(HasEvidenceImages));
            }
        }

        private void ClearSignature()
        {
            // Borrar la firma
            HasSignature = false;
        }

        private async void ConfirmTransaction()
        {
            try
            {
                IsBusy = true;

                // Validar que tengamos los datos mínimos necesarios
                if (SelectedVehiculo == null || SelectedRepuesto == null || string.IsNullOrWhiteSpace(CantidadTransaccion))
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "Faltan datos requeridos para completar la transacción.",
                        "OK");
                    return;
                }

                // Convertir la cantidad
                if (!int.TryParse(CantidadTransaccion, out int cantidad) || cantidad <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "La cantidad debe ser un número positivo.",
                        "OK");
                    return;
                }

                // Verificar stock disponible
                if (cantidad > SelectedRepuesto.Cantidad)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No hay suficiente stock disponible para esta transacción.",
                        "OK");
                    return;
                }

                // Calcular cantidades para historial
                int cantidadAnterior = SelectedRepuesto.Cantidad;
                int cantidadPosterior = cantidadAnterior - cantidad;

                // Crear objeto de transacción
                var transaccion = new Transaccion
                {
                    Fecha = DateTime.Now,
                    Tipo = TipoTransaccion.Salida,
                    Cantidad = cantidad,
                    CantidadAnterior = cantidadAnterior,
                    CantidadPosterior = cantidadPosterior,
                    PrecioUnitario = PrecioUnitario,
                    ValorTotal = ValorTotal,
                    Observaciones = Observaciones,
                    RepuestoId = SelectedRepuesto.Id,
                    RepuestoCodigo = SelectedRepuesto.Codigo,
                    RepuestoNombre = SelectedRepuesto.Nombre,
                    VehiculoId = SelectedVehiculo.Id,
                    VehiculoPlaca = SelectedVehiculo.NumeroPlaca,
                    ResponsableNombre = "Usuario Actual", // En una implementación real, obtendríamos el usuario logueado
                    FechaCreacion = DateTime.Now
                };

                // Actualizar el inventario (reducir stock)
                SelectedRepuesto.Cantidad = cantidadPosterior;
                await RepuestoJson.ActualizarRepuesto(SelectedRepuesto);

                // Guardar la transacción
                bool resultado = await TransaccionJson.AgregarTransaccion(transaccion);

                if (resultado)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Éxito",
                        "Venta registrada correctamente",
                        "OK");

                    // Volver a la página anterior
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se pudo registrar la venta",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NuevaTransaccionViewModel] Error en ConfirmTransaction: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Ocurrió un error al registrar la venta: " + ex.Message,
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion
    }
}
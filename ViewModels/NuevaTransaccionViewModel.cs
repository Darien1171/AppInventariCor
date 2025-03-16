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

        // Propiedades para selección de repuesto (Paso 2) - MODIFICADO para selección múltiple
        private string _repuestoSearchQuery;
        private ObservableCollection<Repuesto> _repuestosFiltrados;
        private ObservableCollection<Repuesto> _selectedRepuestos;

        // Propiedades para datos específicos (Paso 3)
        private Dictionary<int, int> _cantidadesRepuestos; // Mapa de ID de repuesto a cantidad
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

                    Debug.WriteLine($"[DEBUG] Cambiado a paso {value}. CanGoForward={CanGoForward}");
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
                    2 => "Selección de Repuestos",
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
        public bool CanGoForward
        {
            get
            {
                bool result = CurrentStep < _totalSteps && ValidateCurrentStep();
                Debug.WriteLine($"[DEBUG] Evaluando CanGoForward: {result}");
                return result;
            }
        }

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
            set
            {
                if (_selectedVehiculo != value)
                {
                    _selectedVehiculo = value;
                    Debug.WriteLine($"[DEBUG] Vehículo seleccionado: {value?.NumeroPlaca ?? "ninguno"}, ID: {value?.Id.ToString() ?? "N/A"}");
                    OnPropertyChanged(nameof(SelectedVehiculo));
                    OnPropertyChanged(nameof(CanGoForward));

                    // Forzar actualización del estado visual
                    OnPropertyChanged(string.Empty);
                }
            }
        }

        // Propiedades para selección de repuesto (Paso 2) - MODIFICADO para selección múltiple
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

        // Nueva propiedad para selección múltiple de repuestos
        public ObservableCollection<Repuesto> SelectedRepuestos
        {
            get => _selectedRepuestos;
            set => SetProperty(ref _selectedRepuestos, value);
        }

        // Propiedad para determinar si hay repuestos seleccionados
        public bool HasSelectedRepuestos => SelectedRepuestos != null && SelectedRepuestos.Count > 0;

        // Propiedades para datos específicos (Paso 3)
        public Dictionary<int, int> CantidadesRepuestos
        {
            get => _cantidadesRepuestos;
            set => SetProperty(ref _cantidadesRepuestos, value);
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
                decimal total = 0;

                if (SelectedRepuestos != null && CantidadesRepuestos != null)
                {
                    foreach (var repuesto in SelectedRepuestos)
                    {
                        if (CantidadesRepuestos.TryGetValue(repuesto.Id, out int cantidad))
                        {
                            total += repuesto.Precio * cantidad;
                        }
                    }
                }

                return total;
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
        public ICommand SelectVehiculoCommand { get; }
        public ICommand ToggleRepuestoCommand { get; }
        public ICommand RemoveRepuestoCommand { get; }
        public ICommand ScanCommand { get; }
        public ICommand TakePictureCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand ClearSignatureCommand { get; }
        public ICommand ConfirmTransactionCommand { get; }
        public ICommand UpdateCantidadCommand { get; }
        public ICommand IncrementarCantidadCommand { get; }
        public ICommand DecrementarCantidadCommand { get; }
        #endregion

        // Constructor
        public NuevaTransaccionViewModel()
        {
            Title = "Nueva Venta";

            // Inicializar colecciones
            _vehiculosFiltrados = new ObservableCollection<Vehiculo>();
            _repuestosFiltrados = new ObservableCollection<Repuesto>();
            _selectedRepuestos = new ObservableCollection<Repuesto>();
            _cantidadesRepuestos = new Dictionary<int, int>();
            _evidenceImages = new ObservableCollection<ImageSource>();

            // Inicializar comandos
            PreviousStepCommand = new Command(PreviousStep);
            NextStepCommand = new Command(NextStep);
            VehiculoSearchCommand = new Command(SearchVehiculos);
            RepuestoSearchCommand = new Command(SearchRepuestos);
            SelectVehiculoCommand = new Command<Vehiculo>(SelectVehiculo);
            ToggleRepuestoCommand = new Command<Repuesto>(ToggleRepuestoSelection);
            RemoveRepuestoCommand = new Command<Repuesto>(RemoveRepuesto);
            ScanCommand = new Command(ScanRepuesto);
            TakePictureCommand = new Command(TakePicture);
            PickImageCommand = new Command(PickImage);
            RemoveImageCommand = new Command<ImageSource>(RemoveImage);
            ClearSignatureCommand = new Command(ClearSignature);
            ConfirmTransactionCommand = new Command(ConfirmTransaction);
            UpdateCantidadCommand = new Command<Tuple<Repuesto, string>>(UpdateCantidad);
            IncrementarCantidadCommand = new Command<Repuesto>(IncrementarCantidad);
            DecrementarCantidadCommand = new Command<Repuesto>(DecrementarCantidad);

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
                    VehiculosFiltrados.Clear();
                    foreach (var vehiculo in vehiculos.Take(10))
                    {
                        VehiculosFiltrados.Add(vehiculo);
                    }
                    Debug.WriteLine($"[DEBUG] Cargados {VehiculosFiltrados.Count} vehículos");
                }

                // Precargar algunos repuestos para el segundo paso
                var repuestos = await RepuestoJson.ObtenerRepuestos();
                if (repuestos != null && repuestos.Any())
                {
                    RepuestosFiltrados.Clear();
                    foreach (var repuesto in repuestos.Where(r => r.Disponible).Take(10))
                    {
                        RepuestosFiltrados.Add(repuesto);
                    }
                    Debug.WriteLine($"[DEBUG] Cargados {RepuestosFiltrados.Count} repuestos");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error en LoadInitialData: {ex.Message}");
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
            Debug.WriteLine($"[DEBUG] Intentando avanzar al siguiente paso. Paso actual: {CurrentStep}");

            if (ValidateCurrentStep())
            {
                Debug.WriteLine("[DEBUG] Validación exitosa, avanzando al siguiente paso");

                // Acciones especiales según el paso
                if (CurrentStep == 2)
                {
                    // Inicializar cantidades para repuestos seleccionados
                    InitializeCantidades();
                }

                if (CurrentStep < _totalSteps)
                {
                    CurrentStep++;
                }
            }
            else
            {
                Debug.WriteLine("[DEBUG] Validación fallida, no se puede avanzar");
                string mensaje = "Por favor complete todos los campos requeridos para continuar.";

                // Mensajes específicos según el paso
                if (CurrentStep == 1 && SelectedVehiculo == null)
                {
                    mensaje = "Por favor seleccione un vehículo para continuar.";
                }
                else if (CurrentStep == 2 && (SelectedRepuestos == null || SelectedRepuestos.Count == 0))
                {
                    mensaje = "Por favor seleccione al menos un repuesto para continuar.";
                }

                Application.Current.MainPage.DisplayAlert("Validación", mensaje, "OK");
            }
        }

        public void UpdateCantidadDirecto(Repuesto repuesto, int cantidad)
        {
            if (repuesto == null || cantidad <= 0)
                return;

            // Validar que no exceda el stock
            if (cantidad <= repuesto.Cantidad)
            {
                CantidadesRepuestos[repuesto.Id] = cantidad;
                OnPropertyChanged(nameof(ValorTotal));
                OnPropertyChanged(nameof(CanGoForward));
                CheckStockWarning();
            }
            else
            {
                // Ajustar al máximo disponible
                CantidadesRepuestos[repuesto.Id] = repuesto.Cantidad;
                OnPropertyChanged(nameof(ValorTotal));

                // Notificar al usuario
                Application.Current.MainPage.DisplayAlert(
                    "Advertencia",
                    $"La cantidad solicitada excede el stock disponible ({repuesto.Cantidad}). Se ha ajustado al máximo disponible.",
                    "OK");
            }
        }

        private void InitializeCantidades()
        {
            try
            {
                // Reinicializar el diccionario para evitar problemas
                _cantidadesRepuestos = new Dictionary<int, int>();

                // Inicializar cantidades para cada repuesto seleccionado
                foreach (var repuesto in SelectedRepuestos)
                {
                    _cantidadesRepuestos[repuesto.Id] = 1; // Siempre valor predeterminado 1
                }

                OnPropertyChanged(nameof(CantidadesRepuestos));
                OnPropertyChanged(nameof(ValorTotal));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error en InitializeCantidades: {ex.Message}");
            }
        }

        // Método actualizado para recibir un Tuple<Repuesto, string>
        private void UpdateCantidad(Tuple<Repuesto, string> data)
        {
            if (data == null || data.Item1 == null || string.IsNullOrEmpty(data.Item2))
                return;

            var repuesto = data.Item1;
            var cantidadStr = data.Item2;

            if (int.TryParse(cantidadStr, out int cantidad) && cantidad > 0)
            {
                // Validar que no exceda el stock
                if (cantidad <= repuesto.Cantidad)
                {
                    CantidadesRepuestos[repuesto.Id] = cantidad;
                    OnPropertyChanged(nameof(ValorTotal));
                    OnPropertyChanged(nameof(CanGoForward));
                    CheckStockWarning();
                }
                else
                {
                    // Ajustar al máximo disponible
                    CantidadesRepuestos[repuesto.Id] = repuesto.Cantidad;
                    OnPropertyChanged(nameof(ValorTotal));

                    // Notificar al usuario
                    Application.Current.MainPage.DisplayAlert(
                        "Advertencia",
                        $"La cantidad solicitada excede el stock disponible ({repuesto.Cantidad}). Se ha ajustado al máximo disponible.",
                        "OK");
                }
            }
        }

        public void IncrementarCantidad(Repuesto repuesto)
        {
            if (repuesto == null || !CantidadesRepuestos.TryGetValue(repuesto.Id, out int cantidadActual))
                return;

            // Validar que no exceda el stock
            if (cantidadActual < repuesto.Cantidad)
            {
                CantidadesRepuestos[repuesto.Id] = cantidadActual + 1;
                OnPropertyChanged(nameof(ValorTotal));
                OnPropertyChanged(nameof(CantidadesRepuestos)); // Añadido
                CheckStockWarning();

                // Notificar a la vista que debe actualizar los valores
                OnPropertyChanged("ActualizarCantidades");
            }
            else
            {
                // Notificar al usuario que ha alcanzado el máximo
                Application.Current.MainPage.DisplayAlert(
                    "Información",
                    $"No se puede aumentar más la cantidad. El stock disponible es {repuesto.Cantidad}.",
                    "OK");
            }
        }


        public void DecrementarCantidad(Repuesto repuesto)
        {
            if (repuesto == null || !CantidadesRepuestos.TryGetValue(repuesto.Id, out int cantidadActual))
                return;

            if (cantidadActual > 1)
            {
                CantidadesRepuestos[repuesto.Id] = cantidadActual - 1;
                OnPropertyChanged(nameof(ValorTotal));
                OnPropertyChanged(nameof(CantidadesRepuestos)); // Añadido
                CheckStockWarning();

                // Notificar a la vista que debe actualizar los valores
                OnPropertyChanged("ActualizarCantidades");
            }
        }

        private bool ValidateCurrentStep()
        {
            // Validar según el paso actual
            switch (CurrentStep)
            {
                case 1: // Selección de vehículo
                    bool step1Valid = SelectedVehiculo != null;
                    Debug.WriteLine($"[DEBUG] Validación paso 1: {step1Valid}, Vehículo: {SelectedVehiculo?.NumeroPlaca ?? "ninguno"}");
                    return step1Valid;

                case 2: // Selección de repuestos - MODIFICADO para selección múltiple
                    bool step2Valid = SelectedRepuestos != null && SelectedRepuestos.Count > 0;
                    Debug.WriteLine($"[DEBUG] Validación paso 2: {step2Valid}, Repuestos seleccionados: {SelectedRepuestos?.Count ?? 0}");
                    return step2Valid;

                case 3: // Detalles de venta - MODIFICADO para múltiples repuestos
                    if (CantidadesRepuestos == null || SelectedRepuestos == null)
                    {
                        Debug.WriteLine("[DEBUG] Validación paso 3: false - Datos de cantidades no inicializados");
                        return false;
                    }

                    // Verificar que todas las cantidades sean válidas
                    foreach (var repuesto in SelectedRepuestos)
                    {
                        if (!CantidadesRepuestos.TryGetValue(repuesto.Id, out int cantidad) || cantidad <= 0)
                        {
                            Debug.WriteLine($"[DEBUG] Validación paso 3: false - Cantidad inválida para repuesto {repuesto.Codigo}");
                            return false;
                        }

                        // Verificar stock disponible
                        if (cantidad > repuesto.Cantidad)
                        {
                            Debug.WriteLine($"[DEBUG] Validación paso 3: false - Cantidad excede stock para repuesto {repuesto.Codigo}");
                            return false;
                        }
                    }

                    return true;

                case 4: // Evidencia fotográfica
                    // No hay validación específica, la evidencia es opcional
                    return true;

                default:
                    return true;
            }
        }

        // Método para seleccionar vehículo
        private void SelectVehiculo(Vehiculo vehiculo)
        {
            Debug.WriteLine($"[DEBUG] Método SelectVehiculo llamado con: {vehiculo?.NumeroPlaca ?? "ninguno"}");
            if (vehiculo != null)
            {
                SelectedVehiculo = vehiculo;

                // Forzar actualización del estado de navegación
                OnPropertyChanged(nameof(CanGoForward));
            }
        }

        // Método para alternar la selección de un repuesto
        private void ToggleRepuestoSelection(Repuesto repuesto)
        {
            if (repuesto == null) return;

            // Buscar si el repuesto ya está seleccionado
            bool isSelected = SelectedRepuestos.Any(r => r.Id == repuesto.Id);

            if (isSelected)
            {
                // Quitar de la selección
                var itemToRemove = SelectedRepuestos.FirstOrDefault(r => r.Id == repuesto.Id);
                if (itemToRemove != null)
                {
                    SelectedRepuestos.Remove(itemToRemove);

                    // Quitar del diccionario de cantidades
                    if (CantidadesRepuestos.ContainsKey(repuesto.Id))
                    {
                        CantidadesRepuestos.Remove(repuesto.Id);
                    }
                }
            }
            else
            {
                // Añadir a la selección
                SelectedRepuestos.Add(repuesto);

                // Inicializar cantidad por defecto
                CantidadesRepuestos[repuesto.Id] = 1;
            }

            Debug.WriteLine($"[DEBUG] Repuesto {repuesto.Codigo} {(isSelected ? "quitado de" : "añadido a")} la selección. Total: {SelectedRepuestos.Count}");

            // Actualizar propiedades relacionadas
            OnPropertyChanged(nameof(HasSelectedRepuestos));
            OnPropertyChanged(nameof(CanGoForward));
            OnPropertyChanged(nameof(ValorTotal));
        }

        // Método para quitar un repuesto de la selección
        private void RemoveRepuesto(Repuesto repuesto)
        {
            if (repuesto == null) return;

            var itemToRemove = SelectedRepuestos.FirstOrDefault(r => r.Id == repuesto.Id);
            if (itemToRemove != null)
            {
                SelectedRepuestos.Remove(itemToRemove);

                // Quitar del diccionario de cantidades
                if (CantidadesRepuestos.ContainsKey(repuesto.Id))
                {
                    CantidadesRepuestos.Remove(repuesto.Id);
                }

                Debug.WriteLine($"[DEBUG] Repuesto {repuesto.Codigo} quitado de la selección. Total: {SelectedRepuestos.Count}");

                // Actualizar propiedades relacionadas
                OnPropertyChanged(nameof(HasSelectedRepuestos));
                OnPropertyChanged(nameof(CanGoForward));
                OnPropertyChanged(nameof(ValorTotal));
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

                Debug.WriteLine($"[DEBUG] Búsqueda completada: {VehiculosFiltrados.Count} vehículos encontrados");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error en SearchVehiculos: {ex.Message}");
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

                Debug.WriteLine($"[DEBUG] Búsqueda completada: {RepuestosFiltrados.Count} repuestos encontrados");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error en SearchRepuestos: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CheckStockWarning()
        {
            if (SelectedRepuestos == null || CantidadesRepuestos == null)
            {
                ShowStockWarning = false;
                return;
            }

            // Verificar si algún repuesto quedará con stock bajo
            ShowStockWarning = false;

            foreach (var repuesto in SelectedRepuestos)
            {
                if (CantidadesRepuestos.TryGetValue(repuesto.Id, out int cantidad))
                {
                    int stockRestante = repuesto.Cantidad - cantidad;
                    if (stockRestante < repuesto.StockMinimo && stockRestante >= 0)
                    {
                        ShowStockWarning = true;
                        break;
                    }
                }
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
                var repuesto = RepuestosFiltrados.First();

                // Agregar a seleccionados si no está ya
                if (!SelectedRepuestos.Any(r => r.Id == repuesto.Id))
                {
                    SelectedRepuestos.Add(repuesto);
                    CantidadesRepuestos[repuesto.Id] = 1;

                    OnPropertyChanged(nameof(HasSelectedRepuestos));
                    OnPropertyChanged(nameof(CanGoForward));
                }
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
                if (SelectedVehiculo == null || SelectedRepuestos == null || SelectedRepuestos.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "Faltan datos requeridos para completar la transacción.",
                        "OK");
                    return;
                }

                // Lista para almacenar las transacciones creadas
                var transaccionesCreadas = new List<bool>();

                // Crear una transacción para cada repuesto seleccionado
                foreach (var repuesto in SelectedRepuestos)
                {
                    // Obtener la cantidad para este repuesto
                    if (!CantidadesRepuestos.TryGetValue(repuesto.Id, out int cantidad) || cantidad <= 0)
                    {
                        continue; // Saltar este repuesto si no tiene cantidad válida
                    }

                    // Verificar stock disponible
                    if (cantidad > repuesto.Cantidad)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Error",
                            $"No hay suficiente stock disponible para el repuesto {repuesto.Nombre} (Código: {repuesto.Codigo}).",
                            "OK");
                        continue;
                    }

                    // Calcular cantidades para historial
                    int cantidadAnterior = repuesto.Cantidad;
                    int cantidadPosterior = cantidadAnterior - cantidad;
                    decimal valorTotal = repuesto.Precio * cantidad;

                    // Crear objeto de transacción
                    var transaccion = new Transaccion
                    {
                        Fecha = DateTime.Now,
                        Tipo = TipoTransaccion.Salida,
                        Cantidad = cantidad,
                        CantidadAnterior = cantidadAnterior,
                        CantidadPosterior = cantidadPosterior,
                        PrecioUnitario = repuesto.Precio,
                        ValorTotal = valorTotal,
                        Observaciones = Observaciones,
                        RepuestoId = repuesto.Id,
                        RepuestoCodigo = repuesto.Codigo,
                        RepuestoNombre = repuesto.Nombre,
                        VehiculoId = SelectedVehiculo.Id,
                        VehiculoPlaca = SelectedVehiculo.NumeroPlaca,
                        ResponsableNombre = "Usuario Actual", // En una implementación real, obtendríamos el usuario logueado
                        FechaCreacion = DateTime.Now
                    };

                    // Actualizar el inventario (reducir stock)
                    repuesto.Cantidad = cantidadPosterior;
                    await RepuestoJson.ActualizarRepuesto(repuesto);

                    // Guardar la transacción
                    bool resultado = await TransaccionJson.AgregarTransaccion(transaccion);
                    transaccionesCreadas.Add(resultado);
                }

                // Verificar si todas las transacciones se guardaron correctamente
                if (transaccionesCreadas.Count > 0 && transaccionesCreadas.All(t => t))
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Éxito",
                        $"Se han registrado {transaccionesCreadas.Count} transacciones correctamente.",
                        "OK");

                    // Volver a la página anterior
                    await Shell.Current.GoToAsync("..");
                }
                else if (transaccionesCreadas.Count > 0)
                {
                    // Algunas transacciones se guardaron, otras no
                    await Application.Current.MainPage.DisplayAlert(
                        "Advertencia",
                        $"Se registraron {transaccionesCreadas.Count(t => t)} de {transaccionesCreadas.Count} transacciones. Algunas tuvieron errores.",
                        "OK");

                    // Volver a la página anterior
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se pudo registrar ninguna transacción. Por favor intente nuevamente.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error en ConfirmTransaction: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Ocurrió un error al registrar las transacciones: " + ex.Message,
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
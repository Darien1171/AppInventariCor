using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AppInventariCor.Models;
using System.Linq;
using System.Collections.Generic;
using AppInventariCor.Views;
using AppInventariCor.Services;
using System.Diagnostics;

namespace AppInventariCor.ViewModels
{
    public class VehiculosViewModel : BaseViewModel
    {
        private ObservableCollection<Vehiculo> _vehiculos;
        private string _searchQuery;
        private int _currentPage = 1;
        private readonly int _pageSize = 10;
        private bool _hasMoreItems = true;
        private bool _isInitializing = true;
        private string _loadingMessage = "Preparando datos...";
        private int _totalVehiculos;

        private bool _buscarPorPlaca = true;
        private bool _buscarPorNumeroInterno;
        private string _marcaSeleccionada;
        private string _propietarioSeleccionado;
        private ObservableCollection<string> _marcasFiltro;
        private ObservableCollection<string> _propietariosFiltro;

        // Propiedades
        public ObservableCollection<Vehiculo> Vehiculos
        {
            get => _vehiculos;
            set => SetProperty(ref _vehiculos, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    _currentPage = 1;
                    RefreshCommand.Execute(null);
                }
            }
        }

        public bool IsInitializing
        {
            get => _isInitializing;
            set => SetProperty(ref _isInitializing, value);
        }

        public string LoadingMessage
        {
            get => _loadingMessage;
            set => SetProperty(ref _loadingMessage, value);
        }

        public int TotalVehiculos
        {
            get => _totalVehiculos;
            set => SetProperty(ref _totalVehiculos, value);
        }



        public bool BuscarPorPlaca
        {
            get => _buscarPorPlaca;
            set
            {
                if (SetProperty(ref _buscarPorPlaca, value) && value)
                {
                    _buscarPorNumeroInterno = false;
                    OnPropertyChanged(nameof(BuscarPorNumeroInterno));
                    RealizarBusqueda();
                }
            }
        }

        public bool BuscarPorNumeroInterno
        {
            get => _buscarPorNumeroInterno;
            set
            {
                if (SetProperty(ref _buscarPorNumeroInterno, value) && value)
                {
                    _buscarPorPlaca = false;
                    OnPropertyChanged(nameof(BuscarPorPlaca));
                    RealizarBusqueda();
                }
            }
        }

        public string MarcaSeleccionada
        {
            get => _marcaSeleccionada;
            set
            {
                if (SetProperty(ref _marcaSeleccionada, value))
                {
                    RealizarBusqueda();
                }
            }
        }

        public string PropietarioSeleccionado
        {
            get => _propietarioSeleccionado;
            set
            {
                if (SetProperty(ref _propietarioSeleccionado, value))
                {
                    RealizarBusqueda();
                }
            }
        }

        public ObservableCollection<string> MarcasFiltro
        {
            get => _marcasFiltro;
            set => SetProperty(ref _marcasFiltro, value);
        }

        public ObservableCollection<string> PropietariosFiltro
        {
            get => _propietariosFiltro;
            set => SetProperty(ref _propietariosFiltro, value);
        }

        // Comandos
        public ICommand AddVehiculoCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand VehiculoDetailCommand { get; }

        public VehiculosViewModel()
        {
            Title = "Vehículos";

            // Inicializar colecciones
            Vehiculos = new ObservableCollection<Vehiculo>();
            MarcasFiltro = new ObservableCollection<string>();
            PropietariosFiltro = new ObservableCollection<string>();

            // Inicializar comandos
            AddVehiculoCommand = CreateCommand(OnAddVehiculo);
            SearchCommand = CreateCommand(OnSearch);
            RefreshCommand = CreateCommand(OnRefresh);
            LoadMoreCommand = CreateCommand(OnLoadMore);
            VehiculoDetailCommand = CreateCommand<Vehiculo>(OnVehiculoDetail);

            // Iniciar la carga asíncrona
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            // Indicar que estamos en estado de inicialización
            IsInitializing = true;

            try
            {
                // Secuencia de estados de carga para una mejor experiencia visual
                await Task.Delay(300);
                LoadingMessage = "Cargando vehículos...";
                await Task.Delay(300);

                // Obtener datos del JSON para estadísticas
                var vehiculos = await VehiculoJson.ObtenerVehiculos();
                TotalVehiculos = vehiculos.Count;

                // Cargar opciones de filtro
                CargarOpcionesFiltro(vehiculos);

                LoadingMessage = "Preparando lista de vehículos...";
                await Task.Delay(300);

                // Cargar primera página de datos
                await LoadDataAsync();

                // Dar tiempo para una transición visual más suave
                await Task.Delay(300);
            }
            catch (Exception ex)
            {
                LoadingMessage = $"Error al cargar: {ex.Message}";
                await Task.Delay(1500); // Mostrar el mensaje de error brevemente

                // Manejo formal del error
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo inicializar la lista de vehículos: {ex.Message}",
                    "OK");
            }
            finally
            {
                // Completar la inicialización
                IsInitializing = false;
            }
        }

        private void CargarOpcionesFiltro(List<Vehiculo> vehiculos)
        {
            // Obtener marcas únicas para el filtro
            var marcas = vehiculos
                .Select(v => v.Marca)
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            // Obtener propietarios únicos para el filtro
            var propietarios = vehiculos
                .Select(v => v.Propietario)
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            // Añadir opción "Todos" al principio
            marcas.Insert(0, "Todas las marcas");
            propietarios.Insert(0, "Todos los propietarios");

            // Actualizar colecciones observables
            MarcasFiltro.Clear();
            foreach (var marca in marcas)
            {
                MarcasFiltro.Add(marca);
            }

            PropietariosFiltro.Clear();
            foreach (var propietario in propietarios)
            {
                PropietariosFiltro.Add(propietario);
            }

            // Seleccionar el primer elemento por defecto
            MarcaSeleccionada = MarcasFiltro.FirstOrDefault();
            PropietarioSeleccionado = PropietariosFiltro.FirstOrDefault();
        }

        // Métodos para cargar datos
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                // Simular tiempo de carga
                await Task.Delay(300);

                // Limpiar lista si es primera página
                if (_currentPage == 1)
                {
                    Vehiculos.Clear();
                }

                // Obtener datos del JSON
                var todosVehiculos = await VehiculoJson.ObtenerVehiculos();

                // Aplicar filtros
                var vehiculosFiltrados = AplicarFiltros(todosVehiculos);

                // Aplicar paginación
                var nuevosVehiculos = vehiculosFiltrados
                    .Skip((_currentPage - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToList();

                // Verificar si hay más datos
                _hasMoreItems = nuevosVehiculos.Count == _pageSize &&
                                ((_currentPage * _pageSize) < vehiculosFiltrados.Count);

                // Añadir los elementos a la colección
                foreach (var vehiculo in nuevosVehiculos)
                {
                    Vehiculos.Add(vehiculo);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudieron cargar los vehículos: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private List<Vehiculo> AplicarFiltros(List<Vehiculo> vehiculos)
        {
            var resultado = vehiculos.ToList();

            // Aplicar filtro de búsqueda
            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                string query = _searchQuery.ToLower();

                if (_buscarPorPlaca)
                {
                    resultado = resultado.Where(v =>
                        (v.NumeroPlaca?.ToLower().Contains(query) ?? false)).ToList();
                }
                else if (_buscarPorNumeroInterno)
                {
                    resultado = resultado.Where(v =>
                        (v.NumeroInterno?.ToLower().Contains(query) ?? false)).ToList();
                }
            }

            // Aplicar filtro de marca
            if (!string.IsNullOrEmpty(_marcaSeleccionada) && _marcaSeleccionada != "Todas las marcas")
            {
                resultado = resultado.Where(v => v.Marca == _marcaSeleccionada).ToList();
            }

            // Aplicar filtro de propietario
            if (!string.IsNullOrEmpty(_propietarioSeleccionado) && _propietarioSeleccionado != "Todos los propietarios")
            {
                resultado = resultado.Where(v => v.Propietario == _propietarioSeleccionado).ToList();
            }

            return resultado;
        }

        // Manejadores de comandos
        private void RealizarBusqueda()
        {
            _currentPage = 1;
            Task.Run(async () => await LoadDataAsync());
        }

        private void OnSearch()
        {
            RealizarBusqueda();
        }

        private void OnRefresh()
        {
            RealizarBusqueda();
        }

        private void OnLoadMore()
        {
            if (!IsBusy && _hasMoreItems)
            {
                _currentPage++;
                Task.Run(async () => await LoadDataAsync());
            }
        }

        private async void OnAddVehiculo()
        {
            try
            {
                // Navegar a la página de agregar vehículo
                await Shell.Current.GoToAsync(nameof(AgregarVehiculoPage));
            }
            catch (Exception ex)
            {
                // Registrar el error y mostrar un mensaje amigable
                Debug.WriteLine($"Error de navegación: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "No se pudo abrir la página para agregar vehículo. " +
                    "Por favor, intente nuevamente.",
                    "OK");
            }
        }

        private async void OnVehiculoDetail(Vehiculo vehiculo)
        {
            if (vehiculo == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se seleccionó ningún vehículo", "OK");
                return;
            }

            try
            {
                // Enviar ID como parámetro para que la página de detalles pueda cargarlo desde JSON
                var navigationParameter = new Dictionary<string, object>
                {
                    { "VehiculoId", vehiculo.Id }
                };

                await Shell.Current.GoToAsync($"{nameof(VehiculoDetallePage)}", navigationParameter);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error de Navegación",
                    $"Detalle del error: {ex.Message}\n\nTipo: {ex.GetType().Name}\n\nStack: {ex.StackTrace}",
                    "OK");
            }
        }
    }
}
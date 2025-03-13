using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AppInventariCor.Models;
using System.Linq;
using System.Collections.Generic;
using AppInventariCor.Views;

namespace AppInventariCor.ViewModels
{
    public class InventarioViewModel : BaseViewModel
    {
        private ObservableCollection<Repuesto> _repuestos;
        private string _searchQuery;
        private int _currentPage = 1;
        private readonly int _pageSize = 10;
        private bool _hasMoreItems = true;
        private bool _isInitializing = true;
        private string _loadingMessage = "Preparando datos...";
        private int _totalRepuestos;
        private int _totalStockBajo;

        // Propiedades
        public ObservableCollection<Repuesto> Repuestos
        {
            get => _repuestos;
            set => SetProperty(ref _repuestos, value);
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

        public int TotalRepuestos
        {
            get => _totalRepuestos;
            set => SetProperty(ref _totalRepuestos, value);
        }

        public int TotalStockBajo
        {
            get => _totalStockBajo;
            set => SetProperty(ref _totalStockBajo, value);
        }

        // Comandos
        public ICommand AddRepuestoCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand RepuestoDetailCommand { get; }

        public InventarioViewModel()
        {
            Title = "Inventario";

            // Inicializar colecciones
            Repuestos = new ObservableCollection<Repuesto>();

            // Inicializar comandos
            AddRepuestoCommand = CreateCommand(OnAddRepuesto);
            SearchCommand = CreateCommand(OnSearch);
            RefreshCommand = CreateCommand(OnRefresh);
            LoadMoreCommand = CreateCommand(OnLoadMore);
            RepuestoDetailCommand = CreateCommand<Repuesto>(OnRepuestoDetail);

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
                LoadingMessage = "Cargando inventario...";
                await Task.Delay(300);

                // Obtener datos de muestra completos para estadísticas
                var allData = GetSampleData();
                TotalRepuestos = allData.Count;
                TotalStockBajo = allData.Count(r => r.Cantidad <= r.StockMinimo);

                LoadingMessage = "Preparando lista de repuestos...";
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
                    $"No se pudo inicializar el inventario: {ex.Message}",
                    "OK");
            }
            finally
            {
                // Completar la inicialización
                IsInitializing = false;
            }
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
                    Repuestos.Clear();
                }

                // Obtener datos paginados aplicando filtros si existen
                var nuevosRepuestos = GetFilteredData()
                    .Skip((_currentPage - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToList();

                // Verificar si hay más datos
                _hasMoreItems = nuevosRepuestos.Count == _pageSize;

                // Añadir los elementos a la colección
                foreach (var repuesto in nuevosRepuestos)
                {
                    Repuestos.Add(repuesto);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudieron cargar los repuestos: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Obtener datos filtrados según la búsqueda
        private List<Repuesto> GetFilteredData()
        {
            var data = GetSampleData();

            // Aplicar filtro si hay texto de búsqueda
            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                string query = _searchQuery.ToLower();
                return data.Where(r =>
                    r.Nombre.ToLower().Contains(query) ||
                    r.Codigo.ToLower().Contains(query) ||
                    r.Categoria.ToLower().Contains(query)).ToList();
            }

            return data;
        }

        // Simular datos de muestra
        private List<Repuesto> GetSampleData()
        {
            return new List<Repuesto>
            {
                new Repuesto
                {
                    Id = 1,
                    Codigo = "FLT-5678",
                    Nombre = "Filtro de aceite XYZ-123",
                    Categoria = "Filtros",
                    Marca = "FilterPro",
                    Precio = 45.99m,
                    Cantidad = 12,
                    StockMinimo = 5
                },
                new Repuesto
                {
                    Id = 2,
                    Codigo = "BDJ-1234",
                    Nombre = "Bombilla delantera LED",
                    Categoria = "Iluminación",
                    Marca = "LightMaster",
                    Precio = 28.50m,
                    Cantidad = 8,
                    StockMinimo = 10
                },
                new Repuesto
                {
                    Id = 3,
                    Codigo = "ACE-7890",
                    Nombre = "Aceite de motor sintético 10W-40",
                    Categoria = "Lubricantes",
                    Marca = "OilTech",
                    Precio = 35.75m,
                    Cantidad = 20,
                    StockMinimo = 15
                },
                new Repuesto
                {
                    Id = 4,
                    Codigo = "FRN-4567",
                    Nombre = "Pastillas de freno delanteras",
                    Categoria = "Frenos",
                    Marca = "BrakeSafe",
                    Precio = 65.25m,
                    Cantidad = 4,
                    StockMinimo = 8
                },
                new Repuesto
                {
                    Id = 5,
                    Codigo = "BAT-9876",
                    Nombre = "Batería 12V 75Ah",
                    Categoria = "Eléctricos",
                    Marca = "PowerCell",
                    Precio = 120.00m,
                    Cantidad = 2,
                    StockMinimo = 3
                },
                new Repuesto
                {
                    Id = 6,
                    Codigo = "RAD-3456",
                    Nombre = "Radiador de agua",
                    Categoria = "Refrigeración",
                    Marca = "CoolSys",
                    Precio = 95.50m,
                    Cantidad = 2,
                    StockMinimo = 2
                },
                new Repuesto
                {
                    Id = 7,
                    Codigo = "ESC-7821",
                    Nombre = "Escobillas limpiaparabrisas",
                    Categoria = "Exterior",
                    Marca = "CleanView",
                    Precio = 15.75m,
                    Cantidad = 15,
                    StockMinimo = 5
                },
                new Repuesto
                {
                    Id = 8,
                    Codigo = "BUJ-9102",
                    Nombre = "Bujías de encendido",
                    Categoria = "Motor",
                    Marca = "SparkTech",
                    Precio = 8.99m,
                    Cantidad = 30,
                    StockMinimo = 10
                },
                new Repuesto
                {
                    Id = 9,
                    Codigo = "AMO-4355",
                    Nombre = "Amortiguadores traseros",
                    Categoria = "Suspensión",
                    Marca = "SmoothRide",
                    Precio = 89.50m,
                    Cantidad = 4,
                    StockMinimo = 4
                },
                new Repuesto
                {
                    Id = 10,
                    Codigo = "COR-6543",
                    Nombre = "Correa de distribución",
                    Categoria = "Motor",
                    Marca = "BeltPro",
                    Precio = 35.25m,
                    Cantidad = 3,
                    StockMinimo = 5
                },
                new Repuesto
                {
                    Id = 11,
                    Codigo = "EMB-2109",
                    Nombre = "Kit de embrague",
                    Categoria = "Transmisión",
                    Marca = "ClutchMaster",
                    Precio = 145.99m,
                    Cantidad = 3,
                    StockMinimo = 2
                },
                new Repuesto
                {
                    Id = 12,
                    Codigo = "FIL-3322",
                    Nombre = "Filtro de habitáculo",
                    Categoria = "Filtros",
                    Marca = "AirClean",
                    Precio = 12.50m,
                    Cantidad = 9,
                    StockMinimo = 8
                },
                new Repuesto
                {
                    Id = 13,
                    Codigo = "TER-8576",
                    Nombre = "Termostato motor",
                    Categoria = "Refrigeración",
                    Marca = "TempControl",
                    Precio = 22.75m,
                    Cantidad = 7,
                    StockMinimo = 5
                },
                new Repuesto
                {
                    Id = 14,
                    Codigo = "ALT-6701",
                    Nombre = "Alternador 12V 90A",
                    Categoria = "Eléctricos",
                    Marca = "PowerGen",
                    Precio = 129.99m,
                    Cantidad = 4,
                    StockMinimo = 2
                },
                new Repuesto
                {
                    Id = 15,
                    Codigo = "BOC-3291",
                    Nombre = "Bocina doble tono",
                    Categoria = "Accesorios",
                    Marca = "SoundAlert",
                    Precio = 18.50m,
                    Cantidad = 12,
                    StockMinimo = 5
                },
                new Repuesto
                {
                    Id = 16,
                    Codigo = "MAN-4491",
                    Nombre = "Manguera radiador",
                    Categoria = "Refrigeración",
                    Marca = "FlexTube",
                    Precio = 14.25m,
                    Cantidad = 8,
                    StockMinimo = 6
                },
                new Repuesto
                {
                    Id = 17,
                    Codigo = "BOB-7734",
                    Nombre = "Bobina de encendido",
                    Categoria = "Ignición",
                    Marca = "SparkMaster",
                    Precio = 45.00m,
                    Cantidad = 6,
                    StockMinimo = 4
                },
                new Repuesto
                {
                    Id = 18,
                    Codigo = "SOP-9921",
                    Nombre = "Soporte de motor",
                    Categoria = "Suspensión",
                    Marca = "EngineMount",
                    Precio = 72.50m,
                    Cantidad = 3,
                    StockMinimo = 2
                },
                new Repuesto
                {
                    Id = 19,
                    Codigo = "RET-1105",
                    Nombre = "Retén cigüeñal",
                    Categoria = "Motor",
                    Marca = "SealPro",
                    Precio = 11.99m,
                    Cantidad = 15,
                    StockMinimo = 8
                },
                new Repuesto
                {
                    Id = 20,
                    Codigo = "RUL-5509",
                    Nombre = "Rodamiento delantero",
                    Categoria = "Transmisión",
                    Marca = "BearingTech",
                    Precio = 32.75m,
                    Cantidad = 8,
                    StockMinimo = 4
                }
            };
        }

        // Manejadores de comandos
        private void OnSearch()
        {
            _currentPage = 1;
            Task.Run(async () => await LoadDataAsync());
        }

        private void OnRefresh()
        {
            _currentPage = 1;
            Task.Run(async () => await LoadDataAsync());
        }

        private void OnLoadMore()
        {
            if (!IsBusy && _hasMoreItems)
            {
                _currentPage++;
                Task.Run(async () => await LoadDataAsync());
            }
        }

        private async void OnAddRepuesto()
        {
            // Solo un placeholder para la funcionalidad
            await Application.Current.MainPage.DisplayAlert(
                "Agregar Repuesto",
                "Esta funcionalidad se implementará próximamente",
                "OK");
        }

        private async void OnRepuestoDetail(Repuesto repuesto)
        {
            if (repuesto == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se seleccionó ningún repuesto", "OK");
                return;
            }

            // Mostrar un mensaje para confirmar que se seleccionó un repuesto
            await Application.Current.MainPage.DisplayAlert("Repuesto Seleccionado",
                $"Has seleccionado: {repuesto.Nombre} (ID: {repuesto.Id})", "Continuar");

            try
            {
                // Navegar a la página de detalles
                var navigationParameter = new Dictionary<string, object>
        {
            { "id", repuesto.Id }
        };

                await Shell.Current.GoToAsync($"{nameof(Views.RepuestoDetallePage)}", navigationParameter);
            }
            catch (Exception ex)
            {
                // Mostrar cualquier error que ocurra durante la navegación
                await Application.Current.MainPage.DisplayAlert("Error de Navegación",
                    $"No se pudo navegar a la página de detalles: {ex.Message}", "OK");
            }
        }
    }
}
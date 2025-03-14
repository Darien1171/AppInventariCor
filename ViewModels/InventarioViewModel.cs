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

                // Obtener datos del JSON para estadísticas
                var repuestos = await RepuestoJson.ObtenerRepuestos();
                TotalRepuestos = repuestos.Count;
                TotalStockBajo = repuestos.Count(r => r.Cantidad <= r.StockMinimo);

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

                // Obtener datos del JSON
                var todosRepuestos = await RepuestoJson.ObtenerRepuestos();

                // Aplicar filtros si hay búsqueda
                var repuestosFiltrados = todosRepuestos;
                if (!string.IsNullOrWhiteSpace(_searchQuery))
                {
                    string query = _searchQuery.ToLower();
                    repuestosFiltrados = todosRepuestos.Where(r =>
                        (r.Nombre?.ToLower().Contains(query) ?? false) ||
                        (r.Codigo?.ToLower().Contains(query) ?? false) ||
                        (r.Categoria?.ToLower().Contains(query) ?? false)).ToList();
                }

                // Aplicar paginación
                var nuevosRepuestos = repuestosFiltrados
                    .Skip((_currentPage - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToList();

                // Verificar si hay más datos
                _hasMoreItems = nuevosRepuestos.Count == _pageSize &&
                                ((_currentPage * _pageSize) < repuestosFiltrados.Count);

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
            // Navegar a la página de agregar repuesto
            await Shell.Current.GoToAsync(nameof(AgregarRepuestoPage));
        }

        private async void OnRepuestoDetail(Repuesto repuesto)
        {
            if (repuesto == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se seleccionó ningún repuesto", "OK");
                return;
            }

            try
            {
                // Enviar ID como parámetro para que la página de detalles pueda cargarlo desde JSON
                var navigationParameter = new Dictionary<string, object>
                {
                    { "RepuestoId", repuesto.Id }
                };

                await Shell.Current.GoToAsync($"{nameof(RepuestoDetallePage)}", navigationParameter);
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
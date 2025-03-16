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
using AppInventariCor.Views;

namespace AppInventariCor.ViewModels
{
    public class TransaccionesViewModel : BaseViewModel
    {
        // Variables para el estado de inicialización/carga
        private bool _isInitializing = true;
        private string _loadingMessage = "Preparando datos...";

        // Variables para paginación y búsqueda
        private string _searchQuery;
        private int _currentPage = 1;
        private readonly int _pageSize = 10;
        private bool _hasMoreItems = true;

        // Propiedades para filtros
        private bool _filtroTodas = true;
        private bool _filtroEntradas;
        private bool _filtroSalidas;
        private bool _filtroAjustes;

        // Propiedades para estadísticas
        private int _totalTransacciones;
        private int _transaccionesSemana;
        private decimal _valorTotal;

        // Colecciones
        private ObservableCollection<Transaccion> _transacciones;

        #region Propiedades de Binding

        // Propiedades de estado de inicialización
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

        // Propiedades para paginación y búsqueda
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

        // Propiedades para filtros
        public bool FiltroTodas
        {
            get => _filtroTodas;
            set
            {
                if (SetProperty(ref _filtroTodas, value) && value)
                {
                    ResetOtherFilters("Todas");
                    RefreshCommand.Execute(null);
                }
            }
        }

        public bool FiltroEntradas
        {
            get => _filtroEntradas;
            set
            {
                if (SetProperty(ref _filtroEntradas, value) && value)
                {
                    ResetOtherFilters("Entradas");
                    RefreshCommand.Execute(null);
                }
            }
        }

        public bool FiltroSalidas
        {
            get => _filtroSalidas;
            set
            {
                if (SetProperty(ref _filtroSalidas, value) && value)
                {
                    ResetOtherFilters("Salidas");
                    RefreshCommand.Execute(null);
                }
            }
        }

        public bool FiltroAjustes
        {
            get => _filtroAjustes;
            set
            {
                if (SetProperty(ref _filtroAjustes, value) && value)
                {
                    ResetOtherFilters("Ajustes");
                    RefreshCommand.Execute(null);
                }
            }
        }

        // Propiedades para estadísticas
        public int TotalTransacciones
        {
            get => _totalTransacciones;
            set => SetProperty(ref _totalTransacciones, value);
        }

        public int TransaccionesSemana
        {
            get => _transaccionesSemana;
            set => SetProperty(ref _transaccionesSemana, value);
        }

        public decimal ValorTotal
        {
            get => _valorTotal;
            set => SetProperty(ref _valorTotal, value);
        }

        // Colección de transacciones
        public ObservableCollection<Transaccion> Transacciones
        {
            get => _transacciones;
            set => SetProperty(ref _transacciones, value);
        }

        #endregion

        #region Comandos
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand AddTransaccionCommand { get; }
        public ICommand TransaccionDetailCommand { get; }
        #endregion

        // Constructor
        public TransaccionesViewModel()
        {
            Title = "Transacciones";

            // Inicializar colecciones
            _transacciones = new ObservableCollection<Transaccion>();

            // Inicializar comandos
            SearchCommand = new Command(OnSearch);
            RefreshCommand = new Command(OnRefresh);
            LoadMoreCommand = new Command(OnLoadMore);
            AddTransaccionCommand = new Command(OnAddTransaccion);
            TransaccionDetailCommand = new Command<Transaccion>(OnTransaccionDetail);

            // Iniciar la carga asíncrona
            InitializeAsync();
        }

        #region Métodos privados

        private void ResetOtherFilters(string activeFilter)
        {
            // Desactivar todos los filtros excepto el activo
            switch (activeFilter)
            {
                case "Todas":
                    _filtroEntradas = false;
                    _filtroSalidas = false;
                    _filtroAjustes = false;
                    break;
                case "Entradas":
                    _filtroTodas = false;
                    _filtroSalidas = false;
                    _filtroAjustes = false;
                    break;
                case "Salidas":
                    _filtroTodas = false;
                    _filtroEntradas = false;
                    _filtroAjustes = false;
                    break;
                case "Ajustes":
                    _filtroTodas = false;
                    _filtroEntradas = false;
                    _filtroSalidas = false;
                    break;
            }

            // Notificar cambios a la UI
            OnPropertyChanged(nameof(FiltroTodas));
            OnPropertyChanged(nameof(FiltroEntradas));
            OnPropertyChanged(nameof(FiltroSalidas));
            OnPropertyChanged(nameof(FiltroAjustes));
        }

        private async void InitializeAsync()
        {
            // Indicar que estamos en estado de inicialización
            IsInitializing = true;

            try
            {
                // Secuencia de estados de carga para una mejor experiencia visual
                await Task.Delay(300);
                LoadingMessage = "Cargando transacciones...";
                await Task.Delay(300);

                // Obtener datos para estadísticas
                var todasTransacciones = await TransaccionJson.ObtenerTransacciones();
                TotalTransacciones = todasTransacciones.Count;

                // Calcular transacciones de los últimos 7 días
                var fechaLimite = DateTime.Now.AddDays(-7);
                TransaccionesSemana = todasTransacciones.Count(t => t.Fecha >= fechaLimite);

                // Calcular valor total de transacciones
                ValorTotal = todasTransacciones.Sum(t => t.ValorTotal);

                LoadingMessage = "Preparando listado...";
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

                Debug.WriteLine($"[TransaccionesViewModel] Error en InitializeAsync: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Ocurrió un error al inicializar la página de transacciones: " + ex.Message,
                    "OK");
            }
            finally
            {
                // Completar la inicialización
                IsInitializing = false;
            }
        }

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
                    Transacciones.Clear();
                }

                // Obtener datos del JSON
                var todasTransacciones = await TransaccionJson.ObtenerTransacciones();

                // Aplicar filtros
                var transaccionesFiltradas = todasTransacciones;

                // Aplicar filtro de búsqueda si hay texto
                if (!string.IsNullOrWhiteSpace(_searchQuery))
                {
                    string query = _searchQuery.ToLower();
                    transaccionesFiltradas = transaccionesFiltradas.Where(t =>
                        (t.RepuestoNombre?.ToLower().Contains(query) ?? false) ||
                        (t.RepuestoCodigo?.ToLower().Contains(query) ?? false) ||
                        (t.VehiculoPlaca?.ToLower().Contains(query) ?? false) ||
                        (t.Id.ToString().Contains(query))).ToList();
                }

                // Aplicar filtro por tipo de transacción
                if (FiltroEntradas)
                {
                    transaccionesFiltradas = transaccionesFiltradas.Where(t => t.Tipo == TipoTransaccion.Entrada).ToList();
                }
                else if (FiltroSalidas)
                {
                    transaccionesFiltradas = transaccionesFiltradas.Where(t => t.Tipo == TipoTransaccion.Salida).ToList();
                }
                else if (FiltroAjustes)
                {
                    transaccionesFiltradas = transaccionesFiltradas.Where(t => t.Tipo == TipoTransaccion.Ajuste).ToList();
                }

                // Ordenar por fecha descendente (más recientes primero)
                transaccionesFiltradas = transaccionesFiltradas.OrderByDescending(t => t.Fecha).ToList();

                // Aplicar paginación
                var nuevasTransacciones = transaccionesFiltradas
                    .Skip((_currentPage - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToList();

                // Verificar si hay más datos
                _hasMoreItems = nuevasTransacciones.Count == _pageSize &&
                                ((_currentPage * _pageSize) < transaccionesFiltradas.Count);

                // Añadir los elementos a la colección
                foreach (var transaccion in nuevasTransacciones)
                {
                    Transacciones.Add(transaccion);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TransaccionesViewModel] Error al cargar datos: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudieron cargar las transacciones: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region Comandos handlers

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

        private async void OnAddTransaccion()
        {
            try
            {
                // Navegar a la página para registrar una nueva transacción
                Debug.WriteLine("[TransaccionesViewModel] Navegando a NuevaTransaccionPage");
                await Shell.Current.GoToAsync(nameof(NuevaTransaccionPage));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TransaccionesViewModel] Error al navegar a NuevaTransaccionPage: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error de Navegación",
                    $"No se pudo abrir la página de nueva transacción: {ex.Message}",
                    "OK");
            }
        }

        private async void OnTransaccionDetail(Transaccion transaccion)
        {
            if (transaccion == null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "No se seleccionó ninguna transacción",
                    "OK");
                return;
            }

            // En una implementación real, navegaríamos a la página de detalles
            // Por ahora, mostraremos un mensaje con los detalles básicos
            await Application.Current.MainPage.DisplayAlert(
                $"Detalles de Transacción #{transaccion.Id}",
                $"Tipo: {transaccion.Tipo}\n" +
                $"Fecha: {transaccion.Fecha:dd/MM/yyyy HH:mm}\n" +
                $"Repuesto: {transaccion.RepuestoNombre} ({transaccion.RepuestoCodigo})\n" +
                $"Cantidad: {transaccion.Cantidad}\n" +
                $"Valor Total: ${transaccion.ValorTotal:N2}\n" +
                (string.IsNullOrEmpty(transaccion.VehiculoPlaca) ? "" : $"Vehículo: {transaccion.VehiculoPlaca}\n") +
                (string.IsNullOrEmpty(transaccion.Observaciones) ? "" : $"Observaciones: {transaccion.Observaciones}"),
                "OK");

            // En una implementación real:
            // var navigationParameter = new Dictionary<string, object> { { "TransaccionId", transaccion.Id } };
            // await Shell.Current.GoToAsync($"{nameof(TransaccionDetallePage)}", navigationParameter);
        }

        #endregion
    }
}
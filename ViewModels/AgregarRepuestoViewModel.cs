using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AppInventariCor.Models;
using AppInventariCor.Services;
using System.Linq;

namespace AppInventariCor.ViewModels
{
    [QueryProperty(nameof(RepuestoId), "RepuestoId")]
    [QueryProperty(nameof(IsEditMode), "IsEditMode")]
    public class AgregarRepuestoViewModel : BaseViewModel
    {
        private int _repuestoId;
        private bool _isEditMode;
        private Repuesto _repuesto;

        // Propiedades de UI
        private string _pageTitle = "Nuevo Repuesto";
        private string _actionButtonText = "Guardar Repuesto";

        // Propiedades para binding a los campos
        private string _codigo;
        private string _nombre;
        private string _descripcion;
        private string _categoria;
        private string _marca;
        private string _modelo;
        private string _ubicacion;
        private decimal _precio;
        private int _cantidad;
        private int _stockMinimo;
        private int _stockOptimo;
        private string _codigoBarras;
        private string _codigoQR;

        // Propiedades de navegación
        public int RepuestoId
        {
            get => _repuestoId;
            set
            {
                _repuestoId = value;
                CheckAndLoadRepuesto();
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
                    PageTitle = value ? "Editar Repuesto" : "Nuevo Repuesto";
                    ActionButtonText = value ? "Actualizar Repuesto" : "Guardar Repuesto";

                    // Intentar cargar el repuesto si ya tenemos el ID
                    CheckAndLoadRepuesto();
                }
            }
        }

        // Método auxiliar para verificar si podemos cargar el repuesto
        private void CheckAndLoadRepuesto()
        {
            // Cargar el repuesto si tenemos el ID y estamos en modo edición
            if (_isEditMode && _repuestoId > 0)
            {
                LoadRepuestoAsync(_repuestoId);
                Debug.WriteLine($"[AgregarRepuestoViewModel] Cargando repuesto ID: {_repuestoId} en modo edición");
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
        public string Codigo
        {
            get => _codigo;
            set => SetProperty(ref _codigo, value);
        }

        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        public string Categoria
        {
            get => _categoria;
            set => SetProperty(ref _categoria, value);
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

        public string Ubicacion
        {
            get => _ubicacion;
            set => SetProperty(ref _ubicacion, value);
        }

        public decimal Precio
        {
            get => _precio;
            set => SetProperty(ref _precio, value);
        }

        public int Cantidad
        {
            get => _cantidad;
            set => SetProperty(ref _cantidad, value);
        }

        public int StockMinimo
        {
            get => _stockMinimo;
            set => SetProperty(ref _stockMinimo, value);
        }

        public int StockOptimo
        {
            get => _stockOptimo;
            set => SetProperty(ref _stockOptimo, value);
        }

        public string CodigoBarras
        {
            get => _codigoBarras;
            set => SetProperty(ref _codigoBarras, value);
        }

        public string CodigoQR
        {
            get => _codigoQR;
            set => SetProperty(ref _codigoQR, value);
        }

        // Constructor
        public AgregarRepuestoViewModel()
        {
            Title = "Agregar/Editar Repuesto";
        }

        // Método para cargar un repuesto existente
        private async void LoadRepuestoAsync(int repuestoId)
        {
            try
            {
                if (repuestoId <= 0)
                {
                    Debug.WriteLine("[AgregarRepuestoViewModel] ID de repuesto inválido");
                    return;
                }

                Debug.WriteLine($"[AgregarRepuestoViewModel] Intentando cargar repuesto ID: {repuestoId}");
                IsBusy = true;

                // Cargar todos los repuestos desde JSON
                var repuestos = await RepuestoJson.ObtenerRepuestos();

                // Buscar el repuesto específico por ID
                var repuesto = repuestos.FirstOrDefault(r => r.Id == repuestoId);

                if (repuesto != null)
                {
                    _repuesto = repuesto;

                    // Llenar las propiedades del formulario con un pequeño retraso 
                    // para asegurar que la UI esté lista
                    await Task.Delay(100);

                    // Asignar cada propiedad y registrar para depuración
                    Codigo = repuesto.Codigo;
                    Debug.WriteLine($"[AgregarRepuestoViewModel] Código asignado: {Codigo}");

                    Nombre = repuesto.Nombre;
                    Descripcion = repuesto.Descripcion;
                    Categoria = repuesto.Categoria;
                    Marca = repuesto.Marca;
                    Modelo = repuesto.Modelo;
                    Ubicacion = repuesto.Ubicacion;
                    Precio = repuesto.Precio;
                    Cantidad = repuesto.Cantidad;
                    StockMinimo = repuesto.StockMinimo;
                    StockOptimo = repuesto.StockOptimo;
                    CodigoBarras = repuesto.CodigoBarras;
                    CodigoQR = repuesto.CodigoQR;

                    Debug.WriteLine($"[AgregarRepuestoViewModel] Repuesto cargado completamente: {repuesto.Nombre}, ID: {repuesto.Id}, Código: {repuesto.Codigo}");

                    // Forzar actualización de todas las propiedades UI
                    OnPropertyChanged(string.Empty);
                }
                else
                {
                    Debug.WriteLine($"[AgregarRepuestoViewModel] No se encontró repuesto con ID: {repuestoId}");
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se encontró el repuesto solicitado.",
                        "OK");

                    // Volver a la página anterior
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AgregarRepuestoViewModel] Error al cargar repuesto: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Ocurrió un error al cargar los datos: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Método para guardar o actualizar un repuesto
        public async Task<bool> SaveRepuestoAsync()
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(Codigo) || string.IsNullOrWhiteSpace(Nombre))
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Validación",
                        "Los campos Código y Nombre son obligatorios.",
                        "OK");
                    return false;
                }

                // Crear o actualizar el objeto repuesto
                if (IsEditMode && _repuesto != null)
                {
                    // Actualizar el repuesto existente
                    _repuesto.Codigo = Codigo;
                    _repuesto.Nombre = Nombre;
                    _repuesto.Descripcion = Descripcion;
                    _repuesto.Categoria = Categoria;
                    _repuesto.Marca = Marca;
                    _repuesto.Modelo = Modelo;
                    _repuesto.Ubicacion = Ubicacion;
                    _repuesto.Precio = Precio;
                    // No actualizamos Cantidad directamente para mantener la integridad del sistema de transacciones
                    _repuesto.StockMinimo = StockMinimo;
                    _repuesto.StockOptimo = StockOptimo;
                    _repuesto.CodigoBarras = CodigoBarras;
                    _repuesto.CodigoQR = CodigoQR;

                    // Guardar los cambios
                    bool resultado = await RepuestoJson.ActualizarRepuesto(_repuesto);

                    if (resultado)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Éxito",
                            "Repuesto actualizado correctamente.",
                            "OK");
                        return true;
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Error",
                            "No se pudo actualizar el repuesto.",
                            "OK");
                        return false;
                    }
                }
                else
                {
                    // Crear un nuevo repuesto
                    var nuevoRepuesto = new Repuesto
                    {
                        Codigo = Codigo,
                        Nombre = Nombre,
                        Descripcion = Descripcion,
                        Categoria = Categoria,
                        Marca = Marca,
                        Modelo = Modelo,
                        Ubicacion = Ubicacion,
                        Precio = Precio,
                        Cantidad = Cantidad,
                        StockMinimo = StockMinimo,
                        StockOptimo = StockOptimo,
                        CodigoBarras = CodigoBarras,
                        CodigoQR = CodigoQR
                    };

                    // Guardar el nuevo repuesto
                    bool resultado = await RepuestoJson.AgregarRepuesto(nuevoRepuesto);

                    if (resultado)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Éxito",
                            "Repuesto agregado correctamente.",
                            "OK");
                        return true;
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Error",
                            "No se pudo agregar el repuesto. El código podría estar duplicado.",
                            "OK");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AgregarRepuestoViewModel] Error al guardar repuesto: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Ocurrió un error al guardar los datos: {ex.Message}",
                    "OK");
                return false;
            }
        }
    }
}
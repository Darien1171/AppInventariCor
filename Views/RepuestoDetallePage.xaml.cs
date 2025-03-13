using AppInventariCor.Models;
using AppInventariCor.ViewModels;

namespace AppInventariCor.Views
{
    [QueryProperty(nameof(RepuestoId), "id")]
    public partial class RepuestoDetallePage : ContentPage
    {
        private RepuestoDetalleViewModel _viewModel;
        private int _repuestoId;

        public int RepuestoId
        {
            get => _repuestoId;
            set
            {
                _repuestoId = value;
                LoadRepuesto(_repuestoId);
            }
        }

        public RepuestoDetallePage()
        {
            InitializeComponent();
            _viewModel = new RepuestoDetalleViewModel();
            BindingContext = _viewModel;
        }

        private void LoadRepuesto(int repuestoId)
        {
            // En una implementación real, aquí cargaríamos el repuesto de la base de datos
            // Para este ejemplo, usaremos datos de muestra

            // Simulamos la carga del repuesto
            Repuesto repuesto = GetSampleRepuesto(repuestoId);

            if (repuesto != null)
            {
                _viewModel.Repuesto = repuesto;
            }
            else
            {
                DisplayAlert("Error", "No se pudo encontrar el repuesto", "OK");
                Shell.Current.GoToAsync("..");
            }
        }

        // Método para obtener un repuesto de muestra por ID
        // En una implementación real, esto se obtendría de una base de datos
        private Repuesto GetSampleRepuesto(int id)
        {
            // Lista de repuestos de muestra
            var repuestos = new List<Repuesto>
            {
                new Repuesto
                {
                    Id = 1,
                    Codigo = "FLT-5678",
                    Nombre = "Filtro de aceite XYZ-123",
                    Descripcion = "Filtro de aceite de alta calidad para motores de 4 cilindros. Compatible con la mayoría de vehículos modernos. Incluye sello de goma reemplazable.",
                    Categoria = "Filtros",
                    Marca = "FilterPro",
                    Modelo = "XYZ-123",
                    Precio = 45.99m,
                    Cantidad = 12,
                    StockMinimo = 5,
                    StockOptimo = 25,
                    Ubicacion = "Estante A-32",
                    CodigoBarras = "7501234567890",
                    CodigoQR = "https://inventory.example.com/parts/FLT-5678"
                },
                new Repuesto
                {
                    Id = 2,
                    Codigo = "BDJ-1234",
                    Nombre = "Bombilla delantera LED",
                    Descripcion = "Bombilla LED de alta luminosidad para faros delanteros. Bajo consumo y larga duración. Incluye adaptadores universales.",
                    Categoria = "Iluminación",
                    Marca = "LightMaster",
                    Modelo = "LD-100",
                    Precio = 28.50m,
                    Cantidad = 8,
                    StockMinimo = 10,
                    StockOptimo = 30,
                    Ubicacion = "Estante B-15",
                    CodigoBarras = "7509876543210",
                    CodigoQR = "https://inventory.example.com/parts/BDJ-1234"
                },
                new Repuesto
                {
                    Id = 3,
                    Codigo = "ACE-7890",
                    Nombre = "Aceite de motor sintético 10W-40",
                    Descripcion = "Aceite sintético para motor de alto rendimiento. Formulado para temperaturas extremas y condiciones de conducción exigentes. Protección contra el desgaste y la corrosión.",
                    Categoria = "Lubricantes",
                    Marca = "OilTech",
                    Modelo = "SYN-1040",
                    Precio = 35.75m,
                    Cantidad = 20,
                    StockMinimo = 15,
                    StockOptimo = 40,
                    Ubicacion = "Estante C-08",
                    CodigoBarras = "7507654321098",
                    CodigoQR = "https://inventory.example.com/parts/ACE-7890"
                },
                new Repuesto
                {
                    Id = 4,
                    Codigo = "FRN-4567",
                    Nombre = "Pastillas de freno delanteras",
                    Descripcion = "Pastillas de freno de cerámica para eje delantero. Excelente potencia de frenado con baja generación de polvo. Incluye hardware de instalación.",
                    Categoria = "Frenos",
                    Marca = "BrakeSafe",
                    Modelo = "BS-200D",
                    Precio = 65.25m,
                    Cantidad = 4,
                    StockMinimo = 8,
                    StockOptimo = 16,
                    Ubicacion = "Estante D-22",
                    CodigoBarras = "7503456789012",
                    CodigoQR = "https://inventory.example.com/parts/FRN-4567"
                }
            };

            return repuestos.FirstOrDefault(r => r.Id == id);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Código adicional al aparecer la página si es necesario
        }
    }
}
using AppInventariCor.ViewModels;

namespace AppInventariCor.Views
{
    public partial class InventarioPage : ContentPage
    {
        private InventarioViewModel _viewModel;

        public InventarioPage()
        {
            InitializeComponent();
            _viewModel = new InventarioViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Habilitar aqu� cualquier l�gica necesaria al aparecer la p�gina
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Limpiar recursos o detener procesos aqu� si es necesario
        }
    }
}
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

            // Actualizar la lista de repuestos cada vez que la página se vuelve a mostrar
            if (_viewModel != null)
            {
                _viewModel.RefreshCommand.Execute(null);
            }
        }
    }
}
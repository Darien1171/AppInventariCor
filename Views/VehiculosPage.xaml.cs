using AppInventariCor.ViewModels;

namespace AppInventariCor.Views
{
    public partial class VehiculosPage : ContentPage
    {
        private VehiculosViewModel viewModel;

        public VehiculosPage()
        {
            InitializeComponent();

            // Crear e inicializar el ViewModel
            viewModel = new VehiculosViewModel();
            BindingContext = viewModel;
        }

        // Este m�todo se ejecuta cada vez que la p�gina aparece en pantalla
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Recargar los datos cuando la p�gina vuelve a aparecer
            if (viewModel != null)
            {
                viewModel.RefreshCommand.Execute(null);
            }
        }
    }
}
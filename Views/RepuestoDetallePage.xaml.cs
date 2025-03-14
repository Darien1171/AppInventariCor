using AppInventariCor.ViewModels;

namespace AppInventariCor.Views
{
    public partial class RepuestoDetallePage : ContentPage
    {
        RepuestoDetalleViewModel viewModel;

        public RepuestoDetallePage()
        {
            InitializeComponent();
            viewModel = new RepuestoDetalleViewModel();
            BindingContext = viewModel;
        }
    }
}
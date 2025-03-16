using AppInventariCor.ViewModels;

namespace AppInventariCor.Views
{
    public partial class TransaccionesPage : ContentPage
    {
        private TransaccionesViewModel viewModel;

        public TransaccionesPage()
        {
            InitializeComponent();
            viewModel = new TransaccionesViewModel();
            BindingContext = viewModel;
        }
    }
}
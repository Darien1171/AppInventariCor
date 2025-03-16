using AppInventariCor.ViewModels;

namespace AppInventariCor.Views
{
    public partial class NuevaTransaccionPage : ContentPage
    {
        private NuevaTransaccionViewModel viewModel;

        public NuevaTransaccionPage()
        {
            InitializeComponent();
            viewModel = new NuevaTransaccionViewModel();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Código adicional que se ejecuta cuando la página aparece
        }

        protected override bool OnBackButtonPressed()
        {
            // Mostrar alerta de confirmación si el usuario intenta salir durante el proceso
            if (viewModel.CurrentStep > 1)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    bool respuesta = await DisplayAlert(
                        "Confirmar",
                        "¿Estás seguro que deseas cancelar la transacción actual?",
                        "Sí, cancelar",
                        "No, continuar");

                    if (respuesta)
                    {
                        await Navigation.PopAsync();
                    }
                });
                return true;
            }

            return base.OnBackButtonPressed();
        }
    }
}
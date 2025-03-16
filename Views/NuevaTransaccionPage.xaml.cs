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
            // C�digo adicional que se ejecuta cuando la p�gina aparece
        }

        protected override bool OnBackButtonPressed()
        {
            // Mostrar alerta de confirmaci�n si el usuario intenta salir durante el proceso
            if (viewModel.CurrentStep > 1)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    bool respuesta = await DisplayAlert(
                        "Confirmar",
                        "�Est�s seguro que deseas cancelar la transacci�n actual?",
                        "S�, cancelar",
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
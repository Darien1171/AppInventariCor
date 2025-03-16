using AppInventariCor.ViewModels;
using System;
using System.Diagnostics;

namespace AppInventariCor.Views
{
    public partial class AgregarVehiculoPage : ContentPage
    {
        private AgregarVehiculoViewModel viewModel;

        public AgregarVehiculoPage()
        {
            try
            {
                // Primero inicializa los componentes XAML
                InitializeComponent();
                Debug.WriteLine("[AgregarVehiculoPage] InitializeComponent completado");

                // Luego crea e inicializa el ViewModel por separado
                viewModel = new AgregarVehiculoViewModel();
                BindingContext = viewModel;
                Debug.WriteLine("[AgregarVehiculoPage] ViewModel asignado");

                // No vincular campos aqu�, dejarlo para OnAppearing
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AgregarVehiculoPage] ERROR EN CONSTRUCTOR: {ex.Message}");
                Debug.WriteLine($"[AgregarVehiculoPage] Stack trace: {ex.StackTrace}");

                // Proporcionar un ViewModel m�nimo para evitar errores de nulidad
                if (viewModel == null)
                {
                    viewModel = new AgregarVehiculoViewModel();
                    try { BindingContext = viewModel; } catch { /* ignorar */ }
                }
            }
        }

        // Manejador del evento para el bot�n Guardar
        private async void btnGuardar_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (viewModel == null)
                {
                    Debug.WriteLine("[AgregarVehiculoPage] Error: ViewModel es null en btnGuardar_Clicked");
                    await DisplayAlert("Error", "No se puede procesar la solicitud", "OK");
                    return;
                }

                // Transferir datos de los campos al ViewModel
                viewModel.NumeroPlaca = txtNumeroPlaca.Text;
                viewModel.NumeroInterno = txtNumeroInterno.Text;
                viewModel.Marca = txtMarca.Text;
                viewModel.Modelo = txtModelo.Text;
                viewModel.NumeroSerie = txtNumeroSerie.Text;
                viewModel.NumeroMotor = txtNumeroMotor.Text;

                // Convertir el a�o si no est� vac�o
                if (!string.IsNullOrEmpty(txtAnio.Text) && int.TryParse(txtAnio.Text, out int anio))
                {
                    viewModel.Anio = anio;
                }
                else
                {
                    viewModel.Anio = null;
                }

                viewModel.Color = txtColor.Text;
                viewModel.Propietario = txtPropietario.Text;

                // Intentar guardar
                bool resultado = await viewModel.SaveVehiculoAsync();

                // Si se guard� correctamente, regresar a la p�gina anterior
                if (resultado)
                {
                    try
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                    catch (Exception navEx)
                    {
                        Debug.WriteLine($"[AgregarVehiculoPage] Error de navegaci�n despu�s de guardar: {navEx.Message}");
                        // Intentar otra forma de navegaci�n si Shell falla
                        await Navigation.PopAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AgregarVehiculoPage] Error en btnGuardar_Clicked: {ex.Message}");
                Debug.WriteLine($"[AgregarVehiculoPage] Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", "Ocurri� un error al procesar la solicitud", "OK");
            }
        }

        // Manejador del evento para el bot�n Cancelar
        private async void btnCancelar_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Regresar a la p�gina anterior sin guardar
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AgregarVehiculoPage] Error en btnCancelar_Clicked: {ex.Message}");
                // Intentar otra forma de navegaci�n si Shell falla
                try
                {
                    await Navigation.PopAsync();
                }
                catch (Exception navEx)
                {
                    Debug.WriteLine($"[AgregarVehiculoPage] Error de navegaci�n alternativa: {navEx.Message}");
                    // �ltimo recurso: mostrar mensaje de error
                    await DisplayAlert("Error", "No se pudo regresar a la p�gina anterior", "OK");
                }
            }
        }

        // Este m�todo se ejecuta cuando la p�gina aparece
        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                Debug.WriteLine("[AgregarVehiculoPage] OnAppearing iniciado");

                // Verificar que el ViewModel existe
                if (viewModel == null)
                {
                    Debug.WriteLine("[AgregarVehiculoPage] ViewModel es null en OnAppearing, creando nuevo");
                    viewModel = new AgregarVehiculoViewModel();
                    BindingContext = viewModel;
                }

                // Asignar valores de ViewModel a los controles
                // Esto es importante para el modo de edici�n
                txtNumeroPlaca.Text = viewModel.NumeroPlaca;
                txtNumeroInterno.Text = viewModel.NumeroInterno;
                txtMarca.Text = viewModel.Marca;
                txtModelo.Text = viewModel.Modelo;
                txtNumeroSerie.Text = viewModel.NumeroSerie;
                txtNumeroMotor.Text = viewModel.NumeroMotor;
                txtAnio.Text = viewModel.Anio?.ToString();
                txtColor.Text = viewModel.Color;
                txtPropietario.Text = viewModel.Propietario;

                Debug.WriteLine("[AgregarVehiculoPage] OnAppearing completado");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AgregarVehiculoPage] Error en OnAppearing: {ex.Message}");
                // No lanzar excepciones para evitar bloquear la UI
            }
        }

        protected override void OnDisappearing()
        {
            try
            {
                base.OnDisappearing();
                Debug.WriteLine("[AgregarVehiculoPage] OnDisappearing");
                // C�digo de limpieza si es necesario
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AgregarVehiculoPage] Error en OnDisappearing: {ex.Message}");
            }
        }
    }
}
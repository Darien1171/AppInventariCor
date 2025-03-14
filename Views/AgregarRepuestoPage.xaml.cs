using System;
using AppInventariCor.Models;
using AppInventariCor.Services;

namespace AppInventariCor.Views
{
    public partial class AgregarRepuestoPage : ContentPage
    {
        public AgregarRepuestoPage()
        {
            InitializeComponent();
        }

        private async void btnGuardar_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                await DisplayAlert("Campos requeridos", "Código y Nombre son campos obligatorios", "OK");
                return;
            }

            // Mostrar indicador de carga
            IsBusy = true;

            try
            {
                Repuesto nuevoRepuesto = new Repuesto
                {
                    Codigo = txtCodigo.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Descripcion = txtDescripcion.Text?.Trim() ?? string.Empty,
                    Categoria = txtCategoria.Text?.Trim() ?? string.Empty,
                    Marca = txtMarca.Text?.Trim() ?? string.Empty,
                    Modelo = txtModelo.Text?.Trim() ?? string.Empty,
                    Ubicacion = txtUbicacion.Text?.Trim() ?? string.Empty,
                    CodigoBarras = txtCodigoBarras.Text?.Trim(),
                    CodigoQR = txtCodigoQR.Text?.Trim()
                };

                // Conversión de valores numéricos con manejo de errores
                if (decimal.TryParse(txtPrecio.Text, out decimal precio))
                {
                    nuevoRepuesto.Precio = precio;
                }
                else
                {
                    nuevoRepuesto.Precio = 0;
                }

                if (int.TryParse(txtCantidad.Text, out int cantidad))
                {
                    nuevoRepuesto.Cantidad = cantidad;
                }
                else
                {
                    nuevoRepuesto.Cantidad = 0;
                }

                if (int.TryParse(txtStockMinimo.Text, out int stockMinimo))
                {
                    nuevoRepuesto.StockMinimo = stockMinimo;
                }
                else
                {
                    nuevoRepuesto.StockMinimo = 0;
                }

                if (int.TryParse(txtStockOptimo.Text, out int stockOptimo))
                {
                    nuevoRepuesto.StockOptimo = stockOptimo;
                }
                else
                {
                    nuevoRepuesto.StockOptimo = 0;
                }

                // Guardar el repuesto usando el servicio JSON
                bool resultado = await RepuestoJson.AgregarRepuesto(nuevoRepuesto);

                if (resultado)
                {
                    await DisplayAlert("Éxito", "Repuesto guardado correctamente", "OK");

                    // Limpiar formulario
                    LimpiarFormulario();

                    // Volver a la página anterior (opcional - depende del flujo de la app)
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo guardar el repuesto. El código podría estar duplicado.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LimpiarFormulario()
        {
            txtCodigo.Text = string.Empty;
            txtNombre.Text = string.Empty;
            txtDescripcion.Text = string.Empty;
            txtCategoria.Text = string.Empty;
            txtMarca.Text = string.Empty;
            txtModelo.Text = string.Empty;
            txtUbicacion.Text = string.Empty;
            txtPrecio.Text = string.Empty;
            txtCantidad.Text = string.Empty;
            txtStockMinimo.Text = string.Empty;
            txtStockOptimo.Text = string.Empty;
            txtCodigoBarras.Text = string.Empty;
            txtCodigoQR.Text = string.Empty;
        }

        private async void btnCancelar_Clicked(object sender, EventArgs e)
        {
            bool abandonar = await DisplayAlert("Confirmar", "¿Desea abandonar el formulario? Los datos no guardados se perderán.", "Sí", "No");

            if (abandonar)
            {
                await Navigation.PopAsync();
            }
        }
    }
}
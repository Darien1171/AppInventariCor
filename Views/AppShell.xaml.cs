using AppInventariCor.Views;

namespace AppInventariCor
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas para navegación - asegúrate de que el nombre coincida exactamente con el de la clase
            Routing.RegisterRoute(nameof(RepuestoDetallePage), typeof(RepuestoDetallePage));
        }
    }
}
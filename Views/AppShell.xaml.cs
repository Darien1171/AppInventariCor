using AppInventariCor.Views;

namespace AppInventariCor
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas para navegación
            Routing.RegisterRoute(nameof(Views.RepuestoDetallePage), typeof(Views.RepuestoDetallePage));
        }
    }
}
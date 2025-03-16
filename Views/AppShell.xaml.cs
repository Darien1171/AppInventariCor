using AppInventariCor.Views;

namespace AppInventariCor
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas para navegación - asegúrate de que el nombre coincida exactamente con el de la clase

            // Rutas para Repuestos
            Routing.RegisterRoute(nameof(RepuestoDetallePage), typeof(RepuestoDetallePage));
            Routing.RegisterRoute(nameof(AgregarRepuestoPage), typeof(AgregarRepuestoPage));

            // Rutas para Vehículos
            Routing.RegisterRoute(nameof(VehiculosPage), typeof(VehiculosPage));
            Routing.RegisterRoute(nameof(VehiculoDetallePage), typeof(VehiculoDetallePage));
            Routing.RegisterRoute(nameof(AgregarVehiculoPage), typeof(AgregarVehiculoPage));

            // Rutas para Transacciones
            Routing.RegisterRoute(nameof(TransaccionesPage), typeof(TransaccionesPage));
            Routing.RegisterRoute(nameof(NuevaTransaccionPage), typeof(NuevaTransaccionPage));
        }
    }
}
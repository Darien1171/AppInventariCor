using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using AppInventariCor.Models;

namespace AppInventariCor.Services
{
    public class VehiculoJson
    {
        private static string GetFilePath()
        {
            string directorio = "AppInventariCor";
            string ruta = "";

            try
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), directorio);
                    Debug.WriteLine($"[VehiculoJson] Ruta Android: {ruta}");
                }
                else if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), directorio);
                    Debug.WriteLine($"[VehiculoJson] Ruta Windows: {ruta}");
                }
                else // iOS, macOS u otras plataformas
                {
                    ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), directorio);
                    Debug.WriteLine($"[VehiculoJson] Ruta iOS/Otras: {ruta}");
                }

                if (!Directory.Exists(ruta))
                {
                    Debug.WriteLine($"[VehiculoJson] Creando directorio: {ruta}");
                    Directory.CreateDirectory(ruta);
                }
                else
                {
                    Debug.WriteLine($"[VehiculoJson] Directorio ya existe: {ruta}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VehiculoJson] ERROR en GetFilePath: {ex.Message}");
                // En caso de error, intentar usar un directorio de respaldo
                ruta = Path.Combine(FileSystem.AppDataDirectory, directorio);

                if (!Directory.Exists(ruta))
                {
                    Directory.CreateDirectory(ruta);
                }
                Debug.WriteLine($"[VehiculoJson] Usando ruta alternativa: {ruta}");
            }

            string pathCompleto = Path.Combine(ruta, "vehiculos.json");
            Debug.WriteLine($"[VehiculoJson] Ruta completa del archivo: {pathCompleto}");
            return pathCompleto;
        }

        public static async Task GuardarVehiculos(List<Vehiculo> vehiculos)
        {
            try
            {
                string path = GetFilePath();
                Debug.WriteLine($"[VehiculoJson] Guardando {vehiculos.Count} vehículos en: {path}");

                // Usar opciones para hacer el JSON más legible y asegurar compatibilidad con propiedades nulas
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                string json = JsonSerializer.Serialize(vehiculos, options);
                Debug.WriteLine($"[VehiculoJson] JSON serializado: {json.Substring(0, Math.Min(100, json.Length))}..."); // Muestra solo los primeros 100 caracteres

                await File.WriteAllTextAsync(path, json);
                Debug.WriteLine($"[VehiculoJson] Archivo guardado correctamente");

                // Verificar que el archivo existe después de guardarlo
                if (File.Exists(path))
                {
                    long fileSize = new FileInfo(path).Length;
                    Debug.WriteLine($"[VehiculoJson] Verificación: Archivo existe con tamaño: {fileSize} bytes");
                }
                else
                {
                    Debug.WriteLine($"[VehiculoJson] ADVERTENCIA: El archivo no existe después de guardarlo");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VehiculoJson] ERROR guardando vehículos: {ex.Message}");
                Debug.WriteLine($"[VehiculoJson] Stack trace: {ex.StackTrace}");
                throw; // Re-lanzar la excepción para que se maneje en la capa superior
            }
        }

        public static async Task<List<Vehiculo>> ObtenerVehiculos()
        {
            try
            {
                string path = GetFilePath();
                Debug.WriteLine($"[VehiculoJson] Intentando leer vehículos de: {path}");

                if (!File.Exists(path))
                {
                    Debug.WriteLine($"[VehiculoJson] El archivo no existe, devolviendo lista vacía");
                    return new List<Vehiculo>();
                }

                string json = await File.ReadAllTextAsync(path);
                Debug.WriteLine($"[VehiculoJson] Contenido leído: {json.Substring(0, Math.Min(100, json.Length))}..."); // Muestra solo los primeros 100 caracteres

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Para mayor flexibilidad en la deserialización
                };

                var vehiculos = JsonSerializer.Deserialize<List<Vehiculo>>(json, options);

                if (vehiculos == null)
                {
                    Debug.WriteLine($"[VehiculoJson] La deserialización devolvió null, creando lista vacía");
                    return new List<Vehiculo>();
                }

                Debug.WriteLine($"[VehiculoJson] Se cargaron {vehiculos.Count} vehículos");
                return vehiculos;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VehiculoJson] ERROR obteniendo vehículos: {ex.Message}");
                Debug.WriteLine($"[VehiculoJson] Stack trace: {ex.StackTrace}");

                // En caso de error, devolver una lista vacía para evitar que la app se cierre
                return new List<Vehiculo>();
            }
        }

        public static async Task<bool> AgregarVehiculo(Vehiculo nuevoVehiculo)
        {
            try
            {
                Debug.WriteLine($"[VehiculoJson] Iniciando proceso para agregar vehículo: {nuevoVehiculo.NumeroPlaca} - {nuevoVehiculo.NumeroInterno}");
                var vehiculos = await ObtenerVehiculos();

                // Comprobar si ya existe un vehículo con la misma placa
                if (vehiculos.Any(v => v.NumeroPlaca == nuevoVehiculo.NumeroPlaca))
                {
                    Debug.WriteLine($"[VehiculoJson] Placa duplicada: {nuevoVehiculo.NumeroPlaca}");
                    return false; // Placa duplicada
                }

                // Generar un ID único
                if (vehiculos.Any())
                {
                    nuevoVehiculo.Id = vehiculos.Max(v => v.Id) + 1;
                }
                else
                {
                    nuevoVehiculo.Id = 1; // Comenzar desde 1 si es el primer vehículo
                }

                Debug.WriteLine($"[VehiculoJson] ID asignado: {nuevoVehiculo.Id}");

                vehiculos.Add(nuevoVehiculo);
                Debug.WriteLine($"[VehiculoJson] Vehículo agregado a la lista en memoria. Total: {vehiculos.Count}");

                await GuardarVehiculos(vehiculos);
                Debug.WriteLine($"[VehiculoJson] Guardado completado exitosamente");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VehiculoJson] ERROR agregando vehículo: {ex.Message}");
                Debug.WriteLine($"[VehiculoJson] Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public static async Task<bool> EliminarVehiculo(int id)
        {
            try
            {
                Debug.WriteLine($"[VehiculoJson] Intentando eliminar vehículo con ID: {id}");
                var vehiculos = await ObtenerVehiculos();
                var vehiculo = vehiculos.FirstOrDefault(v => v.Id == id);

                if (vehiculo != null)
                {
                    Debug.WriteLine($"[VehiculoJson] Vehículo encontrado: {vehiculo.NumeroPlaca} - {vehiculo.NumeroInterno}");
                    vehiculos.Remove(vehiculo);
                    await GuardarVehiculos(vehiculos);
                    Debug.WriteLine($"[VehiculoJson] Vehículo eliminado exitosamente");
                    return true;
                }

                Debug.WriteLine($"[VehiculoJson] No se encontró vehículo con ID: {id}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VehiculoJson] ERROR eliminando vehículo: {ex.Message}");
                Debug.WriteLine($"[VehiculoJson] Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public static async Task<bool> ActualizarVehiculo(Vehiculo vehiculoActualizado)
        {
            try
            {
                Debug.WriteLine($"[VehiculoJson] Actualizando vehículo ID: {vehiculoActualizado.Id}");
                var vehiculos = await ObtenerVehiculos();
                var index = vehiculos.FindIndex(v => v.Id == vehiculoActualizado.Id);

                if (index >= 0)
                {
                    Debug.WriteLine($"[VehiculoJson] Vehículo encontrado en índice: {index}");
                    vehiculos[index] = vehiculoActualizado;
                    await GuardarVehiculos(vehiculos);
                    Debug.WriteLine($"[VehiculoJson] Actualización completada");
                    return true;
                }

                Debug.WriteLine($"[VehiculoJson] No se encontró vehículo para actualizar con ID: {vehiculoActualizado.Id}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VehiculoJson] ERROR actualizando vehículo: {ex.Message}");
                Debug.WriteLine($"[VehiculoJson] Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public static async Task<Vehiculo> ObtenerVehiculoPorId(int id)
        {
            try
            {
                Debug.WriteLine($"[VehiculoJson] Buscando vehículo con ID: {id}");
                var vehiculos = await ObtenerVehiculos();
                var vehiculo = vehiculos.FirstOrDefault(v => v.Id == id);

                if (vehiculo != null)
                {
                    Debug.WriteLine($"[VehiculoJson] Vehículo encontrado: {vehiculo.NumeroPlaca}");
                    return vehiculo;
                }

                Debug.WriteLine($"[VehiculoJson] No se encontró vehículo con ID: {id}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VehiculoJson] ERROR buscando vehículo: {ex.Message}");
                Debug.WriteLine($"[VehiculoJson] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public static async Task<List<Vehiculo>> ObtenerVehiculosPorMarca(string marca)
        {
            try
            {
                Debug.WriteLine($"[VehiculoJson] Buscando vehículos de marca: {marca}");
                var vehiculos = await ObtenerVehiculos();
                var filtrados = vehiculos.Where(v => v.Marca == marca).ToList();

                Debug.WriteLine($"[VehiculoJson] Se encontraron {filtrados.Count} vehículos de marca {marca}");
                return filtrados;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VehiculoJson] ERROR filtrando por marca: {ex.Message}");
                Debug.WriteLine($"[VehiculoJson] Stack trace: {ex.StackTrace}");
                return new List<Vehiculo>();
            }
        }

        public static async Task<Vehiculo> ObtenerVehiculoPorPlaca(string placa)
        {
            try
            {
                Debug.WriteLine($"[VehiculoJson] Buscando vehículo con placa: {placa}");
                var vehiculos = await ObtenerVehiculos();
                var vehiculo = vehiculos.FirstOrDefault(v => v.NumeroPlaca == placa);

                if (vehiculo != null)
                {
                    Debug.WriteLine($"[VehiculoJson] Vehículo encontrado: ID {vehiculo.Id}");
                    return vehiculo;
                }

                Debug.WriteLine($"[VehiculoJson] No se encontró vehículo con placa: {placa}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VehiculoJson] ERROR buscando por placa: {ex.Message}");
                Debug.WriteLine($"[VehiculoJson] Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
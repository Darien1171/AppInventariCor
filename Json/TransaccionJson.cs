using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using AppInventariCor.Models;

namespace AppInventariCor.Services
{
    public class TransaccionJson
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
                    Debug.WriteLine($"[TransaccionJson] Ruta Android: {ruta}");
                }
                else if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), directorio);
                    Debug.WriteLine($"[TransaccionJson] Ruta Windows: {ruta}");
                }
                else // iOS, macOS u otras plataformas
                {
                    ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), directorio);
                    Debug.WriteLine($"[TransaccionJson] Ruta iOS/Otras: {ruta}");
                }

                if (!Directory.Exists(ruta))
                {
                    Debug.WriteLine($"[TransaccionJson] Creando directorio: {ruta}");
                    Directory.CreateDirectory(ruta);
                }
                else
                {
                    Debug.WriteLine($"[TransaccionJson] Directorio ya existe: {ruta}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TransaccionJson] ERROR en GetFilePath: {ex.Message}");
                // En caso de error, intentar usar un directorio de respaldo
                ruta = Path.Combine(FileSystem.AppDataDirectory, directorio);

                if (!Directory.Exists(ruta))
                {
                    Directory.CreateDirectory(ruta);
                }
                Debug.WriteLine($"[TransaccionJson] Usando ruta alternativa: {ruta}");
            }

            string pathCompleto = Path.Combine(ruta, "transacciones.json");
            Debug.WriteLine($"[TransaccionJson] Ruta completa del archivo: {pathCompleto}");
            return pathCompleto;
        }

        public static async Task GuardarTransacciones(List<Transaccion> transacciones)
        {
            try
            {
                string path = GetFilePath();
                Debug.WriteLine($"[TransaccionJson] Guardando {transacciones.Count} transacciones en: {path}");

                // Usar opciones para hacer el JSON más legible y asegurar compatibilidad con propiedades nulas
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                string json = JsonSerializer.Serialize(transacciones, options);
                Debug.WriteLine($"[TransaccionJson] JSON serializado: {json.Substring(0, Math.Min(100, json.Length))}..."); // Muestra solo los primeros 100 caracteres

                await File.WriteAllTextAsync(path, json);
                Debug.WriteLine($"[TransaccionJson] Archivo guardado correctamente");

                // Verificar que el archivo existe después de guardarlo
                if (File.Exists(path))
                {
                    long fileSize = new FileInfo(path).Length;
                    Debug.WriteLine($"[TransaccionJson] Verificación: Archivo existe con tamaño: {fileSize} bytes");
                }
                else
                {
                    Debug.WriteLine($"[TransaccionJson] ADVERTENCIA: El archivo no existe después de guardarlo");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TransaccionJson] ERROR guardando transacciones: {ex.Message}");
                Debug.WriteLine($"[TransaccionJson] Stack trace: {ex.StackTrace}");
                throw; // Re-lanzar la excepción para que se maneje en la capa superior
            }
        }

        public static async Task<List<Transaccion>> ObtenerTransacciones()
        {
            try
            {
                string path = GetFilePath();
                Debug.WriteLine($"[TransaccionJson] Intentando leer transacciones de: {path}");

                if (!File.Exists(path))
                {
                    Debug.WriteLine($"[TransaccionJson] El archivo no existe, devolviendo lista vacía");
                    return new List<Transaccion>();
                }

                string json = await File.ReadAllTextAsync(path);
                Debug.WriteLine($"[TransaccionJson] Contenido leído: {json.Substring(0, Math.Min(100, json.Length))}..."); // Muestra solo los primeros 100 caracteres

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Para mayor flexibilidad en la deserialización
                };

                var transacciones = JsonSerializer.Deserialize<List<Transaccion>>(json, options);

                if (transacciones == null)
                {
                    Debug.WriteLine($"[TransaccionJson] La deserialización devolvió null, creando lista vacía");
                    return new List<Transaccion>();
                }

                Debug.WriteLine($"[TransaccionJson] Se cargaron {transacciones.Count} transacciones");
                return transacciones;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TransaccionJson] ERROR obteniendo transacciones: {ex.Message}");
                Debug.WriteLine($"[TransaccionJson] Stack trace: {ex.StackTrace}");

                // En caso de error, devolver una lista vacía para evitar que la app se cierre
                return new List<Transaccion>();
            }
        }

        public static async Task<bool> AgregarTransaccion(Transaccion nuevaTransaccion)
        {
            try
            {
                Debug.WriteLine($"[TransaccionJson] Iniciando proceso para agregar transacción para repuesto: {nuevaTransaccion.RepuestoId}");
                var transacciones = await ObtenerTransacciones();

                // Generar un ID único
                if (transacciones.Any())
                {
                    nuevaTransaccion.Id = transacciones.Max(t => t.Id) + 1;
                }
                else
                {
                    nuevaTransaccion.Id = 1; // Comenzar desde 1 si es la primera transacción
                }

                Debug.WriteLine($"[TransaccionJson] ID asignado: {nuevaTransaccion.Id}");

                // Asegurarse de que la fecha de creación esté establecida
                nuevaTransaccion.FechaCreacion = DateTime.Now;

                transacciones.Add(nuevaTransaccion);
                Debug.WriteLine($"[TransaccionJson] Transacción agregada a la lista en memoria. Total: {transacciones.Count}");

                await GuardarTransacciones(transacciones);
                Debug.WriteLine($"[TransaccionJson] Guardado completado exitosamente");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TransaccionJson] ERROR agregando transacción: {ex.Message}");
                Debug.WriteLine($"[TransaccionJson] Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public static async Task<List<Transaccion>> ObtenerTransaccionesPorRepuesto(int repuestoId)
        {
            try
            {
                Debug.WriteLine($"[TransaccionJson] Buscando transacciones para repuesto ID: {repuestoId}");
                var todasTransacciones = await ObtenerTransacciones();
                var transaccionesRepuesto = todasTransacciones.Where(t => t.RepuestoId == repuestoId).ToList();

                Debug.WriteLine($"[TransaccionJson] Se encontraron {transaccionesRepuesto.Count} transacciones para el repuesto ID: {repuestoId}");
                return transaccionesRepuesto;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TransaccionJson] ERROR obteniendo transacciones por repuesto: {ex.Message}");
                Debug.WriteLine($"[TransaccionJson] Stack trace: {ex.StackTrace}");
                return new List<Transaccion>();
            }
        }

        public static async Task<List<Transaccion>> ObtenerUltimasTransacciones(int cantidad)
        {
            try
            {
                Debug.WriteLine($"[TransaccionJson] Obteniendo últimas {cantidad} transacciones");
                var todasTransacciones = await ObtenerTransacciones();
                var ultimasTransacciones = todasTransacciones
                    .OrderByDescending(t => t.Fecha)
                    .Take(cantidad)
                    .ToList();

                Debug.WriteLine($"[TransaccionJson] Se obtuvieron {ultimasTransacciones.Count} transacciones recientes");
                return ultimasTransacciones;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TransaccionJson] ERROR obteniendo últimas transacciones: {ex.Message}");
                Debug.WriteLine($"[TransaccionJson] Stack trace: {ex.StackTrace}");
                return new List<Transaccion>();
            }
        }
    }
}
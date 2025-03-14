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
    public class RepuestoJson
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
                    Debug.WriteLine($"[RepuestoJson] Ruta Android: {ruta}");
                }
                else if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), directorio);
                    Debug.WriteLine($"[RepuestoJson] Ruta Windows: {ruta}");
                }
                else // iOS, macOS u otras plataformas
                {
                    ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), directorio);
                    Debug.WriteLine($"[RepuestoJson] Ruta iOS/Otras: {ruta}");
                }

                if (!Directory.Exists(ruta))
                {
                    Debug.WriteLine($"[RepuestoJson] Creando directorio: {ruta}");
                    Directory.CreateDirectory(ruta);
                }
                else
                {
                    Debug.WriteLine($"[RepuestoJson] Directorio ya existe: {ruta}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RepuestoJson] ERROR en GetFilePath: {ex.Message}");
                // En caso de error, intentar usar un directorio de respaldo
                ruta = Path.Combine(FileSystem.AppDataDirectory, directorio);

                if (!Directory.Exists(ruta))
                {
                    Directory.CreateDirectory(ruta);
                }
                Debug.WriteLine($"[RepuestoJson] Usando ruta alternativa: {ruta}");
            }

            string pathCompleto = Path.Combine(ruta, "repuestos.json");
            Debug.WriteLine($"[RepuestoJson] Ruta completa del archivo: {pathCompleto}");
            return pathCompleto;
        }

        public static async Task GuardarRepuestos(List<Repuesto> repuestos)
        {
            try
            {
                string path = GetFilePath();
                Debug.WriteLine($"[RepuestoJson] Guardando {repuestos.Count} repuestos en: {path}");

                // Usar opciones para hacer el JSON más legible y asegurar compatibilidad con propiedades nulas
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                string json = JsonSerializer.Serialize(repuestos, options);
                Debug.WriteLine($"[RepuestoJson] JSON serializado: {json.Substring(0, Math.Min(100, json.Length))}..."); // Muestra solo los primeros 100 caracteres

                await File.WriteAllTextAsync(path, json);
                Debug.WriteLine($"[RepuestoJson] Archivo guardado correctamente");

                // Verificar que el archivo existe después de guardarlo
                if (File.Exists(path))
                {
                    long fileSize = new FileInfo(path).Length;
                    Debug.WriteLine($"[RepuestoJson] Verificación: Archivo existe con tamaño: {fileSize} bytes");
                }
                else
                {
                    Debug.WriteLine($"[RepuestoJson] ADVERTENCIA: El archivo no existe después de guardarlo");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RepuestoJson] ERROR guardando repuestos: {ex.Message}");
                Debug.WriteLine($"[RepuestoJson] Stack trace: {ex.StackTrace}");
                throw; // Re-lanzar la excepción para que se maneje en la capa superior
            }
        }

        public static async Task<List<Repuesto>> ObtenerRepuestos()
        {
            try
            {
                string path = GetFilePath();
                Debug.WriteLine($"[RepuestoJson] Intentando leer repuestos de: {path}");

                if (!File.Exists(path))
                {
                    Debug.WriteLine($"[RepuestoJson] El archivo no existe, devolviendo lista vacía");
                    return new List<Repuesto>();
                }

                string json = await File.ReadAllTextAsync(path);
                Debug.WriteLine($"[RepuestoJson] Contenido leído: {json.Substring(0, Math.Min(100, json.Length))}..."); // Muestra solo los primeros 100 caracteres

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Para mayor flexibilidad en la deserialización
                };

                var repuestos = JsonSerializer.Deserialize<List<Repuesto>>(json, options);

                if (repuestos == null)
                {
                    Debug.WriteLine($"[RepuestoJson] La deserialización devolvió null, creando lista vacía");
                    return new List<Repuesto>();
                }

                Debug.WriteLine($"[RepuestoJson] Se cargaron {repuestos.Count} repuestos");
                return repuestos;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RepuestoJson] ERROR obteniendo repuestos: {ex.Message}");
                Debug.WriteLine($"[RepuestoJson] Stack trace: {ex.StackTrace}");

                // En caso de error, devolver una lista vacía para evitar que la app se cierre
                return new List<Repuesto>();
            }
        }

        public static async Task<bool> AgregarRepuesto(Repuesto nuevoRepuesto)
        {
            try
            {
                Debug.WriteLine($"[RepuestoJson] Iniciando proceso para agregar repuesto: {nuevoRepuesto.Codigo} - {nuevoRepuesto.Nombre}");
                var repuestos = await ObtenerRepuestos();

                // Comprobar si ya existe un repuesto con el mismo código
                if (repuestos.Any(r => r.Codigo == nuevoRepuesto.Codigo))
                {
                    Debug.WriteLine($"[RepuestoJson] Código duplicado: {nuevoRepuesto.Codigo}");
                    return false; // Código duplicado
                }

                // Generar un ID único
                if (repuestos.Any())
                {
                    nuevoRepuesto.Id = repuestos.Max(r => r.Id) + 1;
                }
                else
                {
                    nuevoRepuesto.Id = 1; // Comenzar desde 1 si es el primer repuesto
                }

                Debug.WriteLine($"[RepuestoJson] ID asignado: {nuevoRepuesto.Id}");

                repuestos.Add(nuevoRepuesto);
                Debug.WriteLine($"[RepuestoJson] Repuesto agregado a la lista en memoria. Total: {repuestos.Count}");

                await GuardarRepuestos(repuestos);
                Debug.WriteLine($"[RepuestoJson] Guardado completado exitosamente");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RepuestoJson] ERROR agregando repuesto: {ex.Message}");
                Debug.WriteLine($"[RepuestoJson] Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public static async Task<bool> EliminarRepuesto(int id)
        {
            try
            {
                Debug.WriteLine($"[RepuestoJson] Intentando eliminar repuesto con ID: {id}");
                var repuestos = await ObtenerRepuestos();
                var repuesto = repuestos.FirstOrDefault(r => r.Id == id);

                if (repuesto != null)
                {
                    Debug.WriteLine($"[RepuestoJson] Repuesto encontrado: {repuesto.Codigo} - {repuesto.Nombre}");
                    repuestos.Remove(repuesto);
                    await GuardarRepuestos(repuestos);
                    Debug.WriteLine($"[RepuestoJson] Repuesto eliminado exitosamente");
                    return true;
                }

                Debug.WriteLine($"[RepuestoJson] No se encontró repuesto con ID: {id}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RepuestoJson] ERROR eliminando repuesto: {ex.Message}");
                Debug.WriteLine($"[RepuestoJson] Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public static async Task<bool> ActualizarRepuesto(Repuesto repuestoActualizado)
        {
            try
            {
                Debug.WriteLine($"[RepuestoJson] Actualizando repuesto ID: {repuestoActualizado.Id}");
                var repuestos = await ObtenerRepuestos();
                var index = repuestos.FindIndex(r => r.Id == repuestoActualizado.Id);

                if (index >= 0)
                {
                    Debug.WriteLine($"[RepuestoJson] Repuesto encontrado en índice: {index}");
                    repuestos[index] = repuestoActualizado;
                    await GuardarRepuestos(repuestos);
                    Debug.WriteLine($"[RepuestoJson] Actualización completada");
                    return true;
                }

                Debug.WriteLine($"[RepuestoJson] No se encontró repuesto para actualizar con ID: {repuestoActualizado.Id}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RepuestoJson] ERROR actualizando repuesto: {ex.Message}");
                Debug.WriteLine($"[RepuestoJson] Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
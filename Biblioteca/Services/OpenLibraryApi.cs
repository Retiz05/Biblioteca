using Biblioteca.Models;
using Biblioteca.Models.ModelosDTO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Biblioteca.Services
{
    public class OpenLibraryApi
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string _urlBase = "https://openlibrary.org/";

        public static async Task<RespuestaBusqueda> ConsultarPorIsbnAsync(string isbn)
        {
            try
            {
                var libro = new LibroInformacionDTO
                {
                    Titulo = "Sin título",
                    NombresAutor = new List<string> { "Desconocido" },
                    Imagen = $"https://covers.openlibrary.org/b/isbn/{isbn}-L.jpg"
                };

                // Paso 1: Intentar con /api/books
                var apiBooksResponse = await client.GetAsync($"{_urlBase}api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data");
                if (apiBooksResponse.IsSuccessStatusCode)
                {
                    string json = await apiBooksResponse.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);
                    var entry = data[$"ISBN:{isbn}"];

                    libro.Titulo = entry?["title"]?.ToString() ?? libro.Titulo;
                    libro.Subtitulo = entry?["subtitle"]?.ToString();
                    libro.NombresAutor = entry?["authors"]?.Select(a => a["name"]?.ToString()).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? libro.NombresAutor;
                    libro.Contributors = entry?["authors"]?.Select(a => new ContributorDTO
                    {
                        Name = a["name"]?.ToString(),
                        Role = "author"
                    }).ToList();
                    libro.Editorial = entry?["publishers"]?.Select(p => p["name"]?.ToString()).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>();
                    libro.PublishDate = entry?["publish_date"]?.ToString();
                    libro.NumeroPaginas = entry?["number_of_pages"]?.ToObject<int?>();
                    libro.Imagen = entry?["cover"]?["large"]?.ToString() ?? libro.Imagen;
                    libro.AnioPrimeraPublicacion = entry?["first_publish_year"]?.ToObject<int?>() ?? ParseYear(libro.PublishDate);

                    Debug.WriteLine($"Desde /api/books - ISBN: {isbn}, Título: {libro.Titulo}, Año: {libro.AnioPrimeraPublicacion}, Autores: {string.Join(", ", libro.NombresAutor)}");
                }

                // Paso 2: Fallback a /search.json si no hay autores
                if (!libro.NombresAutor.Any(a => a != "Desconocido"))
                {
                    var searchResponse = await client.GetAsync($"{_urlBase}search.json?isbn={isbn}");
                    if (searchResponse.IsSuccessStatusCode)
                    {
                        string json = await searchResponse.Content.ReadAsStringAsync();
                        JObject data = JObject.Parse(json);
                        JArray docs = (JArray)data["docs"];
                        if (docs != null && docs.Any())
                        {
                            var doc = docs.FirstOrDefault(d => d["isbn_10"]?.ToString() == isbn || d["isbn_13"]?.ToString() == isbn);
                            if (doc != null)
                            {
                                libro.Titulo = doc["title"]?.ToString() ?? libro.Titulo;
                                libro.Subtitulo = doc["subtitle"]?.ToString() ?? libro.Subtitulo;
                                libro.NombresAutor = doc["author_name"]?.Select(a => a.ToString()).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? libro.NombresAutor;
                                libro.AnioPrimeraPublicacion = doc["first_publish_year"]?.ToObject<int?>() ?? libro.AnioPrimeraPublicacion;
                                Debug.WriteLine($"Desde /search.json - ISBN: {isbn}, Título: {libro.Titulo}, Año: {libro.AnioPrimeraPublicacion}, Autores: {string.Join(", ", libro.NombresAutor)}");
                            }
                        }
                    }
                }

                // Paso 3: Fallback a la edición si no hay autores
                if (!libro.NombresAutor.Any(a => a != "Desconocido"))
                {
                    var editionData = await GetEditionData(isbn);
                    if (editionData != null)
                    {
                        libro.Titulo = editionData.Titulo;
                        libro.Subtitulo = editionData.Subtitulo;
                        libro.NombresAutor = editionData.NombresAutor;
                        libro.Contributors = editionData.Contributors;
                        libro.Editorial = editionData.Editorial;
                        libro.PublishDate = editionData.PublishDate;
                        libro.NumeroPaginas = editionData.NumeroPaginas;
                        libro.Imagen = editionData.Imagen;
                        libro.AnioPrimeraPublicacion = editionData.AnioPrimeraPublicacion;
                        Debug.WriteLine($"Desde edición - ISBN: {isbn}, Título: {libro.Titulo}, Año: {libro.AnioPrimeraPublicacion}, Autores: {string.Join(", ", libro.NombresAutor)}");
                    }
                }

                return new RespuestaBusqueda
                {
                    LibrosEncontrados = libro.Titulo != "Sin título" ? 1 : 0,
                    Documentos = libro.Titulo != "Sin título" ? new List<LibroInformacionDTO> { libro } : new List<LibroInformacionDTO>()
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ConsultarPorIsbnAsync (ISBN: {isbn}): {ex.Message}");
                return new RespuestaBusqueda { LibrosEncontrados = 0, Documentos = new List<LibroInformacionDTO>() };
            }
        }

        public static async Task<string> ConsultarTitulo(string titulo)
        {
            try
            {
                var response = await client.GetAsync($"{_urlBase}search.json?title={HttpUtility.UrlEncode(titulo)}");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"ConsultarTitulo - Título: {titulo}, Respuesta: {json}");
                    return json;
                }
                Debug.WriteLine($"ConsultarTitulo - Título: {titulo}, Error: {response.StatusCode}");
                return $"Error: Libro no encontrado (Status: {response.StatusCode})";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ConsultarTitulo (Título: {titulo}): {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public static async Task<string> ConsultarPorClasificacion(string tema)
        {
            try
            {
                // Normalizar el tema para el endpoint (reemplazar espacios por guiones bajos y minúsculas)
                string temaNormalizado = tema.ToLower().Replace(" ", "_");
                var response = await client.GetAsync($"{_urlBase}subjects/{HttpUtility.UrlEncode(temaNormalizado)}.json");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"ConsultarPorClasificacion - Tema: {tema}, Respuesta: {json}");
                    return json;
                }
                Debug.WriteLine($"ConsultarPorClasificacion - Tema: {tema}, Error: {response.StatusCode}");
                return $"Error: No se encontraron libros para la clasificación '{tema}' (Status: {response.StatusCode})";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ConsultarPorClasificacion (Tema: {tema}): {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public static async Task<List<LibroDTO>> ConsultarLibrosLibreria(List<Libro> libros)
        {
            try
            {
                var librosDTO = new List<LibroDTO>();
                string endpoint = CrearEndpoint(libros);
                if (string.IsNullOrEmpty(endpoint))
                {
                    Debug.WriteLine("ConsultarLibrosLibreria: Lista de libros vacía o nula");
                    return librosDTO;
                }

                var response = await client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"ConsultarLibrosLibreria: Error {response.StatusCode} - No se encontraron libros");
                    return librosDTO;
                }

                var json = await response.Content.ReadAsStringAsync();
                var librosJson = JObject.Parse(json);

                foreach (var kvp in librosJson)
                {
                    var data = kvp.Value;
                    string isbn = kvp.Key.Replace("ISBN:", "");
                    var libroOriginal = libros.FirstOrDefault(l => l.ISBN == isbn);

                    var libroDTO = new LibroDTO
                    {
                        Clave = isbn,
                        Titulo = data["title"]?.ToString() ?? "Sin título",
                        Imagen = data["cover"]?["medium"]?.ToString() ?? $"https://covers.openlibrary.org/b/isbn/{isbn}-M.jpg",
                        Categoria = libroOriginal?.CategoriaID ?? 0,
                        Estatus = true,
                        Autor = data["authors"]?.Select(a => a["name"]?.ToString()).FirstOrDefault() ?? "Desconocido",
                        NumeroEjemplar = libroOriginal?.NumeroEjemplar ?? 1
                    };
                    librosDTO.Add(libroDTO);
                }

                Debug.WriteLine($"ConsultarLibrosLibreria: Encontrados {librosDTO.Count} libros");
                return librosDTO;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ConsultarLibrosLibreria: {ex.Message}");
                return new List<LibroDTO>();
            }
        }

        private static async Task<LibroInformacionDTO> GetEditionData(string isbn)
        {
            try
            {
                var apiBooksResponse = await client.GetAsync($"{_urlBase}api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data");
                if (apiBooksResponse.IsSuccessStatusCode)
                {
                    string json = await apiBooksResponse.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);
                    var entry = data[$"ISBN:{isbn}"];
                    var editionKey = entry?["key"]?.ToString()?.Replace("/books/", "");
                    if (!string.IsNullOrEmpty(editionKey))
                    {
                        var editionResponse = await client.GetAsync($"{_urlBase}books/{editionKey}.json");
                        if (editionResponse.IsSuccessStatusCode)
                        {
                            string editionJson = await editionResponse.Content.ReadAsStringAsync();
                            JObject editionData = JObject.Parse(editionJson);
                            var libro = new LibroInformacionDTO
                            {
                                Titulo = editionData["title"]?.ToString() ?? "Sin título",
                                Subtitulo = editionData["subtitle"]?.ToString(),
                                NombresAutor = editionData["contributors"]?.Select(c => new
                                {
                                    Name = c?["name"]?.ToString(),
                                    IsAuthor = c?["role"]?.ToString()?.IndexOf("author", StringComparison.OrdinalIgnoreCase) >= 0 ?? false
                                })
                                .Where(x => x.IsAuthor && !string.IsNullOrEmpty(x.Name))
                                .Select(x => x.Name)
                                .ToList() ?? new List<string> { "Desconocido" },
                                Contributors = editionData["contributors"]?.Select(c => new ContributorDTO
                                {
                                    Name = c?["name"]?.ToString(),
                                    Role = c?["role"]?.ToString()
                                }).ToList(),
                                Editorial = editionData["publishers"]?.Select(p => p.ToString()).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>(),
                                PublishDate = editionData["publish_date"]?.ToString(),
                                NumeroPaginas = editionData["number_of_pages"]?.ToObject<int?>(),
                                CoverId = null,
                                Imagen = editionData["cover"]?["large"]?.ToString() ?? $"https://covers.openlibrary.org/b/isbn/{isbn}-L.jpg",
                                AnioPrimeraPublicacion = ParseYear(editionData["publish_date"]?.ToString())
                            };
                            return libro;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en GetEditionData (ISBN: {isbn}): {ex.Message}");
                return null;
            }
        }

        private static int? ParseYear(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                Debug.WriteLine("ParseYear: Fecha nula o vacía");
                return null;
            }

            // Usar regex para extraer el año
            Match match = Regex.Match(date, @"\b(\d{4})\b");
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int year) && year > 1900 && year <= DateTime.Now.Year)
                {
                    Debug.WriteLine($"ParseYear: Año {year} extraído de {date}");
                    return year;
                }
            }

            // Fallback a DateTime.TryParse
            if (DateTime.TryParse(date, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime parsedDate))
            {
                int year = parsedDate.Year;
                Debug.WriteLine($"ParseYear: Año {year} extraído de {date} usando DateTime.TryParse");
                return year;
            }

            Debug.WriteLine($"ParseYear: No se pudo extraer el año de {date}");
            return null;
        }

        private static string CrearEndpoint(List<Libro> libros)
        {
            if (libros == null || !libros.Any())
            {
                Debug.WriteLine("CrearEndpoint: Lista de libros vacía o nula");
                return null;
            }

            var isbnList = string.Join(",", libros.Select(l => $"ISBN:{l.ISBN}"));
            string endpoint = $"{_urlBase}api/books?bibkeys={isbnList}&format=json&jscmd=data";
            Debug.WriteLine($"CrearEndpoint: {endpoint}");
            return endpoint;
        }
    }
}
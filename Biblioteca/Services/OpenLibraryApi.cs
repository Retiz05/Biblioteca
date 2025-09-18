using Biblioteca.Models.ModelosDTO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;

namespace Biblioteca.Services
{
    public class OpenLibraryApi
    {
        private static readonly HttpClient client = new HttpClient();
        private static string _urlBase = "https://openlibrary.org/";

        public static async Task<RespuestaBusqueda> ConsultarPorIsbnAsync(string isbn)
        {
            try
            {
                var libro = new LibroInformacionDTO
                {
                    Titulo = "Sin título",
                    NombresAutor = new List<string> { "Desconocido" }
                };

                // Paso 1: Intentar con /api/books como fuente principal
                var apiBooksResponse = await client.GetAsync($"{_urlBase}api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data");
                if (apiBooksResponse.IsSuccessStatusCode)
                {
                    string json = await apiBooksResponse.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);
                    var entry = data[$"ISBN:{isbn}"];

                    libro.Titulo = entry?["title"]?.ToString() ?? "Sin título";
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
                    libro.Imagen = entry?["cover"]?["large"]?.ToString() ?? $"https://covers.openlibrary.org/b/isbn/{isbn}-L.jpg";

                    libro.AnioPrimeraPublicacion = entry?["first_publish_year"]?.ToObject<int?>() ?? ParseYear(libro.PublishDate);
                    Debug.WriteLine($"Desde /api/books - Año: {libro.AnioPrimeraPublicacion}, PublishDate: {libro.PublishDate}, Autores: {string.Join(", ", libro.NombresAutor)}");
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
                            var doc = docs.FirstOrDefault(d => d["isbn_10"]?.ToString() == isbn);
                            if (doc != null)
                            {
                                libro.Titulo = doc["title"]?.ToString() ?? libro.Titulo;
                                libro.Subtitulo = doc["subtitle"]?.ToString() ?? libro.Subtitulo;
                                libro.NombresAutor = doc["author_name"]?.Select(a => a.ToString()).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? libro.NombresAutor;
                                libro.AnioPrimeraPublicacion = doc["first_publish_year"]?.ToObject<int?>() ?? libro.AnioPrimeraPublicacion;
                                Debug.WriteLine($"Después de /search.json - Año: {libro.AnioPrimeraPublicacion}, Autores: {string.Join(", ", libro.NombresAutor)}");
                            }
                        }
                    }
                }

                // Paso 3: Fallback a la edición si no hay autores
                if (!libro.NombresAutor.Any(a => a != "Desconocido"))
                {
                    var editionResponse = await GetEditionData(isbn);
                    if (editionResponse != null)
                    {
                        libro.Titulo = editionResponse.Titulo;
                        libro.Subtitulo = editionResponse.Subtitulo;
                        libro.NombresAutor = editionResponse.NombresAutor;
                        libro.Contributors = editionResponse.Contributors;
                        libro.Editorial = editionResponse.Editorial;
                        libro.PublishDate = editionResponse.PublishDate;
                        libro.NumeroPaginas = editionResponse.NumeroPaginas;
                        libro.Imagen = editionResponse.Imagen;
                        libro.AnioPrimeraPublicacion = editionResponse.AnioPrimeraPublicacion;
                        Debug.WriteLine($"Desde edición - Año: {libro.AnioPrimeraPublicacion}, Autores: {string.Join(", ", libro.NombresAutor)}");
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
                Debug.WriteLine($"Error en OpenLibraryApi: {ex.Message}");
                return new RespuestaBusqueda { LibrosEncontrados = 0, Documentos = new List<LibroInformacionDTO>() };
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
                                    IsAuthor = c?["role"]?.ToString()?.Contains("author") ?? false
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
                                Imagen = editionData["cover"]?["large"]?.ToString() ?? $"https://covers.openlibrary.org/b/isbn/{isbn}-L.jpg"
                            };

                            libro.AnioPrimeraPublicacion = ParseYear(libro.PublishDate);
                            return libro;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en GetEditionData: {ex.Message}");
                return null;
            }
        }

        public static int? ParseYear(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                Debug.WriteLine("ParseYear: date is null or empty");
                return null;
            }

            // Usar regex para extraer el año de una fecha con texto
            Match match = Regex.Match(date, @"\b(\d{4})\b");
            if (match.Success)
            {
                string yearStr = match.Groups[1].Value;
                if (int.TryParse(yearStr, out int year) && year > 1900 && year <= DateTime.Now.Year)
                {
                    Debug.WriteLine($"ParseYear: Successfully parsed year {year} from {date}");
                    return year;
                }
            }

            // Intentar parsear como fecha completa con cultura específica
            if (DateTime.TryParse(date, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime parsedDate))
            {
                int year = parsedDate.Year;
                Debug.WriteLine($"ParseYear: Successfully parsed year {year} from {date} using DateTime.TryParse");
                return year;
            }

            Debug.WriteLine($"ParseYear: Failed to parse year from {date}, returning null");
            return null;
        }
    }
}
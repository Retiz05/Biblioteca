using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Biblioteca.Models.ModelosDTO
{
    public class RespuestaBusqueda
    {
        [JsonProperty("numFound")]
        public int LibrosEncontrados { get; set; }

        [JsonProperty("start")]
        public int Inicio { get; set; }

        [JsonProperty("numFoundExact")]
        public bool EncontradosExactos { get; set; }

        [JsonProperty("num_found")]
        public int LibrosEncontradosAlt { get; set; }

        [JsonProperty("documentation_url")]
        public string UrlDocumentacion { get; set; }

        [JsonProperty("q")]
        public string Consulta { get; set; }

        [JsonProperty("offset")]
        public object Desplazamiento { get; set; }

        [JsonProperty("docs")]
        public List<LibroInformacionDTO> Documentos { get; set; }
    }

    public class LibroInformacionDTO
    {
        [JsonProperty("title")]
        public string Titulo { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitulo { get; set; }

        [JsonProperty("author_name")]
        public List<string> NombresAutor { get; set; }

        [JsonProperty("contributors")]
        public List<ContributorDTO> Contributors { get; set; }

        [JsonProperty("publisher")]
        public List<string> Editorial { get; set; }

        [JsonProperty("first_publish_year")]
        public int? AnioPrimeraPublicacion { get; set; }

        [JsonProperty("publish_date")]
        public string PublishDate { get; set; }

        [JsonProperty("number_of_pages_median")]
        public int? NumeroPaginas { get; set; }

        [JsonProperty("edition_count")]
        public int? NumeroEdiciones { get; set; }

        [JsonProperty("cover_i")]
        public int? CoverId { get; set; }

        public string Imagen { get; set; }
    }

    public class ContributorDTO
    {
        public string Role { get; set; }
        public string Name { get; set; }
    }

    public class LibroDTO
    {
        public string Clave { get; set; } // ISBN u otra clave única
        public string Titulo { get; set; }
        public string Imagen { get; set; }
        public int Categoria { get; set; } // ID de la categoría
        public bool Estatus { get; set; }
        public string Autor { get; set; } // Agregado para compatibilidad con vistas
        public int NumeroEjemplar { get; set; } // Agregado para compatibilidad con LibroEstadoDTO

        public static implicit operator string(LibroDTO v)
        {
            throw new NotImplementedException();
        }
    }

    public class LibroEstadoDTO
    {
        public string ISBN { get; set; }
        public int NumeroEjemplar { get; set; }
        public bool Estatus { get; set; }
    }

    public class BibliotecaLibroViewModel
    {
        public int ID { get; set; }
        public string ISBN { get; set; }
        public int NumeroEjemplar { get; set; }
        public string Clasificacion { get; set; }
        public bool Estatus { get; set; }
        public string Autor { get; set; }
        public int NumeroAdquisicion { get; set; }
        public string CategoriaNombre { get; set; }
        public int? PrestamoActivoId { get; set; }
    }

    public class ClienteDashboardViewModel
    {
        public int TotalLibros { get; set; }
        public int LibrosPrestados { get; set; }
        public int LibrosDisponibles { get; set; }
        public List<LibroEstadoDTO> LibrosEstado { get; set; }
    }
}
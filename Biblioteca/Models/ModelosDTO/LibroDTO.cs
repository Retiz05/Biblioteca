using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
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
        public int? AnioPrimeraPublicacion { get; set; } // nullable para evitar errores
        [JsonProperty("publish_date")]
        public string PublishDate { get; set; } // string porque puede venir como "2005" o "Marzo 2005"

        [JsonProperty("number_of_pages_median")]
        public int? NumeroPaginas { get; set; } // nullable

        [JsonProperty("edition_count")]
        public int? NumeroEdiciones { get; set; } // nullable

        [JsonProperty("cover_i")]
        public int? CoverId { get; set; }

        // Ahora es una propiedad normal con get y set
        public string Imagen { get; set; }
    }

    public class ContributorDTO
    {
        public string Role { get; set; }
        public string Name { get; set; }
    }



    // Nota: LibroDTO no se usa actualmente, puedes eliminarlo o ajustarlo si lo necesitas
    public class LibroEstadoDTO
    {
        public string ISBN { get; set; }
        public string Materia { get; set; }
        public int NumeroEjemplar { get; set; }
        public bool Estatus { get; set; }

        public static implicit operator string(LibroEstadoDTO v)
        {
            throw new NotImplementedException();
        }
    }
    public class ClienteDashboardViewModel
    {
        public int TotalLibros { get; set; }
        public int LibrosPrestados { get; set; }
        public int LibrosDisponibles { get; set; }
        public List<LibroEstadoDTO> LibrosEstado { get; set; }
    }
}
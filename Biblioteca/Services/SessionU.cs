using Biblioteca.Models.ModelosDTO;
using System.Web;

namespace Biblioteca.Services
{
    public class SessionU
    {
        public static void IniciarSesion(UsuarioDTO usuario)
        {
            HttpContext.Current.Session["UsuarioID"] = usuario.ID;
            HttpContext.Current.Session["Nombre"] = usuario.Nombre;
            HttpContext.Current.Session["Rol"] = usuario.Rol;
            HttpContext.Current.Session["BibliotecaID"] = usuario.BibliotecaID; // Añadido
        }

        public static void CerrarSesion()
        {
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
        }

        public static string ObtenerRol()
        {
            return HttpContext.Current.Session["Rol"]?.ToString();
        }
    }
}
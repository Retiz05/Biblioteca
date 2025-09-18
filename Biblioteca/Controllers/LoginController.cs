using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Biblioteca.Models;
using Biblioteca.Models.ModelosDTO;
using Biblioteca.Services;

namespace Biblioteca.Controllers
{
    public class LoginController : Controller
    {
        private Context db = new Context();

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(LoginDTO usuario)
        {
            try
            {
                var cliente = await ValidarCliente(usuario);
                if (cliente != null)
                {
                    Debug.Print("----------------Iniciando sesion----------");
                    SessionU.IniciarSesion(cliente);
                    return RedirectToAction("Index", "Cliente");
                }
                else
                {
                    Debug.Print("----------------No se encontro----------");
                    ViewBag.ErrorMessage = "El correo o la contraseña son incorrectos";
                    return View(usuario);
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"----------------Excepcion: {ex.Message}----------");
                ViewBag.ErrorMessage = "Ocurrió un error al intentar iniciar sesión.";
                return View(usuario);
            }
        }

        public ActionResult Logout()
        {
            SessionU.CerrarSesion();
            return RedirectToAction("Index");
        }

        public async Task<UsuarioDTO> ValidarCliente(LoginDTO usuario)
        {
            try
            {
                var cliente = await db.Clientes
                    .Include(a => a.RolClientes)
                    .Where(c => c.Correo == usuario.Correo && c.Contrasena == usuario.Contrasena)
                    .Select(c => new UsuarioDTO
                    {
                        ID = c.ID,
                        Nombre = c.Nombre,
                        Rol = c.RolClientes.Nombre ?? "Sin rol",
                        BibliotecaID = c.BibliotecaID // Añadido
                    })
                    .FirstOrDefaultAsync();
                return cliente;
            }
            catch (Exception ex)
            {
                Debug.Print($"Ocurrio un error al validar con Cliente: {ex.Message}");
                return null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
using Biblioteca.Models;
using Biblioteca.Models.ModelosDTO;
using Biblioteca.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Biblioteca.Controllers
{
    public class LoginController : Controller
    {
        private Context db = new Context();

        // GET: Login
        [HttpGet]
        public ActionResult Index()
        {
            return View("~/Views/Login/Index.cshtml");
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(LoginDTO login)
        {
            try
            {
                var adminTask = BuscarAdmin(login);
                var clienteTask = BuscarCliente(login);
                var usuarioTask = BuscarUsuario(login);

                await Task.WhenAll(adminTask, clienteTask, usuarioTask);

                var administrador = adminTask.Result;
                var cliente = clienteTask.Result;
                var usuario = usuarioTask.Result;

                if (administrador != null)
                {
                    System.Diagnostics.Debug.WriteLine("----------------Iniciando sesión como Administrador----------");
                    SessionU.IniciarSesion(administrador);
                    return RedirectToAction("Index", "Administrador");
                }
                if (cliente != null)
                {
                    System.Diagnostics.Debug.WriteLine("----------------Iniciando sesión como Cliente----------");
                    SessionU.IniciarSesion(cliente);
                    return RedirectToAction("Index", "Cliente");
                }
                if (usuario != null)
                {
                    System.Diagnostics.Debug.WriteLine("----------------Iniciando sesión como Usuario----------");
                    SessionU.IniciarSesion(usuario);
                    return RedirectToAction("Index", "Usuarios");
                }

                System.Diagnostics.Debug.WriteLine("----------------No se encontró el usuario----------");
                ViewBag.ErrorMessage = "Correo o contraseña incorrectos";
                return View("~/Views/Login/Index.cshtml", login);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"----------------Excepción: {ex.Message}----------");
                ViewBag.ErrorMessage = "Ocurrió un error al intentar iniciar sesión.";
                return View("~/Views/Login/Index.cshtml", login);
            }
        }

        // GET: Login/Logout
        [HttpGet]
        public ActionResult Logout()
        {
            SessionU.CerrarSesion();
            return RedirectToAction("Index");
        }

        // GET: Login/RecuperarContrasena
        [HttpGet]
        public ActionResult RecuperarContrasena()
        {
            return View("~/Views/Login/RecuperarContrasena.cshtml");
        }

        // POST: Login/RecuperarContrasena
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RecuperarContrasena(string correo)
        {
            try
            {
                // Buscar en Administradores, Clientes y Usuarios
                var admin = await db.Administradores.FirstOrDefaultAsync(a => a.Correo == correo && a.Estatus == true);
                var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Correo == correo && c.Estatus == true);
                var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo && u.Estatus == true);

                if (admin != null || cliente != null || usuario != null)
                {
                    // TODO: Implementar lógica para enviar un correo de recuperación
                    System.Diagnostics.Debug.WriteLine($"----------------Correo de recuperación enviado a {correo}----------");
                    ViewBag.SuccessMessage = "Se ha enviado un enlace de recuperación a tu correo.";
                    return View("~/Views/Login/RecuperarContrasena.cshtml");
                }

                ViewBag.ErrorMessage = "No se encontró una cuenta asociada a ese correo.";
                return View("~/Views/Login/RecuperarContrasena.cshtml");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"----------------Excepción en RecuperarContrasena: {ex.Message}----------");
                ViewBag.ErrorMessage = "Ocurrió un error al intentar procesar la solicitud.";
                return View("~/Views/Login/RecuperarContrasena.cshtml");
            }
        }

        private async Task<UsuarioDTO> BuscarAdmin(LoginDTO usuario)
        {
            try
            {
                return await db.Administradores
                    .Where(c => c.Correo == usuario.Correo && c.Contrasena == usuario.Contrasena && c.Estatus == true)
                    .Select(c => new UsuarioDTO
                    {
                        ID = c.ID,
                        Nombre = c.Nombre,
                        Rol = "Administrador",
                        BibliotecaID = null // Administradores no tienen BibliotecaID
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ocurrió un error al validar con Administrador: {ex.Message}");
                return null;
            }
        }

        private async Task<UsuarioDTO> BuscarCliente(LoginDTO usuario)
        {
            try
            {
                return await db.Clientes
                    .Include(a => a.RolClientes)
                    .Where(c => c.Correo == usuario.Correo && c.Contrasena == usuario.Contrasena && c.Estatus == true)
                    .Select(c => new UsuarioDTO
                    {
                        ID = c.ID,
                        Nombre = c.Nombre,
                        Rol = c.RolClientes.Nombre ?? "Sin rol",
                        BibliotecaID = c.BibliotecaID
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ocurrió un error al validar con Cliente: {ex.Message}");
                return null;
            }
        }

        private async Task<UsuarioDTO> BuscarUsuario(LoginDTO usuario)
        {
            try
            {
                return await db.Usuarios
                    .Include(a => a.RolUsuario)
                    .Where(c => c.Correo == usuario.Correo && c.Contrasena == usuario.Contrasena && c.Estatus == true)
                    .Select(c => new UsuarioDTO
                    {
                        ID = c.ID,
                        Nombre = c.Nombre,
                        Rol = c.RolUsuario.Nombre ?? "Sin rol",
                        BibliotecaID = c.BibliotecaID
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ocurrió un error al validar con Usuario: {ex.Message}");
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
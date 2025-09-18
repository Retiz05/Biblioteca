using Biblioteca.Models;
using Biblioteca.Models.ModelosDTO;
using Biblioteca.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Biblioteca.Controllers
{
    public class ClienteController : Controller
    {
        private readonly Context _context;

        public ClienteController()
        {
            _context = new Context();
        }

        public ActionResult Index()
        {
            var librosEstado = _context.Libros.Select(l => new LibroEstadoDTO
            {
                ISBN = l.ISBN ?? l.ID.ToString(),
                Materia = l.Materia,
                NumeroEjemplar = l.NumeroEjemplar,
                Estatus = l.Estatus
            }).ToList();

            var model = new ClienteDashboardViewModel
            {
                TotalLibros = _context.Libros.Count(),
                LibrosPrestados = _context.Libros.Count(l => l.Estatus == false),
                LibrosDisponibles = _context.Libros.Count(l => l.Estatus == true),
                LibrosEstado = librosEstado
            };

            return View(model);
        }


        public ActionResult Usuarios()
        {
            IEnumerable<Usuario> usuarios;
            try
            {
                usuarios = _context.Usuarios.Include(u => u.RolUsuario).ToList();
                if (usuarios == null)
                {
                    usuarios = new List<Usuario>();
                    TempData["AlertMessage"] = "No se pudieron cargar los usuarios.";
                    TempData["AlertType"] = "error";
                }
                else
                {
                    foreach (var u in usuarios)
                    {
                        if (u.RolUsuario == null)
                        {
                            TempData["AlertMessage"] = "Algunos usuarios no tienen un rol asignado.";
                            TempData["AlertType"] = "warning";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                usuarios = new List<Usuario>();
                TempData["AlertMessage"] = $"Error al cargar los usuarios: {ex.Message}";
                TempData["AlertType"] = "error";
            }
            ViewBag.Roles = new SelectList(_context.RolUsuarios, "ID", "Nombre");
            return View(usuarios);
        }

        public ActionResult CrearUsuario()
        {
            var usuarios = _context.Usuarios.Include(u => u.RolUsuario).ToList();
            ViewBag.Roles = new SelectList(_context.RolUsuarios, "ID", "Nombre");
            TempData["ShowModal"] = "true";
            TempData["ModalTitle"] = "Crear Usuario";
            TempData["UserModel"] = new Usuario();
            return View("Usuarios", usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearUsuario(Usuario usuario)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var usuarios = _context.Usuarios.Include(u => u.RolUsuario).ToList();
                    ViewBag.Roles = new SelectList(_context.RolUsuarios, "ID", "Nombre");
                    TempData["ShowModal"] = "true";
                    TempData["ModalTitle"] = "Crear Usuario";
                    TempData["UserModel"] = usuario;
                    return View("Usuarios", usuarios);
                }

                if (Session["BibliotecaID"] == null)
                {
                    TempData["AlertMessage"] = "No se encontró el ID de la biblioteca.";
                    TempData["AlertType"] = "error";
                    return RedirectToAction("Usuarios");
                }

                if (string.IsNullOrEmpty(usuario.Contrasena))
                {
                    ModelState.AddModelError("Contrasena", "La contraseña es obligatoria.");
                    var usuarios = _context.Usuarios.Include(u => u.RolUsuario).ToList();
                    ViewBag.Roles = new SelectList(_context.RolUsuarios, "ID", "Nombre");
                    TempData["ShowModal"] = "true";
                    TempData["ModalTitle"] = "Crear Usuario";
                    TempData["UserModel"] = usuario;
                    return View("Usuarios", usuarios);
                }

                usuario.BibliotecaID = Convert.ToInt32(Session["BibliotecaID"]);
                _context.Usuarios.Add(usuario);
                _context.SaveChanges();
                TempData["AlertMessage"] = "Usuario registrado correctamente.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Usuarios");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error al registrar el usuario: {ex.Message}";
                TempData["AlertType"] = "error";
                var usuarios = _context.Usuarios.Include(u => u.RolUsuario).ToList();
                ViewBag.Roles = new SelectList(_context.RolUsuarios, "ID", "Nombre");
                TempData["ShowModal"] = "true";
                TempData["ModalTitle"] = "Crear Usuario";
                TempData["UserModel"] = usuario;
                return View("Usuarios", usuarios);
            }
        }

        public ActionResult EditarUsuario(int id)
        {
            var usuarios = _context.Usuarios.Include(u => u.RolUsuario).ToList();
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null)
            {
                TempData["AlertMessage"] = "Usuario no encontrado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Usuarios");
            }
            ViewBag.Roles = new SelectList(_context.RolUsuarios, "ID", "Nombre");
            TempData["ShowModal"] = "true";
            TempData["ModalTitle"] = "Editar Usuario";
            TempData["UserModel"] = usuario;
            return View("Usuarios", usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarUsuario(Usuario usuario)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var usuarios = _context.Usuarios.Include(u => u.RolUsuario).ToList();
                    ViewBag.Roles = new SelectList(_context.RolUsuarios, "ID", "Nombre");
                    TempData["ShowModal"] = "true";
                    TempData["ModalTitle"] = "Editar Usuario";
                    TempData["UserModel"] = usuario;
                    return View("Usuarios", usuarios);
                }

                if (Session["BibliotecaID"] == null)
                {
                    TempData["AlertMessage"] = "No se encontró el ID de la biblioteca.";
                    TempData["AlertType"] = "error";
                    return RedirectToAction("Usuarios");
                }

                var existingUsuario = _context.Usuarios.Find(usuario.ID);
                if (existingUsuario == null)
                {
                    TempData["AlertMessage"] = "Usuario no encontrado.";
                    TempData["AlertType"] = "error";
                    return RedirectToAction("Usuarios");
                }

                existingUsuario.Nombre = usuario.Nombre;
                existingUsuario.Correo = usuario.Correo;
                existingUsuario.RolID = usuario.RolID;
                existingUsuario.BibliotecaID = Convert.ToInt32(Session["BibliotecaID"]);
                if (!string.IsNullOrEmpty(usuario.Contrasena))
                {
                    existingUsuario.Contrasena = usuario.Contrasena;
                }

                _context.Entry(existingUsuario).State = EntityState.Modified;
                _context.SaveChanges();
                TempData["AlertMessage"] = "Usuario editado correctamente.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Usuarios");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error al editar el usuario: {ex.Message}";
                TempData["AlertType"] = "error";
                var usuarios = _context.Usuarios.Include(u => u.RolUsuario).ToList();
                ViewBag.Roles = new SelectList(_context.RolUsuarios, "ID", "Nombre");
                TempData["ShowModal"] = "true";
                TempData["ModalTitle"] = "Editar Usuario";
                TempData["UserModel"] = usuario;
                return View("Usuarios", usuarios);
            }
        }

        public ActionResult DeleteUser(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null)
            {
                TempData["AlertMessage"] = "Usuario no encontrado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Usuarios");
            }
            try
            {
                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();
                TempData["AlertMessage"] = "Usuario eliminado correctamente.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Usuarios");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error al eliminar el usuario: {ex.Message}";
                TempData["AlertType"] = "error";
                return RedirectToAction("Usuarios");
            }
        }

        public ActionResult Libros()
        {
            var libros = _context.Libros.Include(l => l.Categoria).ToList();
            ViewBag.Categorias = new SelectList(_context.Categorias, "ID", "Nombre");
            ViewBag.Temas = new SelectList(_context.Temas, "ID", "Nombre");
            ViewBag.Usuarios = new SelectList(_context.Usuarios, "ID", "Nombre");
            ViewBag.TipoPrestamos = new SelectList(_context.TipoPrestamos, "ID", "Nombre");
            ViewBag.TotalLibros = _context.Libros.Count();
            ViewBag.LibrosPrestados = _context.Libros.Count(l => l.Estatus == false);
            ViewBag.LibrosDisponibles = _context.Libros.Count(l => l.Estatus == true);
            return View(libros); // Pasar el modelo explícitamente
        }

        [HttpGet]
        public async Task<ActionResult> SearchApi(string ISBN)
        {
            if (string.IsNullOrEmpty(ISBN))
            {
                TempData["AlertMessage"] = "Ingrese un ISBN válido.";
                TempData["AlertType"] = "warning";
                return RedirectToAction("Libros");
            }

            var apiResponse = await OpenLibraryApi.ConsultarPorIsbnAsync(ISBN);
            TempData["ApiRaw"] = JsonConvert.SerializeObject(apiResponse);

            if (apiResponse.LibrosEncontrados > 0 && apiResponse.Documentos.Any())
            {
                var bookData = apiResponse.Documentos
                    .FirstOrDefault(d => d.NombresAutor != null && d.NombresAutor.Any() && d.NombresAutor[0] != "Desconocido")
                    ?? apiResponse.Documentos.First();

                // Asegurar que NombresAutor se extraiga correctamente
                if (bookData.NombresAutor == null || !bookData.NombresAutor.Any())
                {
                    bookData.NombresAutor = bookData.Contributors?.Where(c => c.Role?.ToLower().Contains("author") ?? false)
                        .Select(c => c.Name).ToList() ?? new List<string> { "Desconocido" };
                }

                int ultimoEjemplar = _context.Libros
                    .Where(l => l.ISBN == ISBN)
                    .OrderByDescending(l => l.NumeroEjemplar)
                    .Select(l => l.NumeroEjemplar)
                    .FirstOrDefault();

                var materia = !string.IsNullOrEmpty(bookData.Subtitulo) ? bookData.Subtitulo : bookData.Titulo;

                List<string> autores = bookData.NombresAutor != null && bookData.NombresAutor.Any()
                    ? bookData.NombresAutor
                    : new List<string> { "Desconocido" };

                var libro = new Libro
                {
                    ISBN = ISBN,
                    Materia = !string.IsNullOrEmpty(bookData.Subtitulo)
                        ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(bookData.Subtitulo.ToLower())
                        : bookData.Titulo,
                    NumeroEjemplar = ultimoEjemplar + 1,
                    Autor = string.Join(", ", autores)
                };

                var serialized = JsonConvert.SerializeObject(bookData);
                ViewData["ApiData"] = serialized;
                Session["ApiDataJson"] = serialized;
                TempData["ApiDataJson"] = serialized;
                TempData["BookModel"] = libro;

                Debug.WriteLine($"Año desde API en controlador: {bookData.AnioPrimeraPublicacion}");
                Debug.WriteLine($"Autores desde API en controlador: {string.Join(", ", autores)}");

                TempData["ShowModal"] = "true";
                TempData["ModalTitle"] = "Agregar Libro";
                ViewBag.Autores = libro.Autor;
            }
            else
            {
                TempData["AlertMessage"] = "No se encontró el libro en la API.";
                TempData["AlertType"] = "warning";
                TempData["ShowModal"] = "true";
                TempData["ModalTitle"] = "Agregar Libro";
                TempData["BookModel"] = new Libro();
                ViewData["ApiData"] = null;
            }

            ViewBag.Categorias = new SelectList(_context.Categorias, "ID", "Nombre");
            ViewBag.Temas = new SelectList(_context.Temas, "ID", "Nombre");

            return View("Libros", _context.Libros.Include(l => l.Categoria).ToList());
        }

        [HttpGet]
        public ActionResult CreateBook()
        {
            var libros = _context.Libros.Include(l => l.Categoria).ToList();
            ViewBag.Categorias = new SelectList(_context.Categorias, "ID", "Nombre");
            ViewBag.Temas = new SelectList(_context.Temas, "ID", "Nombre");
            TempData["ShowModal"] = "true";
            TempData["ModalTitle"] = "Agregar Libro";
            TempData["BookModel"] = new Libro();
            ViewData["ApiData"] = null; // Asegurar valor por defecto
            return View("Libros", libros);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBook(Libro libro, int CantidadEjemplares = 1, int TemaID = 0)
        {
            try
            {
                if (!ModelState.IsValid || CantidadEjemplares < 1)
                {
                    TempData["AlertMessage"] = "Ingrese una cantidad válida de ejemplares.";
                    TempData["AlertType"] = "error";
                    TempData["ShowModal"] = "true";
                    TempData["ModalTitle"] = "Agregar Libro";
                    TempData["BookModel"] = libro;
                    ViewBag.Categorias = new SelectList(_context.Categorias, "ID", "Nombre");
                    ViewBag.Temas = new SelectList(_context.Temas, "ID", "Nombre");
                    return View("Libros", _context.Libros.Include(l => l.Categoria).ToList());
                }

                var categoria = _context.Categorias.Find(libro.CategoriaID);
                if (categoria == null)
                {
                    TempData["AlertMessage"] = "La categoría seleccionada no existe.";
                    TempData["AlertType"] = "error";
                    TempData["ShowModal"] = "true";
                    TempData["ModalTitle"] = "Agregar Libro";
                    TempData["BookModel"] = libro;
                    ViewBag.Categorias = new SelectList(_context.Categorias, "ID", "Nombre");
                    ViewBag.Temas = new SelectList(_context.Temas, "ID", "Nombre");
                    return View("Libros", _context.Libros.Include(l => l.Categoria).ToList());
                }

                int ultimoEjemplar = _context.Libros
                    .Where(l => l.ISBN == libro.ISBN)
                    .Select(l => l.NumeroEjemplar)
                    .DefaultIfEmpty(0)
                    .Max();

                List<string> autores = new List<string> { "Desconocido" };
                if (Session["ApiDataJson"] != null)
                {
                    var apiData = JsonConvert.DeserializeObject<LibroInformacionDTO>((string)Session["ApiDataJson"]);
                    if (apiData?.NombresAutor != null && apiData.NombresAutor.Any())
                        autores = apiData.NombresAutor;
                    else if (apiData?.Contributors != null && apiData.Contributors.Any())
                        autores = apiData.Contributors
                            .Where(c => c.Role != null && c.Role.ToLower().Contains("author"))
                            .Select(c => c.Name)
                            .ToList() ?? new List<string> { "Desconocido" };
                }
                string autoresStr = string.Join(", ", autores);

                string temaNombre = null;
                if (TemaID > 0)
                {
                    var tema = _context.Temas.Find(TemaID);
                    if (tema != null) temaNombre = tema.Nombre;
                }

                int anio = DateTime.Now.Year;
                if (Session["ApiDataJson"] != null)
                {
                    var apiDataTmp = JsonConvert.DeserializeObject<LibroInformacionDTO>((string)Session["ApiDataJson"]);
                    if (apiDataTmp?.AnioPrimeraPublicacion != null)
                        anio = apiDataTmp.AnioPrimeraPublicacion.Value;
                    else if (!string.IsNullOrEmpty(apiDataTmp?.PublishDate))
                        anio = OpenLibraryApi.ParseYear(apiDataTmp.PublishDate) ?? anio;
                }

                for (int i = 1; i <= CantidadEjemplares; i++)
                {
                    var ejemplarNum = ultimoEjemplar + i;
                    var newLibro = new Libro
                    {
                        ISBN = libro.ISBN,
                        Materia = libro.Materia,
                        NumeroEjemplar = ejemplarNum,
                        Clasificacion = libro.Clasificacion ?? GenerateClasificacion(libro, ejemplarNum, anio, temaNombre),
                        Estatus = true,
                        CategoriaID = libro.CategoriaID,
                        Autor = autoresStr
                    };
                    _context.Libros.Add(newLibro);
                }

                _context.SaveChanges();

                var ultimos = _context.Libros
                    .Where(l => l.ISBN == libro.ISBN)
                    .OrderByDescending(l => l.ID)
                    .Take(CantidadEjemplares)
                    .ToList();
                TempData["InsertedPreview"] = JsonConvert.SerializeObject(ultimos);
                Session.Remove("ApiDataJson");

                TempData["AlertMessage"] = $"{CantidadEjemplares} ejemplar(es) registrado(s) correctamente.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Libros");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error al registrar el libro: {ex.Message}";
                TempData["AlertType"] = "error";
                TempData["ShowModal"] = "true";
                TempData["ModalTitle"] = "Agregar Libro";
                TempData["BookModel"] = libro;
                ViewBag.Categorias = new SelectList(_context.Categorias, "ID", "Nombre");
                ViewBag.Temas = new SelectList(_context.Temas, "ID", "Nombre");
                return View("Libros", _context.Libros.Include(l => l.Categoria).ToList());
            }
        }



        // Generar clasificación
        private string GenerateClasificacion(LibroInformacionDTO bookData, int ejemplar)
        {
            var categoria = _context.Categorias.FirstOrDefault(c => c.Nombre == "General")?.Clave ?? "000";
            var tema = bookData.Titulo.Length >= 3 ? bookData.Titulo.Substring(0, 3) : "GEN";
            var anio = bookData.AnioPrimeraPublicacion > 0 ? bookData.AnioPrimeraPublicacion : DateTime.Now.Year;
            return $"{categoria}-{tema}-{anio}-{ejemplar}";
        }

        private string GenerateClasificacion(Libro libro, int ejemplar, int anio = 0, string temaNombre = null)
        {
            var categoriaClave = _context.Categorias.FirstOrDefault(c => c.ID == libro.CategoriaID)?.Clave ?? "000";
            var tema = !string.IsNullOrEmpty(temaNombre)
                        ? (temaNombre.Length >= 3 ? temaNombre.Substring(0, 3) : temaNombre)
                        : (!string.IsNullOrEmpty(libro.Materia) ? (libro.Materia.Length >= 3 ? libro.Materia.Substring(0, 3) : libro.Materia) : "GEN");
            if (anio == 0) anio = DateTime.Now.Year;
            return $"{categoriaClave} - {tema} - {anio} - {ejemplar}";
        }



        [HttpGet]
        public JsonResult GetTemasByCategoria(int categoriaId)
        {
            var temas = _context.Temas
                        .Where(t => t.CategoriaID == categoriaId)
                        .Select(t => new { t.ID, t.Nombre })
                        .OrderBy(t => t.Nombre)
                        .ToList();
            return Json(temas, JsonRequestBehavior.AllowGet);
        }




        public ActionResult EditBook(int id)
        {
            var libros = _context.Libros.Include(l => l.Categoria).ToList();
            var libro = _context.Libros.Find(id);
            if (libro == null)
            {
                TempData["AlertMessage"] = "Libro no encontrado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }
            ViewBag.Categorias = new SelectList(_context.Categorias, "ID", "Nombre");
            ViewBag.Temas = new SelectList(_context.Temas, "Nombre", "Nombre");
            TempData["ShowModal"] = "true";
            TempData["ModalTitle"] = "Editar Libro";
            TempData["BookModel"] = libro;
            return View("Libros", libros);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBook(Libro libro)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var libros = _context.Libros.Include(l => l.Categoria).ToList();
                    ViewBag.Categorias = new SelectList(_context.Categorias, "ID", "Nombre");
                    ViewBag.Temas = new SelectList(_context.Temas, "Nombre", "Nombre");
                    TempData["ShowModal"] = "true";
                    TempData["ModalTitle"] = "Editar Libro";
                    TempData["BookModel"] = libro;
                    return View("Libros", libros);
                }

                var existingLibro = _context.Libros.Find(libro.ID);
                if (existingLibro == null)
                {
                    TempData["AlertMessage"] = "Libro no encontrado.";
                    TempData["AlertType"] = "error";
                    return RedirectToAction("Libros");
                }

                existingLibro.ISBN = libro.ISBN; // Cambiado de KEY a ISBN
                existingLibro.Materia = libro.Materia;
                existingLibro.NumeroEjemplar = libro.NumeroEjemplar;
                existingLibro.Clasificacion = libro.Clasificacion;
                existingLibro.CategoriaID = libro.CategoriaID;
                existingLibro.Estatus = libro.Estatus;

                _context.Entry(existingLibro).State = EntityState.Modified;
                _context.SaveChanges();
                TempData["AlertMessage"] = "Libro editado correctamente.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Libros");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error al editar el libro: {ex.Message}";
                TempData["AlertType"] = "error";
                var libros = _context.Libros.Include(l => l.Categoria).ToList();
                ViewBag.Categorias = new SelectList(_context.Categorias, "ID", "Nombre");
                ViewBag.Temas = new SelectList(_context.Temas, "Nombre", "Nombre");
                TempData["ShowModal"] = "true";
                TempData["ModalTitle"] = "Editar Libro";
                TempData["BookModel"] = libro;
                return View("Libros", libros);
            }
        }

        public ActionResult DeleteBook(int id)
        {
            var libro = _context.Libros.Find(id);
            if (libro == null)
            {
                TempData["AlertMessage"] = "Libro no encontrado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }
            try
            {
                _context.Libros.Remove(libro);
                _context.SaveChanges();
                TempData["AlertMessage"] = "Libro eliminado correctamente.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Libros", new { refresh = true }); // Forzar recarga
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error al eliminar el libro: {ex.Message}";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }
        }

        public ActionResult LendBook(int id)
        {
            var libros = _context.Libros.Include(l => l.Categoria).ToList();
            var libro = _context.Libros.Find(id);
            if (libro == null || !libro.Estatus)
            {
                TempData["AlertMessage"] = "Libro no disponible para préstamo.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }
            var lend = new PrestamoLibro { BibliotecaLibroID = libro.ID, FechaPrestamo = ToUnixTimestamp(DateTime.Now) };
            ViewBag.Usuarios = new SelectList(_context.Usuarios, "ID", "Nombre");
            ViewBag.TipoPrestamos = new SelectList(_context.TipoPrestamos, "ID", "Nombre");
            TempData["ShowModal"] = "true";
            TempData["ModalTitle"] = "Prestar Libro";
            TempData["LendModel"] = lend;
            return View("Libros", libros);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LendBook(PrestamoLibro prestamo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var libros = _context.Libros.Include(l => l.Categoria).ToList();
                    ViewBag.Usuarios = new SelectList(_context.Usuarios, "ID", "Nombre");
                    ViewBag.TipoPrestamos = new SelectList(_context.TipoPrestamos, "ID", "Nombre");
                    TempData["ShowModal"] = "true";
                    TempData["ModalTitle"] = "Prestar Libro";
                    TempData["LendModel"] = prestamo;
                    return View("Libros", libros);
                }

                var libro = _context.Libros.Find(prestamo.BibliotecaLibroID);
                if (libro == null || !libro.Estatus)
                {
                    TempData["AlertMessage"] = "Libro no disponible.";
                    TempData["AlertType"] = "error";
                    return RedirectToAction("Libros");
                }

                var tipoPrestamo = _context.TipoPrestamos.Find(prestamo.TipoPrestamoID);
                prestamo.FechaEntrega = ToUnixTimestamp(DateTime.Now.AddDays(tipoPrestamo.Tiempo));
                libro.Estatus = false;
                _context.PrestamoLibros.Add(prestamo);
                _context.SaveChanges();
                TempData["AlertMessage"] = "Préstamo autorizado correctamente.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Libros");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error al prestar el libro: {ex.Message}";
                TempData["AlertType"] = "error";
                var libros = _context.Libros.Include(l => l.Categoria).ToList();
                ViewBag.Usuarios = new SelectList(_context.Usuarios, "ID", "Nombre");
                ViewBag.TipoPrestamos = new SelectList(_context.TipoPrestamos, "ID", "Nombre");
                TempData["ShowModal"] = "true";
                TempData["ModalTitle"] = "Prestar Libro";
                TempData["LendModel"] = prestamo;
                return View("Libros", libros);
            }
        }

        public ActionResult ReturnBook(int id)
        {
            var libro = _context.Libros.Find(id);
            if (libro == null || libro.Estatus)
            {
                TempData["AlertMessage"] = "Libro no prestado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }
            var prestamo = _context.PrestamoLibros.FirstOrDefault(p => p.BibliotecaLibroID == id && p.FechaEntregaReal == null);
            if (prestamo == null)
            {
                TempData["AlertMessage"] = "Préstamo no encontrado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }
            prestamo.FechaEntregaReal = ToUnixTimestamp(DateTime.Now);
            prestamo.Multa = 0; // Lógica de multa aquí
            libro.Estatus = true;
            _context.SaveChanges();
            TempData["AlertMessage"] = "Libro devuelto correctamente.";
            TempData["AlertType"] = "success";
            return RedirectToAction("Libros");
        }

 

        private static int ToUnixTimestamp(DateTime dateTime)
        {
            return (int)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
using Biblioteca.Models;
using Biblioteca.Models.ModelosDTO;
using Biblioteca.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Biblioteca.Controllers
{
    public class ClienteController : Controller
    {
        private readonly Context db = new Context();

        [HttpGet]
        public ActionResult Index()
        {
            var libros = db.Libros
                .Include(l => l.Categoria)
                .ToList();

            var viewModel = new ClienteDashboardViewModel
            {
                LibrosEstado = libros.Select(l => new LibroEstadoDTO
                {
                    ISBN = l.ISBN,
                    NumeroEjemplar = l.NumeroEjemplar,
                    Estatus = l.Estatus
                }).ToList(),
                TotalLibros = libros.Count,
                LibrosDisponibles = libros.Count(l => l.Estatus),
                LibrosPrestados = libros.Count(l => !l.Estatus)
            };

            ViewBag.Categorias = new SelectList(db.Categorias, "ID", "Nombre");
            ViewBag.Temas = new SelectList(db.Temas, "ID", "Nombre");
            ViewBag.LastNumeroAdquisicion = db.Libros
                .Select(l => l.NumeroAdquisicion)
                .DefaultIfEmpty(0)
                .Max();

            return View(viewModel);
        }

        public ActionResult Usuarios()
        {
            IEnumerable<Usuario> usuarios;
            try
            {
                usuarios = db.Usuarios.Include(u => u.RolUsuario).ToList();
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
            ViewBag.Roles = new SelectList(db.RolUsuarios, "ID", "Nombre");
            return View(usuarios);
        }

        public ActionResult CrearUsuario()
        {
            var usuarios = db.Usuarios.Include(u => u.RolUsuario).ToList();
            ViewBag.Roles = new SelectList(db.RolUsuarios, "ID", "Nombre");
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
                    var usuarios = db.Usuarios.Include(u => u.RolUsuario).ToList();
                    ViewBag.Roles = new SelectList(db.RolUsuarios, "ID", "Nombre");
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
                    var usuarios = db.Usuarios.Include(u => u.RolUsuario).ToList();
                    ViewBag.Roles = new SelectList(db.RolUsuarios, "ID", "Nombre");
                    TempData["ShowModal"] = "true";
                    TempData["ModalTitle"] = "Crear Usuario";
                    TempData["UserModel"] = usuario;
                    return View("Usuarios", usuarios);
                }

                usuario.BibliotecaID = Convert.ToInt32(Session["BibliotecaID"]);
                db.Usuarios.Add(usuario);
                db.SaveChanges();
                TempData["AlertMessage"] = "Usuario registrado correctamente.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Usuarios");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error al registrar el usuario: {ex.Message}";
                TempData["AlertType"] = "error";
                var usuarios = db.Usuarios.Include(u => u.RolUsuario).ToList();
                ViewBag.Roles = new SelectList(db.RolUsuarios, "ID", "Nombre");
                TempData["ShowModal"] = "true";
                TempData["ModalTitle"] = "Crear Usuario";
                TempData["UserModel"] = usuario;
                return View("Usuarios", usuarios);
            }
        }

        public ActionResult EditarUsuario(int id)
        {
            var usuarios = db.Usuarios.Include(u => u.RolUsuario).ToList();
            var usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                TempData["AlertMessage"] = "Usuario no encontrado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Usuarios");
            }
            ViewBag.Roles = new SelectList(db.RolUsuarios, "ID", "Nombre");
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
                    var usuarios = db.Usuarios.Include(u => u.RolUsuario).ToList();
                    ViewBag.Roles = new SelectList(db.RolUsuarios, "ID", "Nombre");
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

                var existingUsuario = db.Usuarios.Find(usuario.ID);
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

                db.Entry(existingUsuario).State = EntityState.Modified;
                db.SaveChanges();
                TempData["AlertMessage"] = "Usuario editado correctamente.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Usuarios");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error al editar el usuario: {ex.Message}";
                TempData["AlertType"] = "error";
                var usuarios = db.Usuarios.Include(u => u.RolUsuario).ToList();
                ViewBag.Roles = new SelectList(db.RolUsuarios, "ID", "Nombre");
                TempData["ShowModal"] = "true";
                TempData["ModalTitle"] = "Editar Usuario";
                TempData["UserModel"] = usuario;
                return View("Usuarios", usuarios);
            }
        }

        public ActionResult DeleteUser(int id)
        {
            var usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                TempData["AlertMessage"] = "Usuario no encontrado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Usuarios");
            }
            try
            {
                db.Usuarios.Remove(usuario);
                db.SaveChanges();
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

        [HttpGet]
        public ActionResult Libros()
        {
            var libros = db.Libros
                .Include(l => l.Categoria)
                .ToList();

            var viewModels = libros.Select(l => new BibliotecaLibroViewModel
            {
                ID = l.ID,
                ISBN = l.ISBN,
                NumeroEjemplar = l.NumeroEjemplar,
                Clasificacion = l.Clasificacion,
                Estatus = l.Estatus,
                Autor = l.Autor ?? "Desconocido",
                NumeroAdquisicion = l.NumeroAdquisicion,
                CategoriaNombre = l.Categoria?.Nombre ?? "Sin categoría",
                PrestamoActivoId = null
            }).ToList();

            ViewBag.TotalLibros = viewModels.Count;
            ViewBag.LibrosDisponibles = viewModels.Count(vm => vm.Estatus);
            ViewBag.LibrosPrestados = viewModels.Count(vm => !vm.Estatus);
            ViewBag.Categorias = new SelectList(db.Categorias, "ID", "Nombre");
            ViewBag.Temas = new SelectList(db.Temas, "ID", "Nombre");
            ViewBag.LastNumeroAdquisicion = db.Libros
                .Select(l => l.NumeroAdquisicion)
                .DefaultIfEmpty(0)
                .Max();

            return View(viewModels);
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

                if (bookData.NombresAutor == null || !bookData.NombresAutor.Any())
                {
                    bookData.NombresAutor = bookData.Contributors?.Where(c => c.Role?.ToLower().Contains("author") ?? false)
                        .Select(c => c.Name).ToList() ?? new List<string> { "Desconocido" };
                }

                if (string.IsNullOrEmpty(bookData.Imagen) && bookData.CoverId.HasValue)
                {
                    bookData.Imagen = $"http://covers.openlibrary.org/b/id/{bookData.CoverId}-M.jpg";
                }
                else if (string.IsNullOrEmpty(bookData.Imagen))
                {
                    bookData.Imagen = "/Content/images/default-book-cover.jpg";
                }

                Debug.WriteLine($"Respuesta API - ISBN: {ISBN}, Imagen: {bookData.Imagen ?? "No disponible"}");

                int ultimoEjemplar = db.Libros
                    .Where(l => l.ISBN == ISBN)
                    .Select(l => l.NumeroEjemplar)
                    .DefaultIfEmpty(0)
                    .Max();

                var libro = new Libro
                {
                    ISBN = ISBN,
                    NumeroEjemplar = ultimoEjemplar + 1,
                    Autor = string.Join(", ", bookData.NombresAutor),
                    NumeroAdquisicion = db.Libros
                        .Where(l => l.ISBN == ISBN)
                        .Select(l => l.NumeroAdquisicion)
                        .DefaultIfEmpty(0)
                        .Max() + 1
                };

                var serialized = JsonConvert.SerializeObject(bookData);
                ViewData["ApiData"] = serialized;
                Session["ApiDataJson"] = serialized;
                TempData["ApiDataJson"] = serialized;
                TempData["BookModel"] = libro;

                TempData["ShowModal"] = "true";
                TempData["ModalTitle"] = "Agregar Libro";
                ViewBag.Autores = libro.Autor;
            }
            else
            {
                Debug.WriteLine($"Respuesta API - ISBN: {ISBN}, No se encontraron libros.");
                TempData["AlertMessage"] = "No se encontró el libro en la API.";
                TempData["AlertType"] = "warning";
                TempData["ShowModal"] = "true";
                TempData["ModalTitle"] = "Agregar Libro";
                TempData["BookModel"] = new Libro();
                ViewData["ApiData"] = null;
            }

            var libros = db.Libros
                .Include(l => l.Categoria)
                .ToList();

            var viewModels = libros.Select(l => new BibliotecaLibroViewModel
            {
                ID = l.ID,
                ISBN = l.ISBN,
                NumeroEjemplar = l.NumeroEjemplar,
                Clasificacion = l.Clasificacion,
                Estatus = l.Estatus,
                Autor = l.Autor ?? "Desconocido",
                NumeroAdquisicion = l.NumeroAdquisicion,
                CategoriaNombre = l.Categoria?.Nombre ?? "Sin categoría",
                PrestamoActivoId = null
            }).ToList();

            ViewBag.Categorias = new SelectList(db.Categorias, "ID", "Nombre");
            ViewBag.Temas = new SelectList(db.Temas, "ID", "Nombre");
            ViewBag.LastNumeroAdquisicion = db.Libros
                .Where(l => l.ISBN == ISBN)
                .Select(l => l.NumeroAdquisicion)
                .DefaultIfEmpty(0)
                .Max();

            return View("Libros", viewModels);
        }

        [HttpGet]
        public ActionResult CreateBook()
        {
            var libros = db.Libros.Include(l => l.Categoria).ToList();
            ViewBag.Categorias = new SelectList(db.Categorias, "ID", "Nombre");
            ViewBag.Temas = new SelectList(db.Temas, "ID", "Nombre");
            TempData["ShowModal"] = "true";
            TempData["ModalTitle"] = "Agregar Libro";
            TempData["BookModel"] = new Libro();
            ViewData["ApiData"] = null;
            return View("Libros", libros);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBook(Libro libro, int CantidadEjemplares = 1, int TemaID = 0, string Autor = null)
        {
            try
            {
                if (!ModelState.IsValid || CantidadEjemplares < 1)
                {
                    TempData["AlertMessage"] = "Ingrese una cantidad válida de ejemplares.";
                    TempData["AlertType"] = "error";
                    return RedirectToAction("Libros");
                }

                int ultimoEjemplar = db.Libros
                    .Where(l => l.ISBN == libro.ISBN)
                    .Select(l => l.NumeroEjemplar)
                    .DefaultIfEmpty(0)
                    .Max();

                int baseNumeroAdquisicion = db.Libros
                    .Where(l => l.ISBN == libro.ISBN)
                    .Select(l => l.NumeroAdquisicion)
                    .DefaultIfEmpty(0)
                    .Max();

                string autoresStr = Autor ?? "Desconocido";
                if (TempData["ApiDataJson"] != null)
                {
                    var apiData = JsonConvert.DeserializeObject<LibroInformacionDTO>((string)TempData["ApiDataJson"]);
                    if (apiData?.NombresAutor != null && apiData.NombresAutor.Any())
                    {
                        autoresStr = string.Join(", ", apiData.NombresAutor);
                    }
                }

                string temaNombre = TemaID > 0 ? db.Temas.FirstOrDefault(t => t.ID == TemaID)?.Nombre : null;

                for (int i = 1; i <= CantidadEjemplares; i++)
                {
                    var newLibro = new Libro
                    {
                        ISBN = libro.ISBN,
                        NumeroEjemplar = ultimoEjemplar + i,
                        Clasificacion = libro.Clasificacion ?? GenerateClasificacion(libro, ultimoEjemplar + i, temaNombre: temaNombre),
                        Estatus = true,
                        CategoriaID = libro.CategoriaID,
                        Autor = autoresStr,
                        NumeroAdquisicion = baseNumeroAdquisicion + i
                    };
                    db.Libros.Add(newLibro);
                    db.SaveChanges();

                    var bibliotecaLibro = new BibliotecaLibro
                    {
                        LibroID = newLibro.ID,
                        BibliotecaID = Convert.ToInt32(Session["BibliotecaID"]),
                        Estatus = true
                    };
                    db.BibliotecaLibros.Add(bibliotecaLibro);
                }

                db.SaveChanges();

                TempData["AlertMessage"] = $"{CantidadEjemplares} ejemplar(es) registrado(s) correctamente.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Libros");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error al registrar el libro: {ex.Message}";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }
        }

        private string GenerateClasificacion(Libro libro, int ejemplar, int anio = 0, string temaNombre = null)
        {
            var categoriaClave = db.Categorias.FirstOrDefault(c => c.ID == libro.CategoriaID)?.Clave ?? "000";
            var tema = !string.IsNullOrEmpty(temaNombre)
                ? (temaNombre.Length >= 3 ? temaNombre.Substring(0, 3) : temaNombre)
                : "GEN";
            if (anio == 0) anio = DateTime.Now.Year;
            return $"{categoriaClave} - {tema} - {anio} - {ejemplar}";
        }

        [HttpGet]
        public JsonResult GetTemasByCategoria(int categoriaId)
        {
            var temas = db.Temas
                .Where(t => t.CategoriaID == categoriaId)
                .Select(t => new { t.ID, t.Nombre })
                .OrderBy(t => t.Nombre)
                .ToList();
            return Json(temas, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLastNumeroAdquisicionByISBN(string isbn)
        {
            var lastNumeroAdquisicion = db.Libros
                .Where(l => l.ISBN == isbn)
                .Select(l => l.NumeroAdquisicion)
                .DefaultIfEmpty(0)
                .Max();
            return Json(new { lastNumeroAdquisicion }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditBook(int id)
        {
            var libro = db.Libros.Find(id);
            if (libro == null)
            {
                TempData["AlertMessage"] = "Libro no encontrado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }

            var viewModel = new BibliotecaLibroViewModel
            {
                ID = libro.ID,
                ISBN = libro.ISBN,
                NumeroEjemplar = libro.NumeroEjemplar,
                Clasificacion = libro.Clasificacion,
                Estatus = libro.Estatus,
                Autor = libro.Autor ?? "Desconocido",
                NumeroAdquisicion = libro.NumeroAdquisicion,
                CategoriaNombre = libro.Categoria?.Nombre ?? "Sin categoría",
                PrestamoActivoId = null
            };

            ViewBag.Categorias = new SelectList(db.Categorias, "ID", "Nombre", libro.CategoriaID);
            ViewBag.Temas = new SelectList(db.Temas, "ID", "Nombre");
            return View("EditBook", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBook(Libro libro)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var libros = db.Libros.Include(l => l.Categoria).ToList();
                    ViewBag.Categorias = new SelectList(db.Categorias, "ID", "Nombre");
                    ViewBag.Temas = new SelectList(db.Temas, "ID", "Nombre");
                    TempData["ShowModal"] = "true";
                    TempData["ModalTitle"] = "Editar Libro";
                    TempData["BookModel"] = libro;
                    return View("Libros", libros);
                }

                var existingLibro = db.Libros.Find(libro.ID);
                if (existingLibro == null)
                {
                    TempData["AlertMessage"] = "Libro no encontrado.";
                    TempData["AlertType"] = "error";
                    return RedirectToAction("Libros");
                }

                existingLibro.ISBN = libro.ISBN;
                existingLibro.NumeroEjemplar = libro.NumeroEjemplar;
                existingLibro.Clasificacion = libro.Clasificacion;
                existingLibro.CategoriaID = libro.CategoriaID;
                existingLibro.Estatus = libro.Estatus;
                existingLibro.Autor = libro.Autor;
                existingLibro.NumeroAdquisicion = libro.NumeroAdquisicion;

                db.Entry(existingLibro).State = EntityState.Modified;
                db.SaveChanges();
                TempData["AlertMessage"] = "Libro editado correctamente.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Libros");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error al editar el libro: {ex.Message}";
                TempData["AlertType"] = "error";
                var libros = db.Libros.Include(l => l.Categoria).ToList();
                ViewBag.Categorias = new SelectList(db.Categorias, "ID", "Nombre");
                ViewBag.Temas = new SelectList(db.Temas, "ID", "Nombre");
                TempData["ShowModal"] = "true";
                TempData["ModalTitle"] = "Editar Libro";
                TempData["BookModel"] = libro;
                return View("Libros", libros);
            }
        }

        public ActionResult DeleteBook(int id)
        {
            var libro = db.Libros.Find(id);
            if (libro == null)
            {
                TempData["AlertMessage"] = "Libro no encontrado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }
            try
            {
                db.Libros.Remove(libro);
                db.SaveChanges();
                TempData["AlertMessage"] = "Libro eliminado correctamente.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Libros", new { refresh = true });
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error al eliminar el libro: {ex.Message}";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }
        }

        [HttpGet]
        public ActionResult LendBook(int id)
        {
            var bibliotecaLibro = db.BibliotecaLibros
                .Include(bl => bl.Libro)
                .FirstOrDefault(bl => bl.LibroID == id && bl.Libro.Estatus);

            if (bibliotecaLibro == null)
            {
                TempData["AlertMessage"] = "Libro no disponible.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }

            return RedirectToAction("AssignLoan", new { id = bibliotecaLibro.ID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LendBook(PrestamoLibro prestamo)
        {
            if (!ModelState.IsValid) return View("_PrestamoLibroForm", prestamo);

            var bibliotecaLibro = db.BibliotecaLibros
                .Include(bl => bl.Libro)
                .FirstOrDefault(bl => bl.ID == prestamo.BibliotecaLibroID);

            if (bibliotecaLibro == null || !bibliotecaLibro.Estatus)
            {
                TempData["AlertMessage"] = "El libro ya está prestado o no existe.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }

            var tipoPrestamo = db.TipoPrestamos.Find(prestamo.TipoPrestamoID);
            if (tipoPrestamo == null)
            {
                TempData["AlertMessage"] = "Tipo de préstamo no válido.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }

            prestamo.FechaPrestamo = ToUnixTimestamp(DateTime.Now);
            prestamo.FechaEntrega = ToUnixTimestamp(DateTime.Now.AddDays(tipoPrestamo.Tiempo));
            prestamo.FechaEntregaReal = 0;
            prestamo.Multa = 0;
            prestamo.Observacion = string.Empty;

            db.PrestamoLibros.Add(prestamo);
            bibliotecaLibro.Estatus = false;
            db.Entry(bibliotecaLibro).State = EntityState.Modified;

            db.SaveChanges();

            TempData["AlertMessage"] = "Libro prestado exitosamente.";
            TempData["AlertType"] = "success";
            return RedirectToAction("Libros");
        }

        [HttpGet]
        public ActionResult AssignLoan(int id)
        {
            ViewBag.Usuarios = new SelectList(db.Usuarios, "ID", "Nombre");
            ViewBag.TipoPrestamos = new SelectList(db.TipoPrestamos, "ID", "Nombre");

            var bibliotecaLibro = db.BibliotecaLibros
                .Include(bl => bl.Libro)
                .FirstOrDefault(bl => bl.ID == id);

            if (bibliotecaLibro == null)
            {
                TempData["AlertMessage"] = "Libro no encontrado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }

            var model = new PrestamoLibro
            {
                BibliotecaLibroID = bibliotecaLibro.ID,
                ClienteID = Session["UsuarioID"] != null ? (int)Session["UsuarioID"] : 0
            };

            return View("_PrestamoLibroForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignLoan(PrestamoLibro prestamo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Usuarios = new SelectList(db.Usuarios, "ID", "Nombre", prestamo.UsuarioID);
                ViewBag.TipoPrestamos = new SelectList(db.TipoPrestamos, "ID", "Nombre", prestamo.TipoPrestamoID);
                return View("_PrestamoLibroForm", prestamo);
            }

            var bibliotecaLibro = db.BibliotecaLibros
                .Include(bl => bl.Libro)
                .FirstOrDefault(bl => bl.ID == prestamo.BibliotecaLibroID);

            if (bibliotecaLibro == null || bibliotecaLibro.Libro == null)
            {
                TempData["AlertMessage"] = "El libro no existe.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }

            if (!bibliotecaLibro.Libro.Estatus)
            {
                TempData["AlertMessage"] = "El libro ya está prestado.";
                TempData["AlertType"] = "warning";
                return RedirectToAction("Libros");
            }

            var tipoPrestamo = db.TipoPrestamos.Find(prestamo.TipoPrestamoID);
            if (tipoPrestamo == null)
            {
                TempData["AlertMessage"] = "Tipo de préstamo no válido.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Libros");
            }

            prestamo.FechaPrestamo = ToUnixTimestamp(DateTime.Now);
            prestamo.FechaEntrega = ToUnixTimestamp(DateTime.Now.AddDays(tipoPrestamo.Tiempo));
            prestamo.FechaEntregaReal = 0;
            prestamo.Multa = 0;
            prestamo.Observacion = string.Empty;

            db.PrestamoLibros.Add(prestamo);
            bibliotecaLibro.Libro.Estatus = false;
            db.Entry(bibliotecaLibro.Libro).State = EntityState.Modified;

            db.SaveChanges();

            TempData["AlertMessage"] = "Libro prestado exitosamente.";
            TempData["AlertType"] = "success";
            return RedirectToAction("Libros");
        }

        [HttpGet]
        public ActionResult ReturnBook(int id)
        {
            var prestamo = db.PrestamoLibros
                .Include(p => p.BibliotecaLibro.Libro)
                .Include(p => p.BibliotecaLibro.Libro.Categoria)
                .Include(p => p.Usuario)
                .FirstOrDefault(p => p.ID == id);

            if (prestamo == null || prestamo.BibliotecaLibro == null)
            {
                TempData["AlertMessage"] = "Préstamo no encontrado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("GestionPrestamos");
            }

            prestamo.Devuelto = true;
            prestamo.FechaEntregaReal = ToUnixTimestamp(DateTime.Now);

            if (prestamo.BibliotecaLibro.Libro != null)
            {
                prestamo.BibliotecaLibro.Libro.Estatus = true;
                db.Entry(prestamo.BibliotecaLibro.Libro).State = EntityState.Modified;
            }

            db.Entry(prestamo).State = EntityState.Modified;
            db.SaveChanges();

            TempData["AlertMessage"] = "Libro devuelto correctamente.";
            TempData["AlertType"] = "success";
            return RedirectToAction("GestionPrestamos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReturnBook(PrestamoLibro prestamo)
        {
            var prestamoDb = db.PrestamoLibros
                .Include(p => p.BibliotecaLibro.Libro)
                .Include(p => p.BibliotecaLibro.Libro.Categoria)
                .Include(p => p.Usuario)
                .FirstOrDefault(p => p.ID == prestamo.ID);

            if (prestamoDb == null)
            {
                TempData["AlertMessage"] = "Préstamo no encontrado.";
                TempData["AlertType"] = "error";
                return RedirectToAction("GestionPrestamos");
            }

            if (prestamoDb.Devuelto)
            {
                TempData["AlertMessage"] = "El libro ya ha sido devuelto.";
                TempData["AlertType"] = "warning";
                return RedirectToAction("GestionPrestamos");
            }

            int ahora = ToUnixTimestamp(DateTime.Now);
            int tarifaMulta = 5;
            int multa = 0;
            string observacion = (Request.Form["Observacion"] ?? "").Trim();

            if (ahora > prestamoDb.FechaEntrega)
            {
                int diasRetraso = (int)((ahora - prestamoDb.FechaEntrega) / 86400);
                multa = diasRetraso * tarifaMulta;
                observacion += $" Multa aplicada por {diasRetraso} día(s) de retraso.";
            }

            prestamoDb.Devuelto = true;
            prestamoDb.FechaEntregaReal = ahora;
            prestamoDb.Multa = multa;
            prestamoDb.Observacion = observacion;

            if (prestamoDb.BibliotecaLibro?.Libro != null)
            {
                prestamoDb.BibliotecaLibro.Libro.Estatus = true;
                db.Entry(prestamoDb.BibliotecaLibro.Libro).State = EntityState.Modified;
            }

            db.Entry(prestamoDb).State = EntityState.Modified;
            db.SaveChanges();

            TempData["AlertMessage"] = "Libro devuelto exitosamente.";
            TempData["AlertType"] = "success";
            return RedirectToAction("GestionPrestamos");
        }

        public ActionResult GestionPrestamos()
        {
            var prestamosActivos = db.PrestamoLibros
                .Include(p => p.BibliotecaLibro.Libro)
                .Include(p => p.BibliotecaLibro.Libro.Categoria)
                .Include(p => p.Usuario)
                .Where(p => !p.Devuelto && p.BibliotecaLibro != null && p.BibliotecaLibro.Libro != null)
                .ToList();

            var prestamosDevueltos = db.PrestamoLibros
                .Include(p => p.BibliotecaLibro.Libro)
                .Include(p => p.BibliotecaLibro.Libro.Categoria)
                .Include(p => p.Usuario)
                .Where(p => p.Devuelto && p.BibliotecaLibro != null && p.BibliotecaLibro.Libro != null)
                .ToList();

            ViewBag.PrestamosActivos = prestamosActivos;
            ViewBag.PrestamosDevueltos = prestamosDevueltos;

            return View();
        }

        private static int ToUnixTimestamp(DateTime dateTime)
        {
            return (int)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
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
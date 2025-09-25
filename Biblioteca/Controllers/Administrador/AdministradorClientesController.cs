using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Biblioteca.Models;

namespace Biblioteca.Controllers.Administrador
{
    public class AdministradorClientesController : Controller
    {
        private Context db = new Context();
        public ActionResult Index(string busqueda)
        {
            var clientes = db.Clientes
                     .Include(c => c.RolClientes)
                     .Include(c => c.Biblioteca)
                     .Where(u => u.Estatus == true)
                     .ToList();
            if (!string.IsNullOrEmpty(busqueda))
            {
                clientes = (List<Models.Cliente>)clientes.Where(b => b.Nombre.Contains(busqueda)).ToList();
            }
            ViewBag.Bibliotecas = db.Bibliotecas.ToList();
            ViewBag.Roles = db.RolClientes.ToList();
            return View(clientes);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Models.Cliente clientes)
        {
            if (ModelState.IsValid)
            {
                db.Clientes.Add(clientes);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Administrador");
            }

            return View(clientes);
        }


        // GET: Clientes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.Cliente cliente = await db.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }

            // Pasamos las listas para los select
            ViewBag.RolID = new SelectList(db.RolClientes, "ID", "Nombre", cliente.RolID);
            ViewBag.BibliotecaID = new SelectList(db.Bibliotecas, "ID", "Nombre", cliente.BibliotecaID);

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Models.Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                // Marcamos la entidad como modificada
                var clienteDb = await db.Clientes.FindAsync(cliente.ID);

                if (clienteDb == null)
                    return HttpNotFound();

                if (cliente.Contrasena != null)
                    clienteDb.Contrasena = cliente.Contrasena;

                clienteDb.Nombre = cliente.Nombre;
                clienteDb.Correo = cliente.Correo;
                clienteDb.RolID = cliente.RolID;
                clienteDb.BibliotecaID = cliente.BibliotecaID;

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Si el modelo no es válido, recargamos los selects
            ViewBag.RolID = new SelectList(db.RolClientes, "ID", "Nombre", cliente.RolID);
            ViewBag.BibliotecaID = new SelectList(db.Bibliotecas, "ID", "Nombre", cliente.BibliotecaID);

            return View(cliente);
        }


        public async Task<ActionResult> Delete(int? id)
        {
            Models.Cliente cliente = await db.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }
            cliente.Estatus = false;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}
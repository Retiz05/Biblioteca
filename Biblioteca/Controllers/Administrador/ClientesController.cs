//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Entity;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Net;
//using System.Web;
//using System.Web.Mvc;
//using Biblioteca.Models;

//namespace Biblioteca.Controllers.Administrador
//{
//    public class ClientesController : Controller
//    {
//        private Context db = new Context();

//        public async Task<ActionResult> Index()
//        {
//            return RedirectToAction("Index", "Administrador");
//        }

//        public async Task<ActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }
//            Cliente cliente = await db.Clientes.FindAsync(id);
//            if (cliente == null)
//            {
//                return HttpNotFound();
//            }
//            return View(cliente);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> Create(Cliente cliente)
//        {
//            if (ModelState.IsValid)
//            {
//                db.Clientes.Add(cliente);
//                await db.SaveChangesAsync();
//                return RedirectToAction("Index", "Administrador");
//            }

//            ViewBag.BibliotecaID = new SelectList(db.Bibliotecas, "ID", "Nombre", cliente.BibliotecaID);
//            ViewBag.RolID = new SelectList(db.RolClientes, "ID", "Nombre", cliente.RolID);
//            return View("./Index", cliente);
//        }

//        public async Task<ActionResult> Edit(int? id)
//        {
//            if (id == null)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }
//            Cliente cliente = await db.Clientes.FindAsync(id);
//            if (cliente == null)
//            {
//                return HttpNotFound();
//            }
//            ViewBag.BibliotecaID = new SelectList(db.Bibliotecas, "ID", "Nombre", cliente.BibliotecaID);
//            ViewBag.RolID = new SelectList(db.RolClientes, "ID", "Nombre", cliente.RolID);
//            return View(cliente);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> Edit([Bind(Include = "ID,Nombre,Correo,Contraseña,RolID,BibliotecaID")] Cliente cliente)
//        {
//            if (ModelState.IsValid)
//            {
//                db.Entry(cliente).State = EntityState.Modified;
//                await db.SaveChangesAsync();
//                return RedirectToAction("Index");
//            }
//            ViewBag.BibliotecaID = new SelectList(db.Bibliotecas, "ID", "Nombre", cliente.BibliotecaID);
//            ViewBag.RolID = new SelectList(db.RolClientes, "ID", "Nombre", cliente.RolID);
//            return View(cliente);
//        }

//        public async Task<ActionResult> Delete(int? id)
//        {
//            if (id == null)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }
//            Cliente cliente = await db.Clientes.FindAsync(id);
//            if (cliente == null)
//            {
//                return HttpNotFound();
//            }
//            return View(cliente);
//        }

//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> DeleteConfirmed(int id)
//        {
//            Cliente cliente = await db.Clientes.FindAsync(id);
//            db.Clientes.Remove(cliente);
//            await db.SaveChangesAsync();
//            return RedirectToAction("Index");
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                db.Dispose();
//            }
//            base.Dispose(disposing);
//        }
//    }
//}

using Biblioteca.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static System.Collections.Specialized.BitVector32;

namespace Biblioteca.Services
{
    public class Autenticacion: AuthorizeAttribute
    {
        private readonly string[] _rolesPermitidos;


        public Autenticacion(params string[] roles)
        {
            _rolesPermitidos = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Verificar si la sesión existe y contiene la clave "Rol"
            if (httpContext.Session?["Rol"] == null)
            {
                return false;
            }

            var rol = httpContext.Session["Rol"].ToString();
            return _rolesPermitidos.Contains(rol);
        }

    }


}
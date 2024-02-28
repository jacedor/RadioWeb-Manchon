using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RadioWeb.Repositories
{
    public class FiltrosRepository
    {
        private readonly UsersDBContext _context;
        public FiltrosRepository(UsersDBContext context)
        {
            _context = context;
        }

        public void Guardar(int usuarioId, string controlador, string objeto, string valor, string tipo = "int")
        {

            var usuario = _context.UCCADUSER.Single(u => u.IDUSER == usuarioId);
            UCCADPERM oPermiso = _context.UCCADPERM
                .SingleOrDefault(u => u.IDUSER == usuario.IDUSER && u.MODULO.ToUpper() == "RADIOWEB"
                && u.OBJNAME.ToUpper() == String.Concat(controlador.ToUpper(), ".", objeto.ToUpper()));
            if (oPermiso == null)
            {
                oPermiso = new UCCADPERM();
                oPermiso.IDUSER = usuario.IDUSER;
                oPermiso.MODULO = "RadioWeb";
                oPermiso.OBJNAME = String.Concat(controlador.ToUpper(), ".", objeto.ToUpper());
                if (tipo == "int")
                {
                    oPermiso.ESTADO = int.Parse(valor);
                }
                else
                {
                    oPermiso.ESTADO = -1;
                    oPermiso.VALOR = valor;
                }

                _context.UCCADPERM.Add(oPermiso);
            }
            else
            {
                if (tipo == "int")
                {
                    oPermiso.ESTADO = int.Parse(valor);
                }
                else
                {
                    oPermiso.ESTADO = -999;
                    oPermiso.VALOR = valor;
                }
                _context.Entry(oPermiso).State = EntityState.Modified;
            }
            _context.SaveChanges();
        }

        //public List<VWFiltros> Obtener(int usuarioId, string controlador)
        //{
        //    List<VWFiltros> oResult = _context.UCCADPERM
        //        .Where(u => u.IDUSER == usuarioId && u.MODULO.ToUpper() == "RADIOWEB"
        //        && u.OBJNAME.ToUpper() == String.Concat(controlador.ToUpper(), ".", objeto.ToUpper()));



        //    var usuario = _context.UCCADUSER.Single(u => u.IDUSER == usuarioId);
        //    UCCADPERM oPermiso = _context.UCCADPERM
        //        .SingleOrDefault(u => u.IDUSER == usuario.IDUSER && u.MODULO.ToUpper() == "RADIOWEB"
        //        && u.OBJNAME.ToUpper() == String.Concat(controlador.ToUpper(), ".", objeto.ToUpper()));
        //    if (oPermiso == null)
        //    {
        //        oPermiso = new UCCADPERM();
        //        oPermiso.IDUSER = usuario.IDUSER;
        //        oPermiso.MODULO = "RadioWeb";
        //        oPermiso.OBJNAME = String.Concat(controlador.ToUpper(), ".", objeto.ToUpper());
        //        oPermiso.ESTADO = valor;
        //        _context.UCCADPERM.Add(oPermiso);
        //    }
        //    else
        //    {
        //        oPermiso.ESTADO = valor;
        //        _context.Entry(oPermiso).State = EntityState.Modified;
        //    }



        //}
    }
}
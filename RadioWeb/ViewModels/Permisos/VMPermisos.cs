using ADPM.Common.Extensions;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Models.VidSigner;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels.Permisos
{
    public class VMPermisos
    {

        public VMPermisos()
        {
            using (UsersDBContext context = new UsersDBContext())
            {
                Roles = context.UCCADUSER
                    .Where(u => u.TIPO == "P")
                    .ToList();
            }
        }
        public VMPermisos(int role)
        {
            UsersDBContext context = new UsersDBContext();
            
                Roles = context.UCCADUSER
                    .Where(u => u.TIPO == "P")
                    .ToList();

            Permisos = (from r in context.UCCADPERM
                       where (r.IDUSER == role && r.MODULO=="RadioWeb")
                       select r).ToList();
 
            var todosLosPermisosExistentes = context.UCCADPERM
                    .Where(p=> p.MODULO== "RadioWebPermiso" || 
                    (p.MODULO == "RadioWeb" &&
                     (p.OBJNAME.Contains(".Edit") 
                    || p.OBJNAME.Contains(".Read") 
                    || p.OBJNAME.Contains(".Delete")
                    || p.OBJNAME.Contains(".Create"))))
                    
                    .DistinctBy(p => p.OBJNAME)
                    .ToList();

               
                //en una variable tenemos todos los permisos que por defecto tendran el valor de permitido
                List<UCCADPERM> oResult = new List<UCCADPERM>();
                for (int i = 0; i < todosLosPermisosExistentes.Count; i++)
                {
                    var permiso = Permisos.Find(p => p.OBJNAME == todosLosPermisosExistentes[i].OBJNAME);
                    oResult.Add(new UCCADPERM
                    {
                        MODULO = todosLosPermisosExistentes[i].MODULO,
                        OBJNAME = todosLosPermisosExistentes[i].OBJNAME,
                        ESTADO = ((permiso == null ) ? 1 : permiso.ESTADO),
                        IDUSER = role                      
                    });
                    
                }
                this.Permisos = oResult;


            
            this.IOR_ROLE = role;

            this.ESTADOS = new List<ESTADOSPERMISO>();

            this.ESTADOS.Add(new ESTADOSPERMISO { ID = 2, TEXT = "SOLO LECTURA" });
            this.ESTADOS.Add(new ESTADOSPERMISO { ID = 1, TEXT = "PERMITIR" });
            this.ESTADOS.Add(new ESTADOSPERMISO { ID = 0, TEXT = "DENEGAR" });
            context.Dispose();    
                
           

        }

        
        public List<USUARIO> Roles { get; set; }
        public List<UCCADPERM> Permisos { get; set; }

        [Display(Name ="Perfil de Seguridad")]
        public int IOR_ROLE { get; set; }

        
        public List<ESTADOSPERMISO> ESTADOS { get; set; }

    }
    public class ESTADOSPERMISO
    {
        public int ID { get; set; }
        public string TEXT { get; set; }
    }
}
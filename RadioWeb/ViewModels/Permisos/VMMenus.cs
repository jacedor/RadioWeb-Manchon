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
    public class VMMenus
    {

        public VMMenus()
        {
            using (UsersDBContext context = new UsersDBContext())
            {
                Roles = context.UCCADUSER
                    .Where(u => u.TIPO == "P")
                    .OrderBy(u => u.LOGIN).ToList();
            }
        }
        public VMMenus(int role)
        {
            UsersDBContext context = new UsersDBContext();

            Roles = context.UCCADUSER
                .Where(u => u.TIPO == "P")
                .OrderBy(u => u.LOGIN).ToList();
            TodosLosMenus = context.UCCADMENU.ToList();


            var menusPemitidos = context.UCCADMENUPERM
                                .Where(u => u.IDUSER == role)
                                .ToList();


            //en una variable tenemos todos los permisos que por defecto tendran el valor de permitido
            List<UCCADMENU> oResult = new List<UCCADMENU>();
            for (int i = 0; i < TodosLosMenus.Count; i++)
            {
                var menu = menusPemitidos.SingleOrDefault(p => p.IDMENU == TodosLosMenus[i].ID);
                if (menu != null)
                {
                    oResult.Add(new UCCADMENU
                    {
                        DESCRIPCION = TodosLosMenus[i].DESCRIPCION,
                        ID = TodosLosMenus[i].ID,
                        ICONO = TodosLosMenus[i].ICONO,
                        IDPARENT = TodosLosMenus[i].IDPARENT,
                        ORDEN = TodosLosMenus[i].ORDEN,
                        URL = TodosLosMenus[i].URL
                    });
                }


            }
            this.MenusPermitidos = oResult;



            this.IOR_ROLE = role;

            //this.ESTADOS = new List<ESTADOSPERMISO>();

            //this.ESTADOS.Add(new ESTADOSPERMISO { ID = 1, TEXT = "PERMITIR" });
            //this.ESTADOS.Add(new ESTADOSPERMISO { ID = 0, TEXT = "DENEGAR" });
            context.Dispose();



        }


        public List<USUARIO> Roles { get; set; }
        public List<UCCADMENU> TodosLosMenus { get; set; }
        public List<UCCADMENU> MenusPermitidos { get; set; }
        [Display(Name = "Perfil de Seguridad")]
        public int IOR_ROLE { get; set; }


        public List<ESTADOSPERMISO> ESTADOS { get; set; }

    }

}
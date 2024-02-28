using ADPM.Common;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.ViewModels.Permisos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    
    public class PermisosController : Controller
    {
        // GET: Permisos
        public ActionResult Index()
        {
            VMPermisos oModel = new VMPermisos();
            oModel.IOR_ROLE = 1;
            return View(oModel);
        }
       
        [HttpPost]       
        public ActionResult Index(int IOR_ROLE)
        {            
            VMPermisos oModel = new VMPermisos(IOR_ROLE);
            return PartialView("_List",oModel);
        }

        public ActionResult AccesoNoPermitido()
        {
            return View("404");
        }


        [HttpPost]        
        public ActionResult Menus(int IOR_ROLE2, List<string> menusselect=null)
        {
           
            if (menusselect!=null)
            {
                using (UsersDBContext context = new UsersDBContext())
                {
                    var menusaBorrar = context.UCCADMENUPERM.Where(u => u.IDUSER == IOR_ROLE2);
                    foreach (var item in menusaBorrar)
                    {
                        context.UCCADMENUPERM.Remove(item);
                        context.SaveChanges();
                    }
                 
                    foreach (var item2 in menusselect)
                    {
                        UCCADMENUPERM omenu = new UCCADMENUPERM
                        {
                            IDMENU = int.Parse(item2),
                            IDUSER = IOR_ROLE2
                        };
                        context.UCCADMENUPERM.Add(omenu);
                    }
                    context.SaveChanges();
                }
            }
            VMMenus oModel = new VMMenus(IOR_ROLE2);
            return PartialView("_MenuAdmin", oModel);
        }

        [HttpPost]
        public ActionResult EditarCampo(string name, string pk, string value)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                string module = pk.Split(':')[0].ToString();
                string idUser = pk.Split(':')[2].ToString();
                string objname = pk.Split(':')[1].ToString();
                oConexion.Open();
                string updateStament = "UPDATE OR INSERT INTO UCCADPERM ( IDUSER, MODULO, OBJNAME, ESTADO)";
                updateStament += " VALUES (" + idUser + "," + module.QuotedString() + "," + objname.QuotedString();
                updateStament += "," + value + ") MATCHING  (IDUSER,OBJNAME,MODULO)";                
                oCommand = new FbCommand(updateStament, oConexion);
                oCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Threading.Thread.Sleep(1);

            }
            finally
            {

                if (oConexion.State == System.Data.ConnectionState.Open)
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }

                }

            }

            return new HttpStatusCodeResult(200);
        }
    }
}

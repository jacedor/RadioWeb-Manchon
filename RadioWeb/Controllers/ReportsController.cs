using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FastReport.Web;
using FirebirdSql.Data.FirebirdClient;
using System.IO;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System.Web.UI.WebControls;

namespace RadioWeb.Controllers
{
    public class ReportsController : Controller
    {
        private WebReport webReport = new WebReport();
        // GET: /Reports/

        public ActionResult Exploracion(int oid)
        {
            WebReport webReport = new WebReport();
            EXPLORACION oExplo = ExploracionRepositorio.Obtener(oid);
            
            string reportPath = Path.Combine(Server.MapPath("~/Reports"), "ExploracionPRI.frx");

            if (oExplo.APARATO.OWNER == 14 || (oExplo.APARATO.OWNER == 22) || (oExplo.IOR_TIPOEXPLORACION == 144)
                || (oExplo.IOR_TIPOEXPLORACION == 702) || (oExplo.IOR_TIPOEXPLORACION == 319) || (oExplo.IOR_TIPOEXPLORACION == 877)
                || (oExplo.IOR_TIPOEXPLORACION == 149) || (oExplo.IOR_TIPOEXPLORACION == 707) || (oExplo.IOR_TIPOEXPLORACION == 445)
                || (oExplo.IOR_TIPOEXPLORACION == 465))
            {
                reportPath = Path.Combine(Server.MapPath("~/Reports"), "ExploracionPRI_MAMOS.frx");
            }
            webReport.Report.RegisterData(ExploracionRepositorio.ImprimirFichaPri(oid));
            webReport.ReportFile = reportPath;
            webReport.CurrentTab.Name = "Exploración Privados";
            webReport.Width = Unit.Percentage(100);
            webReport.Height = Unit.Percentage(100);
            webReport.ToolbarIconsStyle = ToolbarIconsStyle.Black;
            webReport.ShowExports = true;
            webReport.ShowPrint = true;
            //webReport.p
            //ViewBag.WebReport = webReport;
            return webReport.PrintHtml();
            // FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            // oConexion.Open();
            // string reportPath = Path.Combine(Server.MapPath("~/Reports"), "ExploracionPRI.frx");

            // string query = "SELECT e.oid,p.trac,P.POLIZA, P.PACIENTE, P.FECHAN, P.SEXO, P.PROFESION, P.DNI,P.COD_PAC,P.EMAIL,E.IOR_PACIENTE, d.cod_fil as Aparato, a.FIL as Explo,a.DES_FIL, e.fecha, e.hora,e.ior_colegiado,t.cod_grup ,text.TEXTO,m.codmut,m.nombre ";
            // query += " from exploracion e left join paciente p on p.oid=e.ior_paciente join gaparatos t on t.oid=e.ior_grupo left join TEXTOS text on text.OWNER= e.oid left join mutuas m on m.oid=e.ior_entidadpagadora left join daparatos d on d.oid=e.ior_aparato";
            // query += " left join aparatos a on a.oid=e.ior_tipoexploracion left join colegiados c on c.oid=e.ior_colegiado where  e.OID = " + oid;
            // FbDataAdapter da = new FbDataAdapter(query, oConexion);
            // da.SelectCommand.ExecuteNonQuery();
            // System.Data.DataSet dataSet = new System.Data.DataSet();
            // da.Fill(dataSet, "Exploracion");
            // query = "Select * from Direccion where owner=" + dataSet.Tables["Exploracion"].Rows[0]["IOR_PACIENTE"].ToString();
            // da.SelectCommand = new FbCommand(query, oConexion);
            // da.Fill(dataSet, "DIRECCION");
            // query = "Select * from Telefono where owner=" + dataSet.Tables["Exploracion"].Rows[0]["IOR_PACIENTE"].ToString();
            // da.SelectCommand = new FbCommand(query, oConexion);
            // da.Fill(dataSet, "Telefono");
            // query = "Select * from Colegiados where oid=" + dataSet.Tables["Exploracion"].Rows[0]["IOR_COLEGIADO"].ToString();
            // da.SelectCommand = new FbCommand(query, oConexion);
            // da.Fill(dataSet, "Colegiados");
            // query = "SELECT E.HORA, M.COD_MUT, D.COD_FIL,E.OID,A.FIL, A.DES_FIL FROM EXPLORACION E JOIN APARATOS A ON A.OID=E.IOR_TIPOEXPLORACION JOIN DAPARATOS D ON D.OID=E.IOR_APARATO JOIN MUTUAS M ON M.OID=E.IOR_ENTIDADPAGADORA WHERE E.IOR_PACIENTE=" + dataSet.Tables["Exploracion"].Rows[0]["IOR_PACIENTE"].ToString() + " AND E.FECHA='" + DateTime.Parse(dataSet.Tables["Exploracion"].Rows[0]["FECHA"].ToString()).ToString("MM-dd-yyyy") + "' AND E.OID<>" + dataSet.Tables["Exploracion"].Rows[0]["OID"].ToString();

            // da.SelectCommand = new FbCommand(query, oConexion);
            // da.Fill(dataSet, "ExplosAsociadas");

            // query = "select OID,NOMBRE,DIRECCION,CP,CIUTAT,TELEFONO from CENTROS where OID =1";
            // da.SelectCommand = new FbCommand(query, oConexion);
            // da.Fill(dataSet, "Centros");

            // query = "select nombre from empresas where oid=4";
            // da.SelectCommand = new FbCommand(query, oConexion);
            // da.Fill(dataSet, "Empresa");

            // webReport.Report.RegisterData(dataSet);
            // webReport.ReportFile=reportPath;
            // webReport.CurrentTab.Name = "Exploración Privados";
            // webReport.Width = 1024;// Unit.Percentage(100);
            // webReport.Height = 768;// Unit.Percentage(100); 
            // webReport.ToolbarIconsStyle = ToolbarIconsStyle.Black;

            // webReport.ShowExports = true;
            // webReport.ShowPrint = false;

            // oConexion.Close();

            // ViewBag.WebReport = webReport;
            //// webReport.PrintHtml();

            //return PartialView("Report");
        }

        //
        // GET: /Reports/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Reports/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Reports/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Reports/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Reports/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Reports/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Reports/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

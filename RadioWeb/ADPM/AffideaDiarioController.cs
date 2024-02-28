using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FirebirdSql.Data.FirebirdClient;
using OfficeOpenXml;
using RadioWeb.Models.Repos;

namespace RadioWeb.ADPM
{
    public class AffideaDiarioController : ApiController
    {

        public AffideaDiarioController()
        {
        }

        public void Get()
        {
            String fecha = DateTime.Now.ToString("yyyy-MM-dd");
            generarFichero(fecha);
        }

        public void Get(String f)
        {
            generarFichero(f);
        }


        public void generarFichero(String fecha)
        {
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Worksheets.Add("Worksheet1");

                FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
                oConexion.Open();
                string querySelect = "select * from AFFIDEA_DIARIO('" + fecha + "');";
                FbCommand oCommand = new FbCommand(querySelect, oConexion);

                try
                {
                    FbDataReader oReader = oCommand.ExecuteReader();

                    var worksheet = excel.Workbook.Worksheets["Worksheet1"];

                    worksheet.Cells.LoadFromDataReader(oReader, true);

                    WebConfigRepositorio oConfig = new WebConfigRepositorio();
                    String ruta = oConfig.ObtenerValor("DIR_AFFIDEA_DIARIO");
                   
                    FileInfo excelFile = new FileInfo(@ruta);
                    excel.SaveAs(excelFile);
                }
                catch (Exception e)
                {
                    throw;
                }
                finally
                {
                    if (oConexion.State == System.Data.ConnectionState.Open)
                    {
                        oConexion.Close();
                        if (oCommand != null)
                        {
                            oCommand.Dispose();
                        };
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FirebirdSql.Data.FirebirdClient;
using System.IO;
using System.Windows.Forms;
using RadioWeb.Models;
using RadioWeb.Models.Repos;

namespace RadioWeb.Utils
{
    public class DataBase
    {
        public static Dictionary<string, string> Tratamientos()
        {
            Dictionary<string, string> Tratamientos = new Dictionary<string, string>();
            EmpresaRepositorio oEmpresaRepo = new EmpresaRepositorio();
            var empresa = oEmpresaRepo.Obtener(4);

            if (empresa.NOMBRE.Contains("DELFOS"))
            {
                Tratamientos.Add("1", "Sr");
                Tratamientos.Add("5", "Sra.");
            }
            else {
                Tratamientos.Add("1", "Sr");
                Tratamientos.Add("2", "Sr. D.");
                Tratamientos.Add("3", "Dr.");
                Tratamientos.Add("4", "Srta.");
                Tratamientos.Add("5", "Sra.");
                Tratamientos.Add("6", "Sra. Dña.");
                Tratamientos.Add("7", "Dra.");
                Tratamientos.Add("8", "Niño");
                Tratamientos.Add("9", "Niña");

            }
            return Tratamientos;
        }

       

        public static Dictionary<int, string> TiposPlantillas()
        {
            Dictionary<int, string> tipos = new Dictionary<int, string>();
            tipos.Add(1, "Consentimiento");
            tipos.Add(2, "LOPD");
            tipos.Add(3, "Recogida");
            tipos.Add(-1, "Informes");

            return tipos;
        }


        public static List<PIVOTTABLE> pivotTables()
        {
            List<PIVOTTABLE> result = new List<PIVOTTABLE>();
            FbConnection oConnection = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;

            if (oConnection.State == System.Data.ConnectionState.Closed)
            {
                oConnection.Open();
            }
            try
            {
                oCommand = new FbCommand("select * from pivotTable where BORRADO='F'", oConnection);
                FbDataReader oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    PIVOTTABLE oPivotTable = new PIVOTTABLE();
                    oPivotTable.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oPivotTable.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oPivotTable.DESCRIPCION = DataBase.GetStringFromReader(oReader, "DESCRIPCION");

                    result.Add(oPivotTable);// = DataBase.GetStringFromReader(oReader, "COD");
                }
                //oCommand.Dispose();
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                if (oConnection.State == System.Data.ConnectionState.Open)
                {
                    oCommand.Dispose();
                    oConnection.Dispose();
                    oConnection.Close();
                }

            }




            return result;
        }

        public static string DefaultIdioma()
        {
            string result = "";
            FbConnection oConnection = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
         
            if (oConnection.State == System.Data.ConnectionState.Closed)
            {
                oConnection.Open();
            }
            try
            {
                oCommand = new FbCommand("select * from idiomas where vers=1", oConnection);
                FbDataReader oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    result=DataBase.GetStringFromReader(oReader, "COD");
                }
                //oCommand.Dispose();
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                if (oConnection.State == System.Data.ConnectionState.Open)
                {
                    oCommand.Dispose();
                    oConnection.Dispose();
                    oConnection.Close();
                }

            }




            return result;
        }

        public static Dictionary<string, string> Idiomas()
        {
            Dictionary<string, string> Idiomas = new Dictionary<string, string>();

            FbConnection oConnection = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            var result = 0;
            if (oConnection.State == System.Data.ConnectionState.Closed)
            {
                oConnection.Open();
            }
            try
            {
                oCommand = new FbCommand("select * from idiomas", oConnection);
                FbDataReader oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    Idiomas.Add(DataBase.GetStringFromReader(oReader,"COD"), DataBase.GetStringFromReader(oReader, "IDIOMA"));
                }
                //oCommand.Dispose();
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                if (oConnection.State == System.Data.ConnectionState.Open)
                {
                    oCommand.Dispose();
                    oConnection.Dispose();
                    oConnection.Close();
                }

            }


            

            return Idiomas;
        }
        public static string CalcularEdad(DateTime birthday)
        {
            var birthdayTicks = birthday.Ticks;
            var twoYearsTicks = birthday.AddYears(2).Ticks;
            var NowTicks = DateTime.Now.Ticks;
            var moreThanTwoYearsOld = twoYearsTicks < NowTicks;
            DateTime age;
            try
            {
             age    = new DateTime(DateTime.Now.Subtract(birthday).Ticks);
            }
            catch (Exception)
            {

                return "";
            }       

            return moreThanTwoYearsOld ? (age.Year - 1).ToString() + " años" : (age.Year - 1 >= 1 ? age.Month + 12 : age.Month).ToString() + " meses";
        }

        public static int GetIntFromReaderString(FbDataReader oReader, string Column)
        {
            if (oReader[Column] != System.DBNull.Value)
            {
                return int.Parse(oReader[Column].ToString());
            }
            else
            {
                return -1;
            }

        }

        public static int GetIntFromReader(FbDataReader oReader, string Column)
        {
            if (oReader[Column] != System.DBNull.Value)
            {
                return (int)oReader[Column];
            }
            else
            {
                return -1;
            }

        }

        public static char GetCharFromReader(FbDataReader oReader, string Column)
        {
            if (oReader[Column] != System.DBNull.Value)
            {
                return (char)oReader[Column];
            }
            else
            {
                return System.Char.MinValue;
            }

        }

        public static DateTime? GetDateTimeFromReader(FbDataReader oReader, string Column)
        {
            if (oReader[Column] != System.DBNull.Value)
            {
                return (DateTime)oReader[Column];
            }
            else
            {
                return null;
            }

        }

        public static float GetFloatFromReader(FbDataReader oReader, string Column)
        {
            if (oReader[Column] != System.DBNull.Value)
            {
                return (float)oReader[Column];
            }
            else
            {
                return -1;
            }

        }

        public static double GetDoubleFromReader(FbDataReader oReader, string Column)
        {
            if (oReader[Column] != System.DBNull.Value)
            {
                return (double)oReader[Column];
            }
            else
            {
                return -1;
            }

        }

        public static string QuotedString(string valor)
        {

            string retorno = "";
            if (!String.IsNullOrEmpty(valor))
            {
                retorno = "'" + valor.Replace("'", "''") + "'";
            }
            else
            {
                retorno = "''";
            }

            return retorno;


        }

        public static string GetStringFromReader(FbDataReader oReader, string Column)
        {
            if (oReader[Column] != System.DBNull.Value)
            {
                return (string)oReader[Column];
            }
            else
            {
                return "";
            }

        }

        public static bool GetBoolFromReader(FbDataReader oReader, string Column)
        {
            if (oReader[Column] != System.DBNull.Value)
            {
                return (oReader[Column].ToString() == "T" ? true : false);
                
            }
            else
            {
                return false;
            }

        }

        public static decimal GetDecimalFromReader(FbDataReader oReader, string Column)
        {
            if (oReader[Column] != System.DBNull.Value)
            {
                return (decimal)oReader[Column];
            }
            else
            {
                return 0;
            }

        }
        public static short GetShortFromReader(FbDataReader oReader, string Column)
        {
            if (oReader[Column] != System.DBNull.Value)
            {
                return (short)oReader[Column];
            }
            else
            {
                return -1;
            }

        }

        public static string GetStringFromByte(FbDataReader oReader, string Column)
        {

            if (!oReader[Column].Equals(System.DBNull.Value))
            {
                System.Text.ASCIIEncoding codificador = new System.Text.ASCIIEncoding();
                string result= codificador.GetString((byte[])oReader[Column]);
               result= result.Replace(@"\r\","<br/>");
               return result;
            }
            else
            {
                return "";
            }



        }


        public static int EjecutarNonQuery(FbConnection oConnection, string query)
        {

            if (oConnection.State == System.Data.ConnectionState.Closed)
            {
                oConnection.Open();
            }
            FbCommand oCommand = new FbCommand(query, oConnection);
             return oCommand.ExecuteNonQuery();
           
        }
        public static FbDataReader EjecutarQuery(FbConnection oConnection, string query)
        {

            if (oConnection.State == System.Data.ConnectionState.Closed)
            {
                oConnection.Open();
            }
            FbCommand oCommand = new FbCommand(query, oConnection);
            FbDataReader oReader = oCommand.ExecuteReader();
            //oCommand.Dispose();
            return oReader;
        }

        public static int GenUid()
        {
            FbConnection oConnection = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            var result =0;
            if (oConnection.State == System.Data.ConnectionState.Closed)
            {
                oConnection.Open();
            }
            try
            {
                 oCommand = new FbCommand("select gen_id(GENUID,1) from RDB$DATABASE", oConnection);
                 result = int.Parse(oCommand.ExecuteScalar().ToString());
                //oCommand.Dispose();
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                if (oConnection.State == System.Data.ConnectionState.Open)
                {
                    oCommand.Dispose();
                    oConnection.Dispose();
                    oConnection.Close();
                }

            }
            
            return result;
        }

        public static int EjecutarCuenta(FbConnection oConnection, string query)
        {
            if (oConnection.State == System.Data.ConnectionState.Closed)
            {
                oConnection.Open();
            }
            FbCommand oCommand = new FbCommand(query, oConnection);
            int oResult = (int)oCommand.ExecuteScalar();
            oConnection.Close();
            return oResult;
        }


        public class GridDataResult
        {
            public int TotalRowCount { get; set; }
            public int TotalPageCount { get; set; }
            public string colName { get; set; }
            public string sortOrder { get; set; }
            public object GridData { get; set; }
        }

        public static string convertRtf(string texto)
        {
            var box = new RichTextBox();            
            box.Rtf = texto;
            return box.Text;
            
        }

        public static string convertRtfToPlainText(string texto, bool inlineCss = true)
        {
            try
            {
                SautinSoft.RtfToHtml r = new SautinSoft.RtfToHtml();
                r.OutputFormat = SautinSoft.RtfToHtml.eOutputFormat.Text;
                r.Encoding = SautinSoft.RtfToHtml.eEncoding.ISO_8859_8;
                r.TextStyle.InlineCSS = inlineCss;
               
                string ConvertedFromRTF = r.ConvertString(texto);
                ConvertedFromRTF = ConvertedFromRTF.Replace("The trial version can convert no more than 10000 symbols!Please activate your version.", "");
                 ConvertedFromRTF = ConvertedFromRTF.Replace("The trial version can convert no more than 10000 symbols! Please activate your version.", "");
                ConvertedFromRTF = ConvertedFromRTF.Replace("The trial version of RTF-to-HTML DLL .Net can convert up to 10000 symbols.", "");
                ConvertedFromRTF = ConvertedFromRTF.Replace("Get the full featured version!", "");
                ConvertedFromRTF = ConvertedFromRTF.Replace("Times New Roman", "Verdana");
                ConvertedFromRTF = ConvertedFromRTF.Replace("The trial version can convert no more than 10000 symbols!", "");
                ConvertedFromRTF = ConvertedFromRTF.Replace("Please activate your version.", "");
                return ConvertedFromRTF;
            }
            catch (Exception)
            {

                return texto;
            }

        }

        public static string convertRtfToHtml(string texto,bool inlineCss=true) {
            try
            {
                SautinSoft.RtfToHtml r = new SautinSoft.RtfToHtml();
                r.OutputFormat = SautinSoft.RtfToHtml.eOutputFormat.HTML_5;
                r.Encoding = SautinSoft.RtfToHtml.eEncoding.Windows_1251;
                r.TextStyle.InlineCSS = inlineCss;
                r.ImageStyle.IncludeImageInHtml = false; //To save images inside HTML as binary data specify this property to 'true'    
                r.ImageStyle.ImageFolder = "/";
                r.ImageStyle.ImageSubFolder = "Images";
                r.ImageStyle.ImageFileName = "picture";
                string ConvertedFromRTF = r.ConvertString(texto);
                ConvertedFromRTF = ConvertedFromRTF.Replace("The trial version can convert no more than 10000 symbols!Please activate your version.","");
              // ConvertedFromRTF = ConvertedFromRTF.Replace("<p style=\"margin:0pt 0pt 0pt 0pt;line-height:normal;\"><span class=\"st1\">&nbsp;</span></p>", "");
              ConvertedFromRTF = ConvertedFromRTF.Replace("The trial version of RTF-to-HTML DLL .Net can convert up to 10000 symbols.", "");
                ConvertedFromRTF = ConvertedFromRTF.Replace("Get the full featured version!", "");
                ConvertedFromRTF = ConvertedFromRTF.Replace("Times New Roman", "Verdana");

                return ConvertedFromRTF;
            }
            catch (Exception)
            {

                return texto;
            }
           
        }

        public static int GetOIDAparatoFromDesc(string descAparato, List<DAPARATOS> oListaaparatos)
        {

            DAPARATOS oAparato = new DAPARATOS();
            oAparato = oListaaparatos.Find(delegate(DAPARATOS aparato)
           {
               return aparato.COD_FIL == descAparato;
           });

            return oAparato.OID;
        }

        public static string convertRtfToText(string texto)
        {
            try
            {

                System.Windows.Forms.RichTextBox rtBox = new System.Windows.Forms.RichTextBox();
                // Use the RichTextBox to convert the RTF code to plain text.
                rtBox.Rtf = texto.Replace("\n", "<br>");
                string ConvertedFromRTF = rtBox.Text;
                ConvertedFromRTF = ConvertedFromRTF.Replace("The trial version can convert no more than 10000 symbols!Please activate your version.", "");
                ConvertedFromRTF = ConvertedFromRTF.Replace("The trial version can convert no more than 10000 symbols! Please activate your version.", "");
                ConvertedFromRTF = ConvertedFromRTF.Replace("The trial version of RTF-to-HTML DLL .Net can convert up to 10000 symbols.", "");
                ConvertedFromRTF = ConvertedFromRTF.Replace("Get the full featured version!", "");
                ConvertedFromRTF = ConvertedFromRTF.Replace("Times New Roman", "Verdana");
                ConvertedFromRTF = ConvertedFromRTF.Replace("The trial version can convert no more than 10000 symbols!", "");
                ConvertedFromRTF = ConvertedFromRTF.Replace("Please activate your version.", "");
                return ConvertedFromRTF;
            }
            catch (Exception)
            {

                return texto;
            }

        }

    }



    public static class DropDownList<T>
    {
        public static SelectList LoadItems(IList<T> collection, string value, string text)
        {
            return new SelectList(collection, value, text);
        }
    }

    
    public class SHA1
    {
        public static string Encode(string value)
        {
            var hash = System.Security.Cryptography.SHA1.Create();
            var encoder = new System.Text.ASCIIEncoding();
            var combined = encoder.GetBytes(value ?? "");
            return BitConverter.ToString(hash.ComputeHash(combined)).ToLower().Replace("-", "");
        }
    }

    public class AutoCompleteAvanzado
    {
   
        public string value { get; set; }
        public string label { get; set; }
        public string desc { get; set; }
        public string icon { get; set; }
    }

}
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Repos
{
    public class HorasAnulasRepositorio
    {
        public static  List<HORASANULADAS>  Obtener(string FECHA, int IOR_APARATO, string HORAHORARIO)
        {
            List<HORASANULADAS> lHorasAnuladas = new List<HORASANULADAS>();
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            oConexion.Open();
            string queryHoras_Anuladas = "select IOR_APARATO, FECHA, HORA,COMENTARIO from HORASANULADAS where FECHA='" + DateTime.Parse(FECHA).ToString("yyyy-MM-dd HH:mm:ss") + "' and IOR_APARATO=" + IOR_APARATO + " and hora=" + DataBase.QuotedString(HORAHORARIO) + " order by HORA ";
            oCommand = new FbCommand(queryHoras_Anuladas, oConexion);
           
            try
            {
                FbDataReader oReaderHorasAnuladas = oCommand.ExecuteReader();
                
                //rellenamos la lista de horas anuladas
                while (oReaderHorasAnuladas.Read())
                {
                    HORASANULADAS oHorasAnuladas = new HORASANULADAS();
                    oHorasAnuladas.COMENTARIO = DataBase.GetStringFromReader(oReaderHorasAnuladas, "COMENTARIO");
                    oHorasAnuladas.HORA = DataBase.GetStringFromReader(oReaderHorasAnuladas, "HORA");

                    lHorasAnuladas.Add(oHorasAnuladas);
                }
                
            }
            catch (Exception)
            {


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

            return lHorasAnuladas;
        }

        public static int Insertar(string FECHA, int IOR_APARATO, string HORAHORARIO,string Comentario)
        {
            FbCommand oCommand = null;
            FbCommand myCommand = null;
            List<HORASANULADAS> lHorasAnuladas = new List<HORASANULADAS>();
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            string queryHoras_Anuladas = "select OID, IOR_APARATO, FECHA, HORA,COMENTARIO from HORASANULADAS where FECHA='" + DateTime.Parse(FECHA).ToString("yyyy-MM-dd HH:mm:ss") + "' and IOR_APARATO=" + IOR_APARATO + " and hora=" + DataBase.QuotedString(HORAHORARIO) + " order by HORA ";
            oCommand = new FbCommand(queryHoras_Anuladas, oConexion);
            oConexion.Open();
            FbDataReader oReaderHorasAnuladas = oCommand.ExecuteReader();


            HORASANULADAS oHoraAnulada = new HORASANULADAS();
            //rellenamos la lista de horas anuladas
            while (oReaderHorasAnuladas.Read())
            {
                oHoraAnulada.OID = DataBase.GetIntFromReader(oReaderHorasAnuladas, "OID");
                oHoraAnulada.COMENTARIO = DataBase.GetStringFromReader(oReaderHorasAnuladas, "COMENTARIO");
                oHoraAnulada.HORA = DataBase.GetStringFromReader(oReaderHorasAnuladas, "HORA");
            }
            oReaderHorasAnuladas.Close();
            oCommand.Dispose();
            oConexion.Close();
            string query = "";
            if (oHoraAnulada.OID == 0)
            {
                query = "Insert into horasanuladas (OID, IOR_APARATO,FECHA,HORA,COMENTARIO) VALUES (gen_id(GENUID,1)," + IOR_APARATO + "," + DataBase.QuotedString(DateTime.Parse(FECHA).ToString("yyyy-MM-dd HH:mm:ss")) + "," + DataBase.QuotedString(HORAHORARIO) + ",'>" + Comentario.ToUpper() + "')";
            }
            else
            {

                query = "delete from horasanuladas where oid= " + oHoraAnulada.OID;
            }

            try
            {
                oConexion.Open();
                myCommand = new FbCommand();
                myCommand.Connection = oConexion;
                myCommand.CommandText = query;
                return (int)myCommand.ExecuteNonQuery();
            }
            catch (Exception)
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
                    }
                    if (myCommand != null)
                    {
                        myCommand.Dispose();
                    }
               

                }
            }


        }
    }
}
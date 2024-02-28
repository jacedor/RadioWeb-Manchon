using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{

    public class FestivosRepositorio
    {


        public static List<FESTIVOS> Obtener(int cod_fil, string FechaInicial, string FechaFinal)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            //Creamos una lista de festivos de este mes marcados en la base de datos
            List<FESTIVOS> lFestivos = new List<FESTIVOS>();

           
            string queryFestivos = "Select * from festivos where ior_empresa=4 and ";

            if (cod_fil != -1)
            {
                queryFestivos += "ior_daparatos = -1 or ";
            }

            queryFestivos += "ior_daparatos = " + cod_fil.ToString() + " and festivo between '" + FechaInicial + "' AND '" + FechaFinal + "'";
            oConexion.Open();
            FbCommand oCommandFestivos = new FbCommand(queryFestivos, oConexion);
            FbDataReader oReaderFestivos = oCommandFestivos.ExecuteReader();
            try
            {
                while (oReaderFestivos.Read())
                {
                    lFestivos.Add(new FESTIVOS
                    {
                        OID = DataBase.GetIntFromReader(oReaderFestivos, "OID"),
                        BORRADO = DataBase.GetStringFromReader(oReaderFestivos, "BORRADO"),
                        CANAL = DataBase.GetStringFromReader(oReaderFestivos, "CANAL"),
                        FESTIVO = DataBase.GetDateTimeFromReader(oReaderFestivos, "FESTIVO")
                    });
                }
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
                    if (oCommandFestivos != null)
                    {
                        oCommandFestivos.Dispose();
                    }
                }

            }

            
            oConexion.Close();


            
            return lFestivos;
        }

        public static List<FESTIVOS> Obtener(int oidAparato, string FechaInicial)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            //Creamos una lista de festivos de este mes marcados en la base de datos
            List<FESTIVOS> lFestivos = new List<FESTIVOS>();
            string queryFestivos = "Select * from festivos where ior_empresa=4 and ( ior_daparatos= -1 or ";
            queryFestivos += "ior_daparatos = " + oidAparato.ToString() + ") and festivo = '" + FechaInicial + "'";
            oConexion.Open();
            FbCommand oCommandFestivos = new FbCommand(queryFestivos, oConexion);
            FbDataReader oReaderFestivos = oCommandFestivos.ExecuteReader();

            while (oReaderFestivos.Read())
            {
                lFestivos.Add(new FESTIVOS
                {
                    OID = DataBase.GetIntFromReader(oReaderFestivos, "OID"),
                    BORRADO = DataBase.GetStringFromReader(oReaderFestivos, "BORRADO"),
                    CANAL = DataBase.GetStringFromReader(oReaderFestivos, "CANAL"),
                    FESTIVO = DataBase.GetDateTimeFromReader(oReaderFestivos, "FESTIVO")
                });
            }
            oReaderFestivos.Close();


            oConexion.Close();
            if (oCommandFestivos != null)
            {
                oCommandFestivos.Dispose();
            }
          

            return lFestivos;
        }

    }
}
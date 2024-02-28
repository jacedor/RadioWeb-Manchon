using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public class CarteleraRepositorio
    {
        public static IList<CARTELERA> ObtenerAnuncios(string Grupo)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            try
            {
                oConexion.Open();
                FbCommand oCuenta = new FbCommand("Select FIRST 5 USERNAME,FECHA,HORA,GRUPO,TEXTO FROM CARTELERA WHERE GRUPO='" + Grupo + "' ORDER BY FECHA DESC", oConexion);

                //FbCommand oCuenta = new FbCommand("Select FIRST 5 USERNAME,FECHA,HORA,GRUPO,TEXTO FROM CARTELERA WHERE DESRIPCION='" + Admin + "' ORDER BY FECHA DESC", oConexion);

                FbDataReader oReader = oCuenta.ExecuteReader();
                List<CARTELERA> oPacientesList = new List<CARTELERA>();

                while (oReader.Read())
                {
                    CARTELERA oAnuncio = new CARTELERA();
                    oAnuncio.USERNAME = oReader["USERNAME"].ToString();
                    oAnuncio.FECHA = (DateTime)oReader["FECHA"];
                    oAnuncio.HORA = oReader["HORA"].ToString();
                    oAnuncio.GRUPO = oReader["GRUPO"].ToString();
                    oAnuncio.TEXTO = oReader["TEXTO"].ToString();
                    oPacientesList.Add(oAnuncio);
                }
                if (oCuenta != null) { oCuenta.Dispose(); }
                return oPacientesList;
            
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
                    
                }
            }
            

        }

    }
}
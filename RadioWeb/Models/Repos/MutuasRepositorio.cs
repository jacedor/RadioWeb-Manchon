using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using System.Configuration;

namespace RadioWeb.Models.Repos
{
     
    public static class MutuasRepositorio
    {
        /// <summary>
        /// Devuelve una lista de mutuas cuyo owner se pasa como parametro
        /// </summary>   
        /// <param name="paciente">Oid de la mutua</param>
        public static MUTUAS Obtener(int oidMutua)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            try
            {
                oConexion.Open();
                //FbCommand oCommand = new FbCommand("select oid, m.NOMBRE,m.CODMUT, m.OWNER,m.IOR_CENTRAL, m.CID FROM MUTUAS m where m.OID=" + oidMutua, oConexion);
                FbCommand oCommand = new FbCommand("select oid, m.NOMBRE,m.CODMUT, m.OWNER,m.IOR_CENTRAL, m.CID, m.VERS FROM MUTUAS m where m.OID=" + oidMutua, oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();

                MUTUAS oMutua = new MUTUAS();

                while (oReader.Read())
                {
                    oMutua.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oMutua.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oMutua.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oMutua.NOMBRE = oReader["NOMBRE"].ToString();
                    oMutua.CODMUT = oReader["CODMUT"].ToString();
                    oMutua.IOR_CENTRAL = DataBase.GetIntFromReader(oReader, "IOR_CENTRAL");
                    //Añadimos la versión de la mútua para poder comprobar si es obsoleta o no. (0=Activa - 1=Obsoleta)
                    oMutua.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                }

                oCommand.Dispose();
                return oMutua;
            }
            catch (Exception)
            {
                return new MUTUAS();

            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)
                    oConexion.Close();
            }



        }

        public static List<VMMutuasCitaOnline> CitaOnlineList(int oidAparato)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            //APARATOS Aparato = AparatoRepositorio.Obtener(oidAparato);
            DAPARATOS Aparato = DaparatoRepositorio.Obtener(oidAparato);
            string GrupoAparto = GAparatoRepositorio.Obtener((int)Aparato.OWNER).COD_GRUP;
            FbCommand oCommand = null;
            try
            {
               
                oConexion.Open();
               oCommand = new FbCommand("select * FROM MUTUAS where   vers=0 AND CITAONLINE='T'  order by NOMBRE ", oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();

                List<VMMutuasCitaOnline> lMutua = new List<VMMutuasCitaOnline>();
                while (oReader.Read())
                {
                    VMMutuasCitaOnline oMutuas = new VMMutuasCitaOnline();
                    oMutuas.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oMutuas.CODMUT = DataBase.GetStringFromReader(oReader, "CODMUT");
                    oMutuas.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oMutuas.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oMutuas.IOR_CENTRAL = DataBase.GetIntFromReader(oReader, "IOR_CENTRAL");
                    oMutuas.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                    bool EsExcepcion = false;

                    if (GrupoAparto=="DEN" && (oMutuas.NOMBRE.ToUpper().Contains("ASSISTENCIA SANITARIA") || oMutuas.NOMBRE.ToUpper().Contains("ASISA")))
                    {
                           EsExcepcion = true;
                    }


                    if ((Aparato.CID == 1) &&  oMutuas.NOMBRE.ToUpper().Contains("SANITAS")) //TIBIDABO                    
                    {
                        EsExcepcion = true;
                    }
                    if ( (Aparato.CID != 3)  && (GrupoAparto=="MAM" || GrupoAparto =="RM") && 
                        (oMutuas.NOMBRE.ToUpper().Contains("AXA") )) //sino es can mora                    
                    {
                        EsExcepcion = true;
                    }
                    if ((Aparato.CID == 4) && oMutuas.NOMBRE.ToUpper().Contains("SANITAS")) //BALMES                
                    {
                        EsExcepcion = true;
                    }
                    
                    if ((Aparato.CID == 6) && (oMutuas.NOMBRE.ToUpper().Contains("ASSISTENCIA SANITARIA") || oMutuas.NOMBRE.ToUpper().Contains("CIGNA") )) //MERIDIANA
                    {
                        EsExcepcion = true;
                    }
                    /*if (!EsExcepcion)
                    {
                        lMutua.Add(oMutuas);
                    }                 
                    */
                    if (EsExcepcion)
                    {
                        oMutuas.mutuaOnlineDisponible = false;
                    }
                    else
                    {
                        oMutuas.mutuaOnlineDisponible = true;
                    }

                    lMutua.Add(oMutuas);

                }

                return lMutua;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)


                    oConexion.Close();
                if (oCommand != null)
                {
                    oCommand.Dispose();
                }
            }



        }

        public static List<MUTUAS> ListaPorCentroExterno(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            try
            {

                oConexion.Open();
                List<MUTUAS> lMutua = new List<MUTUAS>();
                using (var conn = new FbConnection())
                {
                    string query = "select * FROM MUTUAS where (ior_empresa=4 or ior_empresa is null)";

                    if (oid>0)
                    {
                        query += " and oid in (select ior_mutua from CE_MUTUAS where ior_centroexterno= " + oid + "  ) ";

                    }

                    if (ConfigurationManager.AppSettings["ComboMutuas"].ToUpper() == "NOMBRE")
                    {
                        query += " order by NOMBRE";
                    }
                    else
                    {
                        query += " order by CODMUT";
                    }

                    using (var cmd = new FbCommand(query, oConexion))
                    {

                        FbDataReader oReader = cmd.ExecuteReader();


                        while (oReader.Read())
                        {
                            MUTUAS oMutuas = new MUTUAS();
                            oMutuas.OID = DataBase.GetIntFromReader(oReader, "OID");
                            oMutuas.CODMUT = DataBase.GetStringFromReader(oReader, "CODMUT");
                            oMutuas.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                            oMutuas.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                            oMutuas.IOR_CENTRAL = DataBase.GetIntFromReader(oReader, "IOR_CENTRAL");
                            oMutuas.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                            lMutua.Add(oMutuas);

                        }
                    }
                }




                return lMutua;
            }
            catch (Exception ex)
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


        public static List<MUTUAS> Lista(bool VerObsoletas=true)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            try
            {
             
               oConexion.Open();
                List<MUTUAS> lMutua = new List<MUTUAS>();
                using (var conn = new FbConnection())
                {
                    string query = "select * FROM MUTUAS where (ior_empresa=4 or ior_empresa is null)";

                    if (!VerObsoletas)
                    {
                        query += " and VERS=0 ";
                    }

                    if (ConfigurationManager.AppSettings["ComboMutuas"].ToUpper() == "NOMBRE")
                    {
                        query += " order by NOMBRE";
                    }
                    else {
                        query += " order by CODMUT";
                    }

                  
                    using (var cmd = new FbCommand( query,oConexion))
                    {
                       
                        FbDataReader oReader = cmd.ExecuteReader();

         
                        while (oReader.Read())
                        {
                            MUTUAS oMutuas = new MUTUAS();
                            oMutuas.OID = DataBase.GetIntFromReader(oReader, "OID");
                            oMutuas.CODMUT = DataBase.GetStringFromReader(oReader, "CODMUT");
                            oMutuas.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                            oMutuas.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                            oMutuas.IOR_CENTRAL = DataBase.GetIntFromReader(oReader, "IOR_CENTRAL");
                            oMutuas.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                            lMutua.Add(oMutuas);

                        }
                    }
                }
              
               


                return lMutua;
            }
            catch (Exception ex)
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

        public static List<MUTUAS> Lista(string nombre)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            try
            {

                oConexion.Open();
                List<MUTUAS> lMutua = new List<MUTUAS>();
                using (var conn = new FbConnection())
                {
                    string query = "select * FROM MUTUAS where (ior_empresa=4 or ior_empresa is null) and VERS=0  and nombre like '" + nombre.ToUpper() + "%'";

                    

                    if (ConfigurationManager.AppSettings["ComboMutuas"].ToUpper() == "NOMBRE")
                    {
                        query += " order by NOMBRE";
                    }
                 


                    using (var cmd = new FbCommand(query, oConexion))
                    {

                        FbDataReader oReader = cmd.ExecuteReader();


                        while (oReader.Read())
                        {
                            MUTUAS oMutuas = new MUTUAS();
                            oMutuas.OID = DataBase.GetIntFromReader(oReader, "OID");
                            oMutuas.CODMUT = DataBase.GetStringFromReader(oReader, "CODMUT");
                            oMutuas.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                            oMutuas.NIF= DataBase.GetStringFromReader(oReader, "NIF");
                            oMutuas.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                            oMutuas.IOR_CENTRAL = DataBase.GetIntFromReader(oReader, "IOR_CENTRAL");
                            oMutuas.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                            oMutuas.DIRECCIONES = DireccionRepositorio.Obtener(oMutuas.OID);
                            lMutua.Add(oMutuas);

                        }
                    }
                }




                return lMutua;
            }
            catch (Exception ex)
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
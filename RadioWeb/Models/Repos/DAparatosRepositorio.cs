using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{

    public static class DaparatoRepositorio
    {
        public static DAPARATOS Obtener(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from daparatos where oid=" + oid, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<DAPARATOS> lAparato = new List<DAPARATOS>();
            DAPARATOS oAparato = new DAPARATOS();

            try
            {
                while (oReader.Read())
                {

                    oAparato.BORRADO = oReader["BORRADO"].ToString();
                    oAparato.CANAL = oReader["CANAL"].ToString();
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    if (DataBase.GetIntFromReader(oReader, "VERS") == 0)
                    {
                        oAparato.COD_FIL = "OBSOLETO" + " - " + DataBase.GetStringFromReader(oReader, "COD_FIL");
                    }
                    else
                    {
                        oAparato.COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL");

                    }

                    oAparato.DES_FIL = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    oAparato.IDENTIFICADOR = oReader["IDENTIFICADOR"].ToString();

                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.TOT_FIL = DataBase.GetIntFromReader(oReader, "TOT_FIL");
                    oAparato.USERNAME = oReader["USERNAME"].ToString();
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    oAparato.XEROX = DataBase.GetShortFromReader(oReader, "XEROX");

                    EmpresaRepositorio oEmpresaRepo = new EmpresaRepositorio();

                    oAparato.EMPRESA = oEmpresaRepo.Obtener((int)oReader["IOR_EMPRESA"]);

                    lAparato.Add(oAparato);
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
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }

                }

            }


            return oAparato;
        }

        //Obtener por la descripción del aparato COD_FIL
        public static DAPARATOS Obtener(string cod_fil)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            oConexion.Open();
            FbCommand oCommand = new FbCommand("select * from daparatos where cod_fil=" + DataBase.QuotedString(cod_fil), oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            List<DAPARATOS> lAparato = new List<DAPARATOS>();
            DAPARATOS oAparato = new DAPARATOS();
            try
            {
                while (oReader.Read())
                {
                    oAparato.BORRADO = oReader["BORRADO"].ToString();
                    oAparato.CANAL = oReader["CANAL"].ToString();
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    if (DataBase.GetIntFromReader(oReader, "VERS") == 0)
                    {
                        oAparato.COD_FIL = "OBSOLETO" + " - " + DataBase.GetStringFromReader(oReader, "COD_FIL");
                        oAparato.DES_FIL = "OBSOLETO" + " - " + DataBase.GetStringFromReader(oReader, "DES_FIL");
                    }
                    else
                    {
                        oAparato.COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL");
                        oAparato.DES_FIL = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    }
                    oAparato.IDENTIFICADOR = oReader["IDENTIFICADOR"].ToString();
                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.TOT_FIL = DataBase.GetIntFromReader(oReader, "TOT_FIL");
                    oAparato.USERNAME = oReader["USERNAME"].ToString();
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    oAparato.XEROX = DataBase.GetShortFromReader(oReader, "XEROX");
                    EmpresaRepositorio oEmpresaRepo = new EmpresaRepositorio();
                    oAparato.EMPRESA = oEmpresaRepo.Obtener((int)oReader["IOR_EMPRESA"]);
                    lAparato.Add(oAparato);
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
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }

                }

            }

            return oAparato;
        }

        public static List<DAPARATOS> EsAparatoComplejo(string Aparato)
        {

            List<DAPARATOS> oresult = new List<DAPARATOS>();

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = new FbCommand("select * FROM APARATOS_COMPLEJOS d where d.APARATO1=" + DataBase.QuotedString(Aparato), oConexion);
            oConexion.Open();
            FbDataReader oReader = oCommand.ExecuteReader();
            try
            {
                while (oReader.Read())
                {

                    for (int i = 2; i < 10; i++)
                    {
                        if (DataBase.GetStringFromReader(oReader, "APARATO" + i.ToString()) != "")
                        {
                            oresult.Add(DaparatoRepositorio.Obtener(
                                DataBase.GetStringFromReader(
                                    oReader, "APARATO" + i.ToString()
                                    ))
                                );
                        }
                        else
                        {
                            i = 10;
                        }
                    }

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
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }

                }

            }



            return oresult;

        }


        public static List<DAPARATOS> Lista()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string query = "select * from daparatos where IOR_EMPRESA=4 ";
            if (HttpContext.Current.User != null)
            {
                string test = HttpContext.Current.User.Identity.Name;
                var centroAsociado = UsuariosRepositorio.Obtener(HttpContext.Current.User.Identity.Name.ToString()).CENTROASOCIADO;
                if (centroAsociado.HasValue && centroAsociado > 0)
                {
                    query = query + "and cid=" + centroAsociado.Value;
                }
            }


            query += " order by cod_fil";

            FbCommand oCommand = new FbCommand(query, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            List<DAPARATOS> lAparato = new List<DAPARATOS>();
            try
            {
                while (oReader.Read())
                {
                    DAPARATOS oAparato = new DAPARATOS();
                    oAparato.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oAparato.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oAparato.COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL");
                    oAparato.DES_FIL = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    oAparato.IDENTIFICADOR = DataBase.GetStringFromReader(oReader, "IDENTIFICADOR");
                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.TOT_FIL = DataBase.GetIntFromReader(oReader, "TOT_FIL");
                    oAparato.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    oAparato.XEROX = DataBase.GetShortFromReader(oReader, "XEROX");
                    if (oAparato.OID == -1)
                    {
                        oAparato.COD_FIL = " ";
                    }

                    lAparato.Add(oAparato);
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
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }

            }




            return lAparato;
        }

        public static List<DAPARATOS> ListaPorCentro(int oidCentro)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            string query = "select * from daparatos where VERS=1 AND IOR_EMPRESA=4 ";

            if (oidCentro != -1)
            {
                query += "and CID=" + oidCentro + " or oid=-1";
            }

            query += " order by cod_fil";
            FbCommand oCommand = new FbCommand(query, oConexion);


            FbDataReader oReader = oCommand.ExecuteReader();

            List<DAPARATOS> lAparato = new List<DAPARATOS>();
            try
            {
                while (oReader.Read())
                {
                    DAPARATOS oAparato = new DAPARATOS();
                    oAparato.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oAparato.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL");
                    oAparato.DES_FIL = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    oAparato.IDENTIFICADOR = DataBase.GetStringFromReader(oReader, "IDENTIFICADOR");

                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.TOT_FIL = DataBase.GetIntFromReader(oReader, "TOT_FIL");
                    oAparato.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");


                    lAparato.Add(oAparato);
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
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }

                }

            }



            return lAparato;
        }

        public static List<DAPARATOS> ListaPorGrupoAparatos(int oidGrupo)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            string query = "select * from daparatos where IOR_EMPRESA=4 and vers=1 ";
            if (oidGrupo != -1)
            {
                query += "and owner=" + oidGrupo;
            }

            query += " order by cod_fil";
            FbCommand oCommand = new FbCommand(query, oConexion);

            FbDataReader oReader = oCommand.ExecuteReader();

            List<DAPARATOS> lAparato = new List<DAPARATOS>();
            try
            {
                while (oReader.Read())
                {
                    DAPARATOS oAparato = new DAPARATOS();
                    oAparato.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oAparato.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oAparato.COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL");
                    oAparato.DES_FIL = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    oAparato.IDENTIFICADOR = DataBase.GetStringFromReader(oReader, "IDENTIFICADOR");

                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.TOT_FIL = DataBase.GetIntFromReader(oReader, "TOT_FIL");
                    oAparato.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");

                    if (oAparato.CID != null){
                        int centroId = oAparato.CID.GetValueOrDefault();
                        oAparato.CENTRO = CentrosRepositorio.Obtener(centroId);
                    }
                    


                    lAparato.Add(oAparato);
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
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }

                }

            }


            return lAparato;
        }

        public static List<DAPARATOS> ListaV2()
        {           
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            string query = "select * from daparatos where VERS=1 AND IOR_EMPRESA=4 ";
            FbCommand oCommand = new FbCommand(query, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<DAPARATOS> lAparato = new List<DAPARATOS>();
            try
            {
                while (oReader.Read())
                {
                    DAPARATOS oAparato = new DAPARATOS();
                    oAparato.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oAparato.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oAparato.COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL");
                    oAparato.DES_FIL = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    oAparato.IDENTIFICADOR = DataBase.GetStringFromReader(oReader, "IDENTIFICADOR");

                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.TOT_FIL = DataBase.GetIntFromReader(oReader, "TOT_FIL");
                    oAparato.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");


                    lAparato.Add(oAparato);
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
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }

                }

            }



            return lAparato;
        }

        public static List<DAPARATOS> ListaPorGrupoAparatosV2(int oidGrupo)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            string query = "select * from daparatos where IOR_EMPRESA=4 ";
            if (oidGrupo != -1)
            {
                query += "and owner=" + oidGrupo;
            }

            query += " order by cod_fil";
            FbCommand oCommand = new FbCommand(query, oConexion);

            FbDataReader oReader = oCommand.ExecuteReader();

            List<DAPARATOS> lAparato = new List<DAPARATOS>();
            try
            {
                while (oReader.Read())
                {
                    DAPARATOS oAparato = new DAPARATOS();
                    oAparato.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oAparato.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oAparato.COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL");
                    oAparato.DES_FIL = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    oAparato.IDENTIFICADOR = DataBase.GetStringFromReader(oReader, "IDENTIFICADOR");

                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.TOT_FIL = DataBase.GetIntFromReader(oReader, "TOT_FIL");
                    oAparato.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");


                    lAparato.Add(oAparato);
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
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }

                }

            }


            return lAparato;
        }
    }
}
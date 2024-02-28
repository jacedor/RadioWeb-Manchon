using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos

{
    //ojo, los aparatos a nivel logico son tipos de exploraciones, viene de DSM
    public class AparatoRepositorio
    {
        public static string ObtenerTextoAparato(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string result = "";
            string query = "SELECT a.TEXTO FROM APARATOS a where a.oid=" + oid;
            FbCommand oCommand = new FbCommand(query, oConexion);
            try
            {
             
                FbDataReader oReader = oCommand.ExecuteReader();
              
                while (oReader.Read())
                {
                    result = DataBase.GetStringFromReader(oReader, "TEXTO");
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

            return result;


                
           
        }

        public static List<APARATOS> ObtenerParaInternet(string grupo ="",bool claustro=false)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string query = "SELECT a.OID, a.OWNER,g.COD_GRUP,a.FIL, a.TEXTO_INTERNET, g.DES_GRUP,a.ENVIARSMS, a.TEXTO " +
                " FROM APARATOS a left join GAPARATOS g on g.OID=a.OWNER where a.IOR_EMPRESA=4 and (a.TEXTO_INTERNET<>'') ";
            List<APARATOS> lDAparato = null;
            if (grupo.Length > 0) {
                query = query + " and g.COD_GRUP='" + grupo + "'";
            }
            if (claustro && grupo.Trim()=="RM")
            {
                query = query + " and ('RM1' IN (a.APARATO1,a.APARATO2,a.APARATO3,a.APARATO4,a.APARATO5,a.APARATO6))";
            }
            query += " order by a.TEXTO_INTERNET";
            FbCommand oCommand = new FbCommand(query, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            try
            {

                lDAparato = new List<APARATOS>();
                while (oReader.Read())
                {
                    APARATOS oAparato = new APARATOS();
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.DES_FIL = oReader["DES_GRUP"].ToString();
                    oAparato.FIL = oReader["FIL"].ToString();
                    oAparato.SMS = DataBase.GetStringFromReader(oReader, "ENVIARSMS");
                    oAparato.TEXTO_INTERNET = DataBase.GetStringFromReader(oReader, "TEXTO_INTERNET");
                    oAparato.TEXTO = DataBase.GetStringFromReader(oReader, "TEXTO");

                    lDAparato.Add(oAparato);
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
            
            return lDAparato;
        }

       

        public static APARATOS Obtener(string DescripcionAparato,string grupo)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from aparatos where OWNER=" + grupo + " and  FIL=" + DataBase.QuotedString(DescripcionAparato).Replace(" ",""), oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            APARATOS oAparato = new APARATOS();
            oAparato.OID = -1;
            try
            {
                while (oReader.Read())
                {
                    oAparato.APARATO1 = oReader["APARATO1"].ToString();
                    oAparato.APARATO2 = oReader["APARATO2"].ToString();
                    oAparato.APARATO3 = oReader["APARATO3"].ToString();
                    oAparato.BORRADO = oReader["BORRADO"].ToString();
                    oAparato.CANAL = oReader["CANAL"].ToString();
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oAparato.COD_FIL = oReader["COD_FIL"].ToString();
                    oAparato.C0MEN1 = oReader["C0MEN1"].ToString();
                    oAparato.COMEN2 = oReader["COMEN2"].ToString();
                    oAparato.COMEN3 = oReader["COMEN3"].ToString();
                    oAparato.DES_FIL = oReader["DES_FIL"].ToString();
                    oAparato.FIL = oReader["FIL"].ToString();
                    //La tabla Aparatos esta relacionada con GAPARATOS mediante el OWner
                    //oAparato.GRUPOAPARATOS = GAparatoRepositorio.Obtener((DBUtils.GetIntFromReader(oReader, "OWNER")));
                    oAparato.TEXTO = DataBase.GetStringFromReader(oReader, "TEXTO");
                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.USERNAME = oReader["USERNAME"].ToString();
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    EmpresaRepositorio oEmpresaRepo = new EmpresaRepositorio();
                    oAparato.EMPRESA = oEmpresaRepo.Obtener((int)oReader["IOR_EMPRESA"]);
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

        public static APARATOS Obtener(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from aparatos where oid=" + oid, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            APARATOS oAparato = new APARATOS();
            try
            {
                while (oReader.Read())
                {
                    oAparato.SMS = DataBase.GetStringFromReader(oReader, "ENVIARSMS");
                    oAparato.APARATO1 = oReader["APARATO1"].ToString();
                    oAparato.APARATO2 = oReader["APARATO2"].ToString();
                    oAparato.APARATO3 = oReader["APARATO3"].ToString();
                    oAparato.BORRADO = oReader["BORRADO"].ToString();
                    oAparato.CANAL = oReader["CANAL"].ToString();
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oAparato.COD_FIL = oReader["COD_FIL"].ToString();
                    oAparato.C0MEN1 = oReader["C0MEN1"].ToString();
                    oAparato.COMEN2 = oReader["COMEN2"].ToString();
                    oAparato.COMEN3 = oReader["COMEN3"].ToString();
                    oAparato.DES_FIL = oReader["DES_FIL"].ToString();
                    oAparato.FIL = oReader["FIL"].ToString();
                    //La tabla Aparatos esta relacionada con GAPARATOS mediante el OWner

                    //oAparato.GRUPOAPARATOS = GAparatoRepositorio.Obtener((DBUtils.GetIntFromReader(oReader, "OWNER")));

                    oAparato.TEXTO = DataBase.GetStringFromReader(oReader, "TEXTO");
                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.USERNAME = oReader["USERNAME"].ToString();
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    EmpresaRepositorio oEmpresaRepo = new EmpresaRepositorio();
                    oAparato.EMPRESA = oEmpresaRepo.Obtener((int)oReader["IOR_EMPRESA"]);
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
                    };
                }
            }
            oConexion.Close();
            return oAparato;
        }

        public static List<APARATOS> List()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from aparatos", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();


            List<APARATOS> lDAparato = new List<APARATOS>();
            try
            {
                while (oReader.Read())
                {
                    APARATOS oAparato = new APARATOS();
                    oAparato.APARATO1 = oReader["APARATO1"].ToString();
                    oAparato.APARATO2 = oReader["APARATO2"].ToString();
                    oAparato.APARATO3 = oReader["APARATO3"].ToString();
                    oAparato.BORRADO = oReader["BORRADO"].ToString();
                    oAparato.CANAL = oReader["CANAL"].ToString();
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oAparato.COD_FIL = oReader["COD_FIL"].ToString();
                    oAparato.C0MEN1 = oReader["C0MEN1"].ToString();
                    oAparato.COMEN2 = oReader["COMEN2"].ToString();
                    oAparato.COMEN3 = oReader["COMEN3"].ToString();
                    oAparato.DES_FIL = oReader["DES_FIL"].ToString();
                    oAparato.FIL = oReader["FIL"].ToString();
                    //La tabla Aparatos esta relacionada con GAPARATOS mediante el OWner

                    //oAparato.GRUPOAPARATOS = GAparatoRepositorio.Obtener((DBUtils.GetIntFromReader(oReader, "OWNER")));

                    //oAparato.TEXTO = DataBase.GetStringFromByte(oReader, "TEXTO");
                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.USERNAME = oReader["USERNAME"].ToString();
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    EmpresaRepositorio oEmpresaRepo = new EmpresaRepositorio();
                    oAparato.EMPRESA = oEmpresaRepo.Obtener((int)oReader["IOR_EMPRESA"]);

                    lDAparato.Add(oAparato);
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
            return lDAparato;
        }

        public static List<APARATOS> ListExploracionesCubiertasPorMutuaYGrupo(int grupoAparatos, int mutua)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("SELECT a.*  FROM PRECIOS P join APARATOS a on a.OID = p.IOR_TIPOEXPLORACION WHERE a.OWNER = " + grupoAparatos +" and P.IOR_ENTIDADPAGADORA = " + mutua, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();


            List<APARATOS> lDAparato = new List<APARATOS>();
            try
            {
                EmpresaRepositorio oEmpresaRepo = new EmpresaRepositorio();
                EMPRESAS oEmpresa = new EMPRESAS();
                while (oReader.Read())
                {
                    APARATOS oAparato = new APARATOS();
                    oAparato.APARATO1 = oReader["APARATO1"].ToString();
                    oAparato.APARATO2 = oReader["APARATO2"].ToString();
                    oAparato.APARATO3 = oReader["APARATO3"].ToString();
                    oAparato.BORRADO = oReader["BORRADO"].ToString();
                    oAparato.CANAL = oReader["CANAL"].ToString();
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oAparato.COD_FIL = oReader["COD_FIL"].ToString();
                    oAparato.C0MEN1 = oReader["C0MEN1"].ToString();
                    oAparato.COMEN2 = oReader["COMEN2"].ToString();
                    oAparato.COMEN3 = oReader["COMEN3"].ToString();
                    oAparato.DES_FIL = oReader["DES_FIL"].ToString();
                    oAparato.FIL = oReader["FIL"].ToString();
                    //La tabla Aparatos esta relacionada con GAPARATOS mediante el OWner

                    //oAparato.GRUPOAPARATOS = GAparatoRepositorio.Obtener((DBUtils.GetIntFromReader(oReader, "OWNER")));

                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.USERNAME = oReader["USERNAME"].ToString();
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    if (String.IsNullOrEmpty(oEmpresa.NOMBRE))
                    {
                        oEmpresa = oEmpresaRepo.Obtener((int)oReader["IOR_EMPRESA"]);
                        
                    }
                    
                        oAparato.EMPRESA = oEmpresa;
                    
                    try
                    {
                        oAparato.TEXTO =  DataBase.GetStringFromReader(oReader, "TEXTO");
                    }
                    catch (Exception)
                    {


                    }


                    lDAparato.Add(oAparato);
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
            return lDAparato;
        }

        public static List<APARATOS> List(int owner)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from aparatos where owner=" + owner, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
           

            List<APARATOS> lDAparato = new List<APARATOS>();
            try
            {
                while (oReader.Read())
                {
                    APARATOS oAparato = new APARATOS();
                    oAparato.APARATO1 = oReader["APARATO1"].ToString();
                    oAparato.APARATO2 = oReader["APARATO2"].ToString();
                    oAparato.APARATO3 = oReader["APARATO3"].ToString();
                    oAparato.BORRADO = oReader["BORRADO"].ToString();
                    oAparato.CANAL = oReader["CANAL"].ToString();
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oAparato.COD_FIL = oReader["COD_FIL"].ToString();
                    oAparato.C0MEN1 = oReader["C0MEN1"].ToString();
                    oAparato.COMEN2 = oReader["COMEN2"].ToString();
                    oAparato.COMEN3 = oReader["COMEN3"].ToString();
                    oAparato.DES_FIL = oReader["DES_FIL"].ToString();
                    oAparato.FIL = oReader["FIL"].ToString();
                    //La tabla Aparatos esta relacionada con GAPARATOS mediante el OWner

                    //oAparato.GRUPOAPARATOS = GAparatoRepositorio.Obtener((DBUtils.GetIntFromReader(oReader, "OWNER")));

                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.USERNAME = oReader["USERNAME"].ToString();
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    EmpresaRepositorio oEmpresaRepo = new EmpresaRepositorio();
                    oAparato.EMPRESA = oEmpresaRepo.Obtener((int)oReader["IOR_EMPRESA"]);
                    try
                    {
                        oAparato.TEXTO = DataBase.GetStringFromByte(oReader, "TEXTO");
                    }
                    catch (Exception)
                    {

                       
                    }
                   

                    lDAparato.Add(oAparato);
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
            return lDAparato;
        }

        public static List<APARATOS> ListaPorGrupoAparatos(int owner)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from aparatos where owner=" + owner + " and (borrado != 'T' OR borrado IS NULL) order by FIL", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();


            List<APARATOS> lDAparato = new List<APARATOS>();
            try
            {
                while (oReader.Read())
                {
                    APARATOS oAparato = new APARATOS();
                    oAparato.APARATO1 = oReader["APARATO1"].ToString();
                    oAparato.APARATO2 = oReader["APARATO2"].ToString();
                    oAparato.APARATO3 = oReader["APARATO3"].ToString();
                    oAparato.BORRADO = oReader["BORRADO"].ToString();
                    oAparato.CANAL = oReader["CANAL"].ToString();
                    oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oAparato.COD_FIL = oReader["COD_FIL"].ToString();
                    oAparato.C0MEN1 = oReader["C0MEN1"].ToString();
                    oAparato.COMEN2 = oReader["COMEN2"].ToString();
                    oAparato.COMEN3 = oReader["COMEN3"].ToString();
                    oAparato.DES_FIL = oReader["DES_FIL"].ToString();
                    oAparato.FIL = oReader["FIL"].ToString();
                    oAparato.TEXTO = oReader["TEXTO"].ToString();
                    oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oAparato.USERNAME = oReader["USERNAME"].ToString();
                    oAparato.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    EmpresaRepositorio oEmpresaRepo = new EmpresaRepositorio();
                    oAparato.EMPRESA = oEmpresaRepo.Obtener((int)oReader["IOR_EMPRESA"]);
                    oAparato.RECUENTO = DataBase.GetIntFromReader(oReader, "RECUENTO");
                    lDAparato.Add(oAparato);
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
            return lDAparato;
        }

        public static int checkPorMutua(int mutua, int tipoExploracion)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            int retorno = 0;

            FbCommand oCommand = new FbCommand("SELECT P.OID FROM PRECIOS P WHERE P.IOR_ENTIDADPAGADORA = " + mutua + " AND P.IOR_TIPOEXPLORACION =" + tipoExploracion, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            try
            {
                if (oReader.Read())
                {
                    retorno = 1;
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
            return retorno;
        }
    }
}

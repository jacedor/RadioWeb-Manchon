using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using ADPM.Common;

namespace RadioWeb.Models.Repos
{

    public class ListaDiaRepositorio
    {


        public static List<LISTADIA> ObtenerPorPaciente(int oidOwner)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            FbCommand oCommand = null;

            try
            {
                string query = "select d.*,e.nhcap FROM LISTADIA d";
                query += " left join exploracion e on d.OID =E.OID";                    
                 query +="  where d.IOR_PACIENTE=" + oidOwner + " order by FECHA DESC,HORA DESC ";
                oCommand = new FbCommand(query, oConexion);
                oConexion.Open();
                FbDataReader oReader = oCommand.ExecuteReader();
                List<LISTADIA> lListaDia = new List<LISTADIA>();
                while (oReader.Read())
                {
                    DateTime fechaExploracion;
                    LISTADIA oListaDia = new LISTADIA();
                    oListaDia.OID = DataBase.GetIntFromReader(oReader, "OID");
                    if (DateTime.TryParse(oReader["FECHA"].ToString(), out fechaExploracion))
                    {
                        oListaDia.FECHA = (DateTime)oReader["FECHA"];
                        oListaDia.DIA = ((DateTime)oReader["FECHA"]).ToLongDateString();

                    }
                    oListaDia.HORA = oReader["HORA"].ToString();
                    oListaDia.COLOR = ListaDiaAmbForatsRepositorio.GetColorByEstadoExploracion(oReader["ESTADO"].ToString());
                    oListaDia.COD_MED = DataBase.GetStringFromReader(oReader, "COD_MED");
                    oListaDia.HAYCOMEN = DataBase.GetBoolFromReader(oReader, "HAYCOMEN");
                    oListaDia.IOR_MASTER = DataBase.GetIntFromReader(oReader, "IOR_MASTER");
                    oListaDia.IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE");
                    oListaDia.PACIENTE = oReader["PACIENTE"].ToString();
                    oListaDia.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oListaDia.VIP = (oReader["VIP"].ToString() == "T" ? true : false);
                    ////oListaDia.INFORMADA = (oReader["INFORMADA"].ToString() == "T" ? true : false);
                    oListaDia.INFORMADA = DataBase.GetStringFromReader(oReader, "INFORMADA"); //(oReader["INFORMADA"].ToString() == "T" ? true : false);

                    oListaDia.MEDICO = oReader["MEDICO"].ToString();
                    oListaDia.TECNICO = oReader["TECNICO"].ToString();
                    oListaDia.COD_MED = oReader["COD_MED"].ToString();
                    oListaDia.NUMEROS = oReader["NUMEROS"].ToString();
                    oListaDia.COD_MUT = oReader["COD_MUT"].ToString();
                    oListaDia.COD_FIL = oReader["COD_FIL"].ToString();
                    oListaDia.ESTADO = oReader["ESTADO"].ToString();
                    oListaDia.FIL = oReader["FIL"].ToString();
                    oListaDia.EXPLO = oReader["EXPLO"].ToString();
                    oListaDia.NHCAP = oReader["NHCAP"].ToString();
                    oListaDia.HAYCONSUMIBLE = (oReader["HAYCONSUMIBLE"].ToString() == "T" ? true : false);
                    oListaDia.INTOCABLE = (oReader["INTOCABLE"].ToString() == "T" ? true : false);
                    float cantidad;
                    if (float.TryParse(oReader["CANTIDAD"].ToString(), out cantidad))
                    {
                        oListaDia.CANTIDAD = cantidad;

                    }
                    oListaDia.CENTRO = DataBase.GetIntFromReader(oReader, "CENTRO");

                    oListaDia.SIMBOLO = oReader["SIMBOLO"].ToString();
                    oListaDia.FACTURADA = (oReader["FACTURADA"].ToString() == "T" ? true : false);
                    oListaDia.APLAZADO = (oReader["APLAZADO"].ToString() == "T" ? true : false);
                    oListaDia.NOFACTURAB = (oReader["NOFACTURAB"].ToString() == "T" ? true : false);

                    oListaDia.TEXTO = DataBase.GetStringFromReader(oReader, "TEXTO");
                    oListaDia.MUTUA = DataBase.GetStringFromReader(oReader, "MUTUA");

                    if ((!string.IsNullOrEmpty(oListaDia.TEXTO)) && (oListaDia.TEXTO.Length > 15))
                    {
                        oListaDia.SUBTEXTO = oListaDia.TEXTO.Substring(0, 15) + "...";
                    }
                    else
                    {
                        oListaDia.SUBTEXTO = oListaDia.TEXTO;
                    }

                    lListaDia.Add(oListaDia);
                }
                //if (lListaDia.Count == 0)
                //    lListaDia.Add(new LISTADIA());

                return lListaDia;
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



        }

        public static List<LISTADIA> Lista(int ior_entidadpagadora,string estado,string username)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            FbCommand oCommand = null;

            try
            {
                string query = "SELECT r.oid, r.FECHA,r.HORA,p.PACIENTE,m.NOMBRE,r.fecha_iden,r.HORAMOD,r.USERNAME";
                query += " FROM exploracion r join paciente p on r.IOR_PACIENTE = p.OID join mutuas m on r.IOR_ENTIDADPAGADORA = m.OID";
                query +=" where r.idcitaonline is not null and r.ior_entidadpagadora = " + ior_entidadpagadora +  " and r.ESTADO = " + estado.QuotedString() ;
                oCommand = new FbCommand(query, oConexion);
                oConexion.Open();
                FbDataReader oReader = oCommand.ExecuteReader();
                List<LISTADIA> lListaDia = new List<LISTADIA>();
                while (oReader.Read())
                {
                    DateTime fechaExploracion;
                    LISTADIA oListaDia = new LISTADIA();
                    oListaDia.OID = DataBase.GetIntFromReader(oReader, "OID");
                    if (DateTime.TryParse(oReader["FECHA"].ToString(), out fechaExploracion))
                    {
                        oListaDia.FECHA = (DateTime)oReader["FECHA"];
                        oListaDia.DIA = ((DateTime)oReader["FECHA"]).ToLongDateString();
                    }
                    oListaDia.HORA = oReader["HORA"].ToString();                    
                    oListaDia.PACIENTE = oReader["PACIENTE"].ToString();   

                    oListaDia.TEXTO = "Derivada el " + DataBase.GetDateTimeFromReader(oReader,"FECHA_IDEN").Value.ToShortDateString() 
                        + " " + DataBase.GetStringFromReader(oReader,"HORA");
                    oListaDia.MUTUA = DataBase.GetStringFromReader(oReader, "NOMBRE");                   
                    lListaDia.Add(oListaDia);
                }
                
                return lListaDia;
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



        }

        public static List<LISTADIA> ListaPeticionesHL7()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            FbCommand oCommand = null;

            try
            {
                string query = "select d.*,e.nhcap,e.fecha_iden FROM LISTADIA d";
                query += " left join exploracion e on d.OID =E.OID";
                query += "  where d.ESTADO=0 and d.cid=99 and e.fecha >" +  DataBase.QuotedString(DateTime.Now.AddDays(-65).ToString("MM-dd-yyyy")) + " order by FECHA_IDEN DESC,HORA DESC ";
                oCommand = new FbCommand(query, oConexion);
                oConexion.Open();
                FbDataReader oReader = oCommand.ExecuteReader();
                List<LISTADIA> lListaDia = new List<LISTADIA>();
                while (oReader.Read())
                {
                    DateTime fechaExploracion;
                    LISTADIA oListaDia = new LISTADIA();
                    oListaDia.OID = DataBase.GetIntFromReader(oReader, "OID");
                    if (DateTime.TryParse(oReader["FECHA"].ToString(), out fechaExploracion))
                    {
                        oListaDia.FECHA = (DateTime)oReader["FECHA"];
                        oListaDia.DIA = ((DateTime)oReader["FECHA"]).ToLongDateString();
                        oListaDia.FECHA_IDEN = (DateTime)oReader["FECHA_IDEN"];


                    }
                    oListaDia.HORA = oReader["HORA"].ToString();
                    oListaDia.COLOR = ListaDiaAmbForatsRepositorio.GetColorByEstadoExploracion(oReader["ESTADO"].ToString());
                    oListaDia.COD_MED = DataBase.GetStringFromReader(oReader, "COD_MED");
                    oListaDia.HAYCOMEN = DataBase.GetBoolFromReader(oReader, "HAYCOMEN");
                    oListaDia.IOR_MASTER = DataBase.GetIntFromReader(oReader, "IOR_MASTER");
                    oListaDia.IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE");
                    oListaDia.PACIENTE = oReader["PACIENTE"].ToString();
                    oListaDia.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oListaDia.VIP = (oReader["VIP"].ToString() == "T" ? true : false);
                    oListaDia.INFORMADA = DataBase.GetStringFromReader(oReader, "INFORMADA"); //(oReader["INFORMADA"].ToString() == "T" ? true : false);

                    oListaDia.MEDICO = oReader["MEDICO"].ToString();
                    oListaDia.TECNICO = oReader["TECNICO"].ToString();
                    oListaDia.COD_MED = oReader["COD_MED"].ToString();
                    oListaDia.NUMEROS = oReader["NUMEROS"].ToString();
                    oListaDia.COD_MUT = oReader["COD_MUT"].ToString();
                    oListaDia.COD_FIL = oReader["COD_FIL"].ToString();
                    oListaDia.ESTADO = oReader["ESTADO"].ToString();
                    oListaDia.FIL = oReader["FIL"].ToString();
                    oListaDia.EXPLO = oReader["EXPLO"].ToString();
                    oListaDia.NHCAP = oReader["NHCAP"].ToString();
                    oListaDia.HAYCONSUMIBLE = (oReader["HAYCONSUMIBLE"].ToString() == "T" ? true : false);
                    oListaDia.INTOCABLE = (oReader["INTOCABLE"].ToString() == "T" ? true : false);
                    float cantidad;
                    if (float.TryParse(oReader["CANTIDAD"].ToString(), out cantidad))
                    {
                        oListaDia.CANTIDAD = cantidad;

                    }
                    oListaDia.CENTRO = DataBase.GetIntFromReader(oReader, "CENTRO");

                    oListaDia.SIMBOLO = oReader["SIMBOLO"].ToString();
                    oListaDia.FACTURADA = (oReader["FACTURADA"].ToString() == "T" ? true : false);
                    oListaDia.APLAZADO = (oReader["APLAZADO"].ToString() == "T" ? true : false);
                    oListaDia.NOFACTURAB = (oReader["NOFACTURAB"].ToString() == "T" ? true : false);

                    oListaDia.TEXTO = DataBase.GetStringFromReader(oReader, "TEXTO");
                    oListaDia.MUTUA = DataBase.GetStringFromReader(oReader, "MUTUA");

                    if ((!string.IsNullOrEmpty(oListaDia.TEXTO)) && (oListaDia.TEXTO.Length > 15))
                    {
                        oListaDia.SUBTEXTO = oListaDia.TEXTO.Substring(0, 15) + "...";
                    }
                    else
                    {
                        oListaDia.SUBTEXTO = oListaDia.TEXTO;
                    }

                    lListaDia.Add(oListaDia);
                }

                return lListaDia;
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



        }

  

        public static List<LISTADIA> ObtenerPorPacienteYFecha(int oidExploracion, int oidOwner, DateTime fecha)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
         

                oConexion.Open();

                 oCommand = new FbCommand("select * FROM LISTADIA d where d.IOR_PACIENTE=" +
                    oidOwner + " and d.fecha ='" + fecha.ToString("MM/dd/yyyy") + "' and  d.estado not in (1,4,5) and not d.oid =" + oidExploracion + " order by FECHA DESC,HORA DESC ", oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();

                List<LISTADIA> lListaDia = new List<LISTADIA>();
                while (oReader.Read())
                {

                    LISTADIA oListaDia = new LISTADIA();
                    oListaDia.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oListaDia.CENTRO = DataBase.GetIntFromReader(oReader, "CENTRO");
                    oListaDia.FECHA = fecha;
                    oListaDia.PACIENTE = DataBase.GetStringFromReader(oReader,"PACIENTE");                    
                    oListaDia.COLOR = ListaDiaAmbForatsRepositorio.GetColorByEstadoExploracion(oReader["ESTADO"].ToString());
                    oListaDia.ESTADO = oReader["ESTADO"].ToString();
                    oListaDia.EXPLO = oReader["EXPLO"].ToString();
                    oListaDia.HORA = oReader["HORA"].ToString();
                    oListaDia.COD_MUT = oReader["COD_MUT"].ToString();
                    oListaDia.COD_FIL = oReader["COD_FIL"].ToString();
                    oListaDia.EXPLO = oReader["EXPLO"].ToString();
                    float cantidad;
                    if (float.TryParse(oReader["CANTIDAD"].ToString(), out cantidad))
                    {
                        oListaDia.CANTIDAD = cantidad;

                    }

                    oListaDia.SIMBOLO = oReader["SIMBOLO"].ToString();
                    oListaDia.IOR_GPR = DataBase.GetIntFromReader( oReader,"IOR_GPR");
                    oListaDia.FACTURADA = (oReader["FACTURADA"].ToString() == "T" ? true : false);
                    oListaDia.APLAZADO = (oReader["APLAZADO"].ToString() == "T" ? true : false);
                    oListaDia.NOFACTURAB = (oReader["NOFACTURAB"].ToString() == "T" ? true : false);
                    oListaDia.PAGADO = (oReader["PAGADO"].ToString() == "T" ? true : false);
             oListaDia.GRUPOAPA = DataBase.GetIntFromReader(oReader, "GRUPOAPA");
                    oListaDia.FIL = oReader["FIL"].ToString();
                    oListaDia.MUTUA = DataBase.GetStringFromReader(oReader, "MUTUA");
                    lListaDia.Add(oListaDia);
                }               

                return lListaDia;
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



        }

        public static LISTADIA Obtener(int oidExploracion)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                oCommand = new FbCommand("select * FROM LISTADIA d where d.oid=" + oidExploracion,oConexion);
                FbDataReader oReader = oCommand.ExecuteReader() ;
                LISTADIA oListaDia = new LISTADIA();
                while (oReader.Read())
                {
                    DateTime fechaExploracion;

                    oListaDia.OID = DataBase.GetIntFromReader(oReader, "OID");
                    if (DateTime.TryParse(oReader["FECHA"].ToString(), out fechaExploracion))
                    {
                        oListaDia.FECHA = (DateTime)oReader["FECHA"];
                        oListaDia.DIA = ((DateTime)oReader["FECHA"]).ToLongDateString();

                    }
                    oListaDia.HORA = oReader["HORA"].ToString();
                    oListaDia.HORA_LL = oReader["HORA_LL"].ToString();

                    oListaDia.COD_MED = DataBase.GetStringFromReader(oReader, "COD_MED");
                    oListaDia.CENTRO = DataBase.GetIntFromReader(oReader, "CENTRO");

                    oListaDia.HAYCOMEN = (oReader["HAYCOMEN"].ToString() == "T" ? true : false);
                    oListaDia.PACIENTE = oReader["PACIENTE"].ToString();
                    oListaDia.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oListaDia.VIP = (oReader["VIP"].ToString() == "T" ? true : false);
                  
                    oListaDia.INFORMADA = DataBase.GetStringFromReader(oReader, "INFORMADA"); //(oReader["INFORMADA"].ToString() == "T" ? true : false);

                    oListaDia.MEDICO = oReader["MEDICO"].ToString();
                    oListaDia.TECNICO = oReader["TECNICO"].ToString();
                    oListaDia.COD_MED = oReader["COD_MED"].ToString();
                    oListaDia.NUMEROS = oReader["NUMEROS"].ToString();

                    oListaDia.COD_MUT = oReader["COD_MUT"].ToString();
                    oListaDia.COD_FIL = oReader["COD_FIL"].ToString();
                    oListaDia.COLOR = ListaDiaAmbForatsRepositorio.GetColorByEstadoExploracion(oReader["ESTADO"].ToString());
                    oListaDia.ESTADO = oReader["ESTADO"].ToString();
                    oListaDia.FIL = oReader["FIL"].ToString();
                    oListaDia.EXPLO = oReader["EXPLO"].ToString();
                    oListaDia.GRUPOAPA = DataBase.GetIntFromReader(oReader, "GRUPOAPA");

                    oListaDia.IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE");
                    oListaDia.IOR_GPR = DataBase.GetIntFromReader(oReader, "IOR_GPR");
                    oListaDia.IOR_MEDICO = DataBase.GetIntFromReader(oReader, "IOR_MEDICO");
                    oListaDia.IOR_MASTER = DataBase.GetIntFromReader(oReader, "IOR_MASTER");
                    oListaDia.FIRMA_CONSEN = DataBase.GetBoolFromReader(oReader, "FIRMA_CONSEN");
                    oListaDia.LOPD = DataBase.GetBoolFromReader(oReader, "LOPD");
                    oListaDia.FECHAMAXENTREGA= DataBase.GetDateTimeFromReader(oReader, "FECHAMAXENTREGA");
                    oListaDia.QRCOMPARTIRCASO = DataBase.GetBoolFromReader(oReader, "QRCOMPARTIRCASO");                    
                    oListaDia.HAYCONSUMIBLE = (oReader["HAYCONSUMIBLE"].ToString() == "T" ? true : false);
                    oListaDia.INTOCABLE = (oReader["INTOCABLE"].ToString() == "T" ? true : false);
                    float cantidad;
                    if (float.TryParse(oReader["CANTIDAD"].ToString(), out cantidad))
                    {
                        oListaDia.CANTIDAD = cantidad;

                    }

                    oListaDia.SIMBOLO = oReader["SIMBOLO"].ToString();
                    oListaDia.FACTURADA = (oReader["FACTURADA"].ToString() == "T" ? true : false);
                    oListaDia.APLAZADO = (oReader["APLAZADO"].ToString() == "T" ? true : false);
                    oListaDia.NOFACTURAB = (oReader["NOFACTURAB"].ToString() == "T" ? true : false);
                    oListaDia.MUTUA = DataBase.GetStringFromReader(oReader, "MUTUA");
                    oListaDia.PAGADO = (oReader["PAGADO"].ToString() == "T" ? true : false);

                    oListaDia.TEXTO = ((TEXTOS)TextosRepositorio.Obtener((int)oReader["OID"])).TEXTO;
                    if ((!string.IsNullOrEmpty(oListaDia.TEXTO)) && (oListaDia.TEXTO.Length > 15))
                    {
                        oListaDia.SUBTEXTO = oListaDia.TEXTO.Substring(0, 15) + "...";
                    }
                    else
                    {
                        oListaDia.SUBTEXTO = oListaDia.TEXTO;
                    }

                }

                return oListaDia;
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


        }

        public static List<string> EsAparatoComplejo(string Aparato) {

            List<string> oresult = new List<string>();

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = new FbCommand("select * FROM APARATOS_COMPLEJOS d where d.APARATO1=" + DataBase.QuotedString(Aparato), oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            try
            {
                while (oReader.Read())
                {

                    for (int i = 2; i < 10; i++)
                    {
                        if (DataBase.GetStringFromReader(oReader, "APARATO" + i.ToString()) != "")
                        {
                            oresult.Add(DataBase.GetStringFromReader(oReader, "APARATO" + i.ToString()));
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


  
    }
}
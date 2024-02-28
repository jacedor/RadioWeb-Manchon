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
    public class ColegiadoRepositorio
    {

        private static string GetFriendlyTratamiento(short id)
        {


            string Tratamiento;
            switch (id)
            {
                case 1:
                    Tratamiento = "Sr";
                    break;
                case 2:
                    Tratamiento = "Sr.D";
                    break;
                case 3:
                    Tratamiento = "Dr.";
                    break;
                case 4:
                    Tratamiento = "Srta.";
                    break;
                case 5:
                    Tratamiento = "Sra.";
                    break;
                case 6:
                    Tratamiento = "Sra. Dña.";
                    break;
                case 7:
                    Tratamiento = "Dra.";
                    break;
                case 8:
                    Tratamiento = "Niño";
                    break;
                case 9:
                    Tratamiento = "Niña";
                    break;
                default:
                    Tratamiento = "";
                    break;
            }
            return Tratamiento;
        }

        public static COLEGIADOS Obtener(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from colegiados where oid=" + oid, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            COLEGIADOS oColegiado = new COLEGIADOS();
            try
            {
                while (oReader.Read())
                {
                    oColegiado.BORRADO = oReader["BORRADO"].ToString();
                    oColegiado.CANAL = oReader["CANAL"].ToString();
                    oColegiado.CID = DataBase.GetIntFromReader(oReader, "CID");

                    oColegiado.COD_MED = oReader["COD_MED"].ToString();
                    oColegiado.ESPEC = oReader["ESPEC"].ToString();
                    oColegiado.IOR_ESPECIALIDAD = DataBase.GetIntFromReader(oReader, "IOR_ESPECIALIDAD");
                    oColegiado.ESPECIALIDAD = EspecialidadRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "IOR_ESPECIALIDAD"));
                    oColegiado.HORAMOD = oReader["HORAMOD"].ToString();
                    oColegiado.ICS = DataBase.GetDoubleFromReader(oReader, "ICS");
                    oColegiado.MAILING = oReader["MAILING"].ToString();
                    oColegiado.MAILING1 = oReader["MAILING1"].ToString();
                    oColegiado.MAILING2 = oReader["MAILING2"].ToString();
                    oColegiado.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oColegiado.MUTUAS = DataBase.GetDoubleFromReader(oReader, "MUTUAS");
                    oColegiado.NIF = oReader["NIF"].ToString();
                    oColegiado.NOMBRE = oReader["NOMBRE"].ToString();
                    oColegiado.PRIVADOS = DataBase.GetDoubleFromReader(oReader, "PRIVADOS");
                    oColegiado.TRATA = oReader["TRATA"].ToString();

                    oColegiado.USERNAME = oReader["USERNAME"].ToString();
                    oColegiado.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    oColegiado.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oColegiado.IOR_ESPECIALIDAD = DataBase.GetIntFromReader(oReader, "IOR_ESPECIALIDAD");
                    oColegiado.IOR_CENTRO = DataBase.GetIntFromReader(oReader, "IOR_CENTRO");

                    oColegiado.DIRECCIONES = DireccionRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID"));
                    oColegiado.TELEFONOS = TelefonoRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID"));
                    oColegiado.TEXTO = DataBase.convertRtfToHtml(TextosRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID")).TEXTO);
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


            return oColegiado;
        }
        public static int Update(COLEGIADOS oColegiado)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand myCommand = null;
            try
            {
                oConexion.Open();
                if (oColegiado.COD_MED != null && oColegiado.COD_MED.Length > 0)
                {
                    oColegiado.COD_MED = oColegiado.COD_MED.ToUpper();
                }

                string updateStament = "update COLEGIADOS set Username=" + HttpContext.Current.User.Identity.Name.ToString().QuotedString() +
                    ",MODIF=" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss").QuotedString() + ",NOMBRE=" + oColegiado.NOMBRE.QuotedString() +
                    ",TRATA=" + oColegiado.TRATA.QuotedString() + ",COD_MED=" + oColegiado.COD_MED.QuotedString() + ", NIF=" + oColegiado.NIF.QuotedString() +
                    ",IOR_ESPECIALIDAD=" + oColegiado.IOR_ESPECIALIDAD + ",IOR_CENTRO=" + oColegiado.IOR_CENTRO +
                    ",EMAIL=" + oColegiado.EMAIL.QuotedString() + "" +
                     " WHERE OID = " + oColegiado.OID;

                myCommand = new FbCommand();

                myCommand.CommandText = updateStament;
                myCommand.Connection = oConexion;

                // Execute uPDATE
                myCommand.ExecuteNonQuery();
                int oidColegiado = oColegiado.OID;

                myCommand.Dispose();

                //este indizador es para cuando agreguemos multiples direcciones
                for (int i = 0; i < oColegiado.DIRECCIONES.Count; i++)
                {

                    //SI LA DIRECCIÓN ES UNA NUEVA INSERCION.
                    if (oColegiado.DIRECCIONES.ElementAt(i).OID == -1)
                    {
                        oColegiado.DIRECCIONES.ElementAt(i).OWNER = oidColegiado;
                        int oidDireccion = DireccionRepositorio.Insertar(oColegiado.DIRECCIONES.ElementAt(i));
                    }
                    else
                    {
                        DireccionRepositorio.Editar(oColegiado.DIRECCIONES.ElementAt(i));
                    }

                }

                List<TELEFONO> oListTelColBD = TelefonoRepositorio.Obtener(oidColegiado);


                for (int j = 0; j < oListTelColBD.Count; j++)
                {

                    try
                    {
                        //Buscamos en el objeto enviado desde la Página si coincide el oid, sino lo eliminamos
                        if (oListTelColBD.Where(t => t.OID == oColegiado.TELEFONOS.ElementAt(j).OID).Count() == 0)
                        {
                            TelefonoRepositorio.Delete(oListTelColBD[j].OID.ToString());
                        }

                    }
                    catch (Exception)
                    {


                    }

                }

                //este indizador es para cuando agreguemos multiples telefonos
                for (int i = 0; i < oColegiado.TELEFONOS.Count; i++)
                {

                    //SI EL TELEFONO ES UNA NUEVA INSERCION.
                    if (oColegiado.TELEFONOS.ElementAt(i).OID <= 0)
                    {
                        oColegiado.TELEFONOS.ElementAt(i).OWNER = oidColegiado;
                        int oidTelefono = TelefonoRepositorio.Insertar(oColegiado.TELEFONOS.ElementAt(i));
                    }
                    else
                    {
                        TelefonoRepositorio.Editar(oColegiado.TELEFONOS.ElementAt(i));
                    }

                }


                return oidColegiado;
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
                    if (myCommand != null)
                    {
                        myCommand.Dispose();
                    }
                }
            }
        }

        public static int Insertar(COLEGIADOS oColegiado)
        {
            int oidColeagiado;
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand myCommand = null;
            string query = "Insert into colegiados (OID, NOMBRE,TRATA, COD_MED, ESPEC,NIF,IOR_ESPECIALIDAD,IOR_CENTRO,EMAIL) VALUES (gen_id(GENUID,1)," +
                DataBase.QuotedString(oColegiado.NOMBRE) + "," + DataBase.QuotedString(oColegiado.TRATA) + "," + DataBase.QuotedString(oColegiado.COD_MED) + "," +
                DataBase.QuotedString(oColegiado.ESPEC) + "," + DataBase.QuotedString(oColegiado.NIF) + "," + oColegiado.IOR_ESPECIALIDAD + "," + oColegiado.IOR_CENTRO + "," +
                DataBase.QuotedString(oColegiado.EMAIL) + ") returning oid";

            try
            {
                oConexion.Open();
                myCommand = new FbCommand();
                myCommand.Connection = oConexion;
                myCommand.CommandText = query;
                oidColeagiado = (int)myCommand.ExecuteScalar();

                //este indizador es para cuando agreguemos multiples direcciones
                for (int i = 0; i < oColegiado.DIRECCIONES.Count; i++)
                {
                    if (oColegiado.DIRECCIONES.ElementAt(i).DIRECCION1 != null && oColegiado.DIRECCIONES.ElementAt(i).DIRECCION1.Length > 0)
                    {
                        oColegiado.DIRECCIONES.ElementAt(i).OWNER = oidColeagiado;
                        int oidDireccion = DireccionRepositorio.Insertar(oColegiado.DIRECCIONES.ElementAt(i));
                    }
                }

                //este indizador es para cuando agreguemos multiples telefonos
                for (int i = 0; i < oColegiado.TELEFONOS.Count; i++)
                {
                    if (oColegiado.TELEFONOS.ElementAt(i).NUMERO != null && oColegiado.TELEFONOS.ElementAt(i).NUMERO.Length > 0)
                    {
                        oColegiado.TELEFONOS.ElementAt(i).OWNER = oidColeagiado;
                        int oidTelefono = TelefonoRepositorio.Insertar(oColegiado.TELEFONOS.ElementAt(i));
                    }
                }

                if (oColegiado.TEXTO != null && oColegiado.TEXTO.Length > 0)
                {
                    TEXTOS oTextoExplo = new TEXTOS();
                    oTextoExplo.TEXTO = oColegiado.TEXTO;
                    oTextoExplo.OWNER = oidColeagiado;
                    TextosRepositorio.InsertarOrUpdate(oTextoExplo);
                }


                return oidColeagiado;
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
                    if (myCommand != null)
                    {
                        myCommand.Dispose();
                    }

                }

            }


        }

        public static List<COLEGIADOS> List()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from colegiados ", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<COLEGIADOS> lColegiado = new List<COLEGIADOS>();

            try
            {
                while (oReader.Read())
                {
                    COLEGIADOS oColegiado = new COLEGIADOS();
                    oColegiado.BORRADO = oReader["BORRADO"].ToString();
                    oColegiado.CANAL = oReader["CANAL"].ToString();
                    oColegiado.CID = DataBase.GetIntFromReader(oReader, "CID");
                    // CentroExternoRepositorio oCentroRepo = new CentroExternoRepositorio();
                    // oColegiado.CENTRO = oCentroRepo.Obtener(DBUtils.GetIntFromReader(oReader, "IOR_CENTRO"));
                    oColegiado.COD_MED = oReader["COD_MED"].ToString();
                    oColegiado.ESPEC = oReader["ESPEC"].ToString();

                    if (DataBase.GetIntFromReader(oReader, "IOR_ESPECIALIDAD") != -1)
                    {
                        oColegiado.ESPECIALIDAD = EspecialidadRepositorio.Obtener((int)oReader["IOR_ESPECIALIDAD"]);
                    }

                    oColegiado.HORAMOD = oReader["HORAMOD"].ToString();
                    oColegiado.ICS = DataBase.GetDoubleFromReader(oReader, "ICS");
                    oColegiado.MAILING = oReader["MAILING"].ToString();
                    oColegiado.MAILING1 = oReader["MAILING1"].ToString();
                    oColegiado.MAILING2 = oReader["MAILING2"].ToString();
                    oColegiado.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oColegiado.MUTUAS = DataBase.GetDoubleFromReader(oReader, "MUTUAS");
                    oColegiado.NIF = oReader["NIF"].ToString();
                    oColegiado.NOMBRE = oReader["NOMBRE"].ToString();
                    oColegiado.PRIVADOS = DataBase.GetDoubleFromReader(oReader, "PRIVADOS");
                    oColegiado.TRATA = oReader["TRATA"].ToString();
                    oColegiado.USERNAME = oReader["USERNAME"].ToString();
                    oColegiado.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    oColegiado.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oColegiado.DIRECCIONES = DireccionRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID"));
                    oColegiado.TELEFONOS = TelefonoRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID"));
                    oColegiado.TEXTO = TextosRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID")).TEXTO;
                    lColegiado.Add(oColegiado);
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


            return lColegiado;
        }

        public static List<COLEGIADOS> Buscar(int numcolegiado)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from colegiados where cod_med like '%" + numcolegiado.ToString().ToUpper() + "%'", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<COLEGIADOS> lColegiado = new List<COLEGIADOS>();
            try
            {
                while (oReader.Read())
                {
                    COLEGIADOS oColegiado = new COLEGIADOS();
                    oColegiado.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oColegiado.COD_MED = oReader["COD_MED"].ToString();
                    oColegiado.ESPEC = oReader["ESPEC"].ToString();
                    if (DataBase.GetIntFromReader(oReader, "IOR_ESPECIALIDAD") != -1)
                    {
                        oColegiado.ESPECIALIDAD = EspecialidadRepositorio.Obtener((int)oReader["IOR_ESPECIALIDAD"]);
                    }

                    oColegiado.MUTUAS = DataBase.GetDoubleFromReader(oReader, "MUTUAS");
                    oColegiado.NIF = oReader["NIF"].ToString();
                    oColegiado.NOMBRE = oReader["NOMBRE"].ToString();
                    oColegiado.TRATA = oReader["TRATA"].ToString();
                    oColegiado.OID = DataBase.GetIntFromReader(oReader, "OID");
                   // oColegiado.DIRECCIONES = DireccionRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID"));
                   // oColegiado.TELEFONOS = TelefonoRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID"));
                    lColegiado.Add(oColegiado);
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



            return lColegiado;
        }

        public static List<COLEGIADOS> BuscarSinEspec(string searchCriteria)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select c.*" +
            " from colegiados c " +
            "where c.NOMBRE like '" + searchCriteria.ToUpper().Replace(' ', '%') +
            "%' order by nombre", oConexion);

            FbDataReader oReader = oCommand.ExecuteReader();
            List<COLEGIADOS> lColegiado = new List<COLEGIADOS>();
            try
            {
                while (oReader.Read())
                {
                    COLEGIADOS oColegiado = new COLEGIADOS();
                    oColegiado.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oColegiado.COD_MED = oReader["COD_MED"].ToString();
                    oColegiado.ESPEC = oReader["ESPEC"].ToString();
                    if (DataBase.GetIntFromReader(oReader, "IOR_ESPECIALIDAD") != -1)
                    {
                        oColegiado.ESPECIALIDAD = new ESPECIALIDADES
                        {
                            DESCRIPCION = DataBase.GetStringFromReader(oReader, "DESCRIPCION")
                        };
                        //EspecialidadRepositorio.Obtener((int)oReader["IOR_ESPECIALIDAD"]);
                    }

                    oColegiado.MUTUAS = DataBase.GetDoubleFromReader(oReader, "MUTUAS");
                    oColegiado.NIF = oReader["NIF"].ToString();
                    oColegiado.NOMBRE = oReader["NOMBRE"].ToString();
                    oColegiado.TRATA = oReader["TRATA"].ToString();
                    oColegiado.OID = DataBase.GetIntFromReader(oReader, "OID");

                    lColegiado.Add(oColegiado);
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
            return lColegiado;
        }

        public static List<COLEGIADOS> Buscar(string searchCriteria)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select (c.NOMBRE || ' ' || e.DESCRIPCION),e.DESCRIPCION ,c.*" +
            " from colegiados c left join ESPECIALIDADES e on c.IOR_ESPECIALIDAD = e.OID " +
            "where (c.NOMBRE || ' ' || e.DESCRIPCION) like '" + searchCriteria.ToUpper().Replace(' ', '%') + 
            "%' order by nombre", oConexion);

            FbDataReader oReader = oCommand.ExecuteReader();
            List<COLEGIADOS> lColegiado = new List<COLEGIADOS>();
            try
            {
                while (oReader.Read())
                {
                    COLEGIADOS oColegiado = new COLEGIADOS();
                    oColegiado.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oColegiado.COD_MED = oReader["COD_MED"].ToString();
                    oColegiado.ESPEC = oReader["ESPEC"].ToString();
                    if (DataBase.GetIntFromReader(oReader, "IOR_ESPECIALIDAD") != -1)
                    {
                        oColegiado.ESPECIALIDAD = new ESPECIALIDADES
                        {
                            DESCRIPCION = DataBase.GetStringFromReader(oReader, "DESCRIPCION")
                        };                        
                            //EspecialidadRepositorio.Obtener((int)oReader["IOR_ESPECIALIDAD"]);
                    }

                    oColegiado.MUTUAS = DataBase.GetDoubleFromReader(oReader, "MUTUAS");
                    oColegiado.NIF = oReader["NIF"].ToString();
                    oColegiado.NOMBRE = oReader["NOMBRE"].ToString();
                    oColegiado.TRATA = oReader["TRATA"].ToString();
                    oColegiado.OID = DataBase.GetIntFromReader(oReader, "OID");

                    lColegiado.Add(oColegiado);
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
            return lColegiado;
        }
    }
}
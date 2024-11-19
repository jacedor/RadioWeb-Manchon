using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using RadioWeb.Models.Logica;
using System.Text;
using Newtonsoft.Json;
using System.Collections;
using RadioWeb.ViewModels.Paciente;

namespace RadioWeb.Models.Repos
{

    public class PacienteRepositorio
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

        public static PACIENTE ObtenerPorHC(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from PACIENTE where cod_pac=" + oid, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            PACIENTE oPaciente = new PACIENTE();
            try
            {
                while (oReader.Read())
                {

                    string dateBirth;

                    if (oReader["FECHAN"] == DBNull.Value)
                    {
                        dateBirth = "";
                    }
                    else
                    {
                        dateBirth = DataBase.CalcularEdad((DateTime)oReader["FECHAN"]);
                    }
                    string fechaNacimiento = DataBase.GetDateTimeFromReader(oReader, "FECHAN").ToString();
                    DateTime dateValue;

                    if (DateTime.TryParse(fechaNacimiento, out dateValue))
                    {
                        fechaNacimiento = dateValue.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        fechaNacimiento = "";
                    }


                    MUTUAS oMutuaTemporal = MutuasRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "CID"));


                    oPaciente = new PACIENTE
                    {
                        AVISO = DataBase.GetStringFromReader(oReader, "AVISO"),
                        OID = DataBase.GetIntFromReader(oReader, "OID"),
                        BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO"),
                        CID = DataBase.GetIntFromReader(oReader, "CID"),
                        CIP = DataBase.GetStringFromReader(oReader, "CIP"),
                        COD_PAC = DataBase.GetStringFromReader(oReader, "COD_PAC"),
                        COMENTARIO = DataBase.GetStringFromByte(oReader, "COMENTARIO"),
                        CODMUTUA = oMutuaTemporal.CODMUT,
                        DESCMUTUA = oMutuaTemporal.NOMBRE,
                        DIRECCIONES = DireccionRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID")),
                        DNI = DataBase.GetStringFromReader(oReader, "DNI"),
                        EMAIL = DataBase.GetStringFromReader(oReader, "EMAIL"),
                        EDAD = dateBirth,
                        FECHAN = dateValue,
                        HORAMOD = DataBase.GetStringFromReader(oReader, "HORAMOD"),
                        IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA"),
                        MAILING = DataBase.GetStringFromReader(oReader, "MAILING"),
                        MAILING1 = DataBase.GetStringFromReader(oReader, "MAILING1"),
                        MAILING2 = DataBase.GetStringFromReader(oReader, "MAILING2"),
                        MAILING3 = DataBase.GetStringFromReader(oReader, "MAILING3"),
                        MAILING4 = DataBase.GetStringFromReader(oReader, "MAILING4"),
                        MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF"),
                        OTROS1 = DataBase.GetStringFromReader(oReader, "OTROS1"),
                        NUEVA_LOPD = DataBase.GetStringFromReader(oReader, "NUEVA_LOPD"),
                        OTROS3 = DataBase.GetStringFromReader(oReader, "OTROS3"),
                        OTROS4 = DataBase.GetStringFromReader(oReader, "OTROS4"),
                        OTROS5 = DataBase.GetStringFromReader(oReader, "OTROS5"),
                        OWNER = DataBase.GetIntFromReader(oReader, "OWNER"),
                        PACIENTE1 = DataBase.GetStringFromReader(oReader, "PACIENTE"),
                        POLIZA = DataBase.GetStringFromReader(oReader, "POLIZA"),
                        PROFESION = DataBase.GetStringFromReader(oReader, "PROFESION"),
                        RIP = DataBase.GetStringFromReader(oReader, "RIP"),
                        SEXO = (DataBase.GetStringFromReader(oReader, "SEXO") == "H" ? "H" : "M"),
                        TARJETA = DataBase.GetStringFromReader(oReader, "TARJETA"),
                        TELEFONOS = TelefonoRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID")),
                        TRAC = DataBase.GetShortFromReader(oReader, "TRAC"),
                        TRATAMIENTODESC = GetFriendlyTratamiento(DataBase.GetShortFromReader(oReader, "TRAC")),
                        USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME"),
                        VERS = DataBase.GetIntFromReader(oReader, "VERS"),
                        VIP = DataBase.GetStringFromReader(oReader, "VIP"),
                        RESPONSABLE = DataBase.GetStringFromReader(oReader, "RNOMBRE"),
                        DNIRESPONSABLE = DataBase.GetStringFromReader(oReader, "RDNI"),
                        COMPARTIR = DataBase.GetStringFromReader(oReader, "QRCOMPARTIRCASO"),
                        TEXTO = TextosRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID")).TEXTO,
                        HAYCAMBIOSPACIENTE = "F",
                        //Añadimos las configuraciones de LOPD.
                        ENVIO_MEDICO = DataBase.GetStringFromReader(oReader, "ENVIO_MEDICO"),
                        ENVIO_RESULTADOS = DataBase.GetStringFromReader(oReader, "ENVIO_RESULTADOS"),
                        ENVIO_MAIL = DataBase.GetStringFromReader(oReader, "ENVIO_MAIL"),
                        ENVIO_SMS = DataBase.GetStringFromReader(oReader, "ENVIO_SMS"),
                        ENVIO_PROPAGANDA = DataBase.GetStringFromReader(oReader, "ENVIO_PROPAGANDA")
                    };



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



            return oPaciente;
        }



        public static PACIENTE Obtener(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from PACIENTE where oid=" + oid, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            PACIENTE oPaciente = new PACIENTE();
            try
            {
                while (oReader.Read())
                {

                    string dateBirth;

                    if (oReader["FECHAN"] == DBNull.Value)
                    {
                        dateBirth = "";
                    }
                    else
                    {
                        dateBirth = DataBase.CalcularEdad((DateTime)oReader["FECHAN"]);
                    }
                    string fechaNacimiento = DataBase.GetDateTimeFromReader(oReader, "FECHAN").ToString();
                    DateTime dateValue;

                    if (DateTime.TryParse(fechaNacimiento, out dateValue))
                    {
                        fechaNacimiento = dateValue.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        fechaNacimiento = "";
                    }


                    MUTUAS oMutuaTemporal = MutuasRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "CID"));


                    oPaciente = new PACIENTE
                    {
                        AVISO = DataBase.GetStringFromReader(oReader, "AVISO"),
                        OID = DataBase.GetIntFromReader(oReader, "OID"),
                        BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO"),
                        CID = DataBase.GetIntFromReader(oReader, "CID"),
                        CIP = DataBase.GetStringFromReader(oReader, "CIP"),
                        COD_PAC = DataBase.GetStringFromReader(oReader, "COD_PAC"),
                        COMENTARIO = DataBase.GetStringFromByte(oReader, "COMENTARIO"),
                        CODMUTUA = oMutuaTemporal.CODMUT,
                        DESCMUTUA = oMutuaTemporal.NOMBRE,
                        DIRECCIONES = DireccionRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID")),
                        DNI = DataBase.GetStringFromReader(oReader, "DNI"),
                        EMAIL = DataBase.GetStringFromReader(oReader, "EMAIL"),
                        EDAD = dateBirth,
                        
                        FECHAN = dateValue,
                        HORAMOD = DataBase.GetStringFromReader(oReader, "HORAMOD"),
                        IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA"),
                        MAILING = DataBase.GetStringFromReader(oReader, "MAILING"),
                        MAILING1 = DataBase.GetStringFromReader(oReader, "MAILING1"),
                        MAILING2 = DataBase.GetStringFromReader(oReader, "MAILING2"),
                        MAILING3 = DataBase.GetStringFromReader(oReader, "MAILING3"),
                        MAILING4 = DataBase.GetStringFromReader(oReader, "MAILING4"),
                        MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF"),
                        OTROS1 = DataBase.GetStringFromReader(oReader, "OTROS1"),
                        NUEVA_LOPD = DataBase.GetStringFromReader(oReader, "NUEVA_LOPD"),
                        OTROS3 = DataBase.GetStringFromReader(oReader, "OTROS3"),
                        OTROS4 = DataBase.GetStringFromReader(oReader, "OTROS4"),
                        OTROS5 = DataBase.GetStringFromReader(oReader, "OTROS5"),
                        OWNER = DataBase.GetIntFromReader(oReader, "OWNER"),
                        PACIENTE1 = DataBase.GetStringFromReader(oReader, "PACIENTE"),
                        POLIZA = DataBase.GetStringFromReader(oReader, "POLIZA"),
                        PROFESION = DataBase.GetStringFromReader(oReader, "PROFESION"),
                        RIP = DataBase.GetStringFromReader(oReader, "RIP"),
                        SEXO = (DataBase.GetStringFromReader(oReader, "SEXO") == "H" ? "H" : "M"),
                        TARJETA = DataBase.GetStringFromReader(oReader, "TARJETA"),
                        TELEFONOS = TelefonoRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID")),
                        TRAC = DataBase.GetShortFromReader(oReader, "TRAC"),
                        TRATAMIENTODESC = GetFriendlyTratamiento(DataBase.GetShortFromReader(oReader, "TRAC")),
                        USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME"),
                        VERS = DataBase.GetIntFromReader(oReader, "VERS"),
                        VIP = DataBase.GetStringFromReader(oReader, "VIP"),
                        RESPONSABLE = DataBase.GetStringFromReader(oReader, "RNOMBRE"),
                        DNIRESPONSABLE = DataBase.GetStringFromReader(oReader, "RDNI"),
                        COMPARTIR = DataBase.GetStringFromReader(oReader, "QRCOMPARTIRCASO"),
                        TIPO_DOC = DataBase.GetShortFromReader(oReader, "TIPO_DOC"),
                        TEXTO = TextosRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID")).TEXTO,
                        HAYCAMBIOSPACIENTE = "F",
                        //Añadimos los campos de permisos de LOPD
                        ENVIO_MEDICO = DataBase.GetStringFromReader(oReader, "ENVIO_MEDICO"),
                        ENVIO_RESULTADOS = DataBase.GetStringFromReader(oReader, "ENVIO_RESULTADOS"),
                        ENVIO_MAIL = DataBase.GetStringFromReader(oReader, "ENVIO_MAIL"),
                        ENVIO_SMS = DataBase.GetStringFromReader(oReader, "ENVIO_SMS"),
                        ENVIO_PROPAGANDA = DataBase.GetStringFromReader(oReader, "ENVIO_PROPAGANDA"),
                        LLAMADA_NOMBRE = DataBase.GetStringFromReader(oReader, "LLAMADA_NOMBRE"),
                        ACCESO_WEB = DataBase.GetStringFromReader(oReader, "ACCESO_WEB"),
                        CONSULTA_PREVIA = DataBase.GetStringFromReader(oReader, "CONSULTA_PREVIA")
                    };

                    if (String.IsNullOrEmpty(oPaciente.OTROS4))
                    {
                        oPaciente.OTROS4 = "C";
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



            return oPaciente;
        }

        public static List<PACIENTE> Lista(string nombre, bool conDirecciones = false)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();


            FbCommand oCommand;
            string query = "";
            query = "select first (30) p.*, M.CODMUT,M.NOMBRE from PACIENTE P LEFT JOIN mutuas M ON ( M.OID=p.CID) where paciente like '" + nombre.ToUpper() + "%'  ";
            query = query + " order by P.paciente";

            oCommand = new FbCommand(query, oConexion);



            FbDataReader oReader = oCommand.ExecuteReader();

            List<PACIENTE> lPaciente = new List<PACIENTE>();
            PACIENTE oPaciente;
            while (oReader.Read())
            {

                string dateBirth;

                if (oReader["FECHAN"] == DBNull.Value)
                {
                    dateBirth = "";
                }
                else
                {
                    dateBirth = DataBase.CalcularEdad((DateTime)oReader["FECHAN"]);
                }

                //MUTUAS oMutuaPaciente = MutuasRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "CID"));
                oPaciente = new PACIENTE
                {
                    OID = DataBase.GetIntFromReader(oReader, "OID"),
                    DESCMUTUA = DataBase.GetStringFromReader(oReader, "NOMBRE"),
                    CODMUTUA = DataBase.GetStringFromReader(oReader, "CODMUT"),
                    DNI = DataBase.GetStringFromReader(oReader, "DNI"),
                    EDAD = dateBirth,
                    FECHAN = DataBase.GetDateTimeFromReader(oReader, "FECHAN"),
                    OWNER = DataBase.GetIntFromReader(oReader, "OWNER"),
                    PACIENTE1 = DataBase.GetStringFromReader(oReader, "PACIENTE")

                };
                if (conDirecciones)
                {
                    oPaciente.DIRECCIONES = DireccionRepositorio.Obtener(oPaciente.OID);
                }
                lPaciente.Add(oPaciente);


            }
            oCommand.Dispose();
            oConexion.Close();
            return lPaciente;

        }

        public static List<PACIENTE> Lista(FiltrosBusquedaPaciente oFiltros, int NumRows = 50)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();


            FbCommand oCommand;
            string query = "";
            //query = "select FIRST(" + NumRows + ") p.*, (select count(*) from exploracion e where e.IOR_PACIENTE=p.OID) as EXPLORACIONES, ";
            //query += "(select fecha from exploracion where estado=3 and oid = (select max(ee.oid) from exploracion ee where ee.estado=3 and ee.IOR_PACIENTE = p.oid)) as ULTIMAREALIZADA, ";
            //query += "(select fecha from exploracion where estado=0 and oid = (select max(e2.oid) from exploracion e2 where e2.FECHA> '" + DateTime.Now.ToString("yyyy-MM-dd") + "' and e2.IOR_PACIENTE = p.oid)) as PROXIMACITA, ";
            //query += "M.CODMUT,M.NOMBRE from PACIENTE P LEFT JOIN mutuas M ON (M.OID=p.CID) join TELEFONO t on t.OWNER=p.OID";

            query = "select FIRST(" + NumRows + ") p.*, (select count(*) from exploracion e where e.IOR_PACIENTE=p.OID) as EXPLORACIONES, ";
            query += "(select fecha from exploracion where estado=3 and oid = (select max(ee.oid) from exploracion ee where ee.estado=3 and ee.IOR_PACIENTE = p.oid)) as ULTIMAREALIZADA, ";
            query += "(select fecha from exploracion where estado=0 and oid = (select max(e2.oid) from exploracion e2 where e2.FECHA> '" + DateTime.Now.ToString("yyyy-MM-dd") + "' and e2.IOR_PACIENTE = p.oid)) as PROXIMACITA, ";
            query += "M.CODMUT,M.NOMBRE from PACIENTE P LEFT JOIN mutuas M ON (M.OID=p.CID) left join TELEFONO t on t.owner=(select FIRST(1) oid from TELEFONO where owner = p.OID) ";

            if (oFiltros.Nombre != null)
            {

                string nombre = "";
                string apellidos = "";
                if (oFiltros.Nombre.Contains(","))
                {

                    nombre = oFiltros.Nombre.Split(',')[1].Trim();
                    apellidos = oFiltros.Nombre.Split(',')[0];
                    oFiltros.Nombre = apellidos + ", " + nombre;
                }

                query = query + " where paciente like '" + oFiltros.Nombre.ToUpper() + "%' ";


            }
            else
            {
                query = query + " where P.paciente is not null ";
            }

            if (oFiltros.Dni != null && !string.IsNullOrEmpty(oFiltros.Dni))
            {
                query = query + " and P.dni like '" + oFiltros.Dni.ToUpper() + "%'";
            }
            if (oFiltros.Telefono != null && !string.IsNullOrEmpty(oFiltros.Telefono))
            {
                query = query + " and P.oid in (select owner from TELEFONO where NUMERO like '" + oFiltros.Telefono + "%')";
            }


            if (oFiltros.Status == "VIP")
            {
                query = query + " and P.VIP='T'";
            }

            query = query + " order by P.paciente";

            oCommand = new FbCommand(query, oConexion);



            FbDataReader oReader = oCommand.ExecuteReader();

            List<PACIENTE> lPaciente = new List<PACIENTE>();
            PACIENTE oPaciente;
            while (oReader.Read())
            {

                string dateBirth;

                if (oReader["FECHAN"] == DBNull.Value)
                {
                    dateBirth = "";
                }
                else
                {
                    dateBirth = DataBase.CalcularEdad((DateTime)oReader["FECHAN"]);
                }

                //MUTUAS oMutuaPaciente = MutuasRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "CID"));
                oPaciente = new PACIENTE
                {
                    OID = DataBase.GetIntFromReader(oReader, "OID"),
                    CID = DataBase.GetIntFromReader(oReader, "CID"),
                    BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO"),
                    DESCMUTUA = DataBase.GetStringFromReader(oReader, "NOMBRE"),
                    CODMUTUA = DataBase.GetStringFromReader(oReader, "CODMUT"),
                    //DIRECCIONES = DireccionRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID")),
                    //TELEFONOS = TelefonoRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID")),
                    DNI = DataBase.GetStringFromReader(oReader, "DNI"),
                    EDAD = dateBirth,
                    FECHAN = DataBase.GetDateTimeFromReader(oReader, "FECHAN"),
                    OWNER = DataBase.GetIntFromReader(oReader, "OWNER"),
                    PACIENTE1 = DataBase.GetStringFromReader(oReader, "PACIENTE"),
                    PROFESION = DataBase.GetStringFromReader(oReader, "PROFESION"),
                    PROXIMACITA = DataBase.GetDateTimeFromReader(oReader, "PROXIMACITA"),
                    ULTIMAVISITAREALIZADA = DataBase.GetDateTimeFromReader(oReader, "ULTIMAREALIZADA"),
                    CUENTAVISITAS = DataBase.GetIntFromReader(oReader, "EXPLORACIONES"),

                    RIP = DataBase.GetStringFromReader(oReader, "RIP"),
                    CIP = DataBase.GetStringFromReader(oReader, "CIP"),
                    SEXO = (DataBase.GetStringFromReader(oReader, "SEXO") == "H" ? "Hombre" : "Mujer"),
                    TARJETA = DataBase.GetStringFromReader(oReader, "TARJETA"),
                    TRATAMIENTODESC = GetFriendlyTratamiento(DataBase.GetShortFromReader(oReader, "TRAC")),
                    VIP = DataBase.GetStringFromReader(oReader, "VIP"),
                    //EXPLORACIONES= ListaDiaRepositorio.ObtenerPorPaciente(DataBase.GetIntFromReader(oReader, "OID"))

                };
                lPaciente.Add(oPaciente);


            }
            oCommand.Dispose();
            oConexion.Close();
            return lPaciente;

        }

        public static void MarcarBorrar(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            FbCommand myCommand = new FbCommand();
            string updatequery = "UPDATE PACIENTE SET   BORRADO='T'";
            updatequery += " WHERE OID=" + oid;
            myCommand.CommandText = updatequery;
            myCommand.Connection = oConexion;


            myCommand.ExecuteNonQuery();

            oConexion.Close();

        }

        public static int UpdateCampo(string campo, string valor, int oid, string tipoCampo = "string")
        {


            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                //   string updateStament = "update paciente set " + campo + "='" + valor.ToUpper() + "'";
                string updateStament = "update paciente set " + campo + "=";
                if (tipoCampo != "string")
                {
                    updateStament += valor.ToUpper() + "";
                }
                else
                {
                    updateStament += "'" + valor.ToUpper() + "'";
                }
                updateStament += " where oid= " + oid;
                oCommand = new FbCommand(updateStament, oConexion);
                oCommand.ExecuteNonQuery();
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

            return oid;

        }

        public static int Update(PACIENTE oPaciente)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            string updatequery = "";
            string queryTexto = "";
            try
            {
                if (oPaciente.AVISO == "True" || oPaciente.AVISO == "T" || oPaciente.AVISO == "on")
                {
                    oPaciente.AVISO = "T";
                }
                else
                {
                    oPaciente.AVISO = "F";
                }
                if (oPaciente.VIP == "True" || oPaciente.VIP == "T" || oPaciente.VIP == "on")
                {
                    oPaciente.VIP = "T";
                }
                else
                {
                    oPaciente.VIP = "F";
                }
                //LOPDS
                if (oPaciente.NUEVA_LOPD == "True" || oPaciente.NUEVA_LOPD == "T" || oPaciente.NUEVA_LOPD == "on")
                {
                    oPaciente.NUEVA_LOPD = "T";
                }
                else
                {
                    oPaciente.NUEVA_LOPD = "F";
                }
                //LOPDS
                if (oPaciente.OTROS3 == "True" || oPaciente.OTROS3 == "T" || oPaciente.OTROS3 == "on")
                {
                    oPaciente.OTROS3 = "T";
                }
                else
                {
                    oPaciente.OTROS3 = "F";
                }
                if (oPaciente.RIP == "True" || oPaciente.RIP == "T" || oPaciente.RIP == "on")
                {
                    oPaciente.RIP = "T";
                }
                else
                {
                    oPaciente.RIP = "F";
                }
                if (oPaciente.COMPARTIR == "True" || oPaciente.COMPARTIR == "T" || oPaciente.COMPARTIR == "on")
                {
                    oPaciente.COMPARTIR = "T";
                }
                else
                {
                    oPaciente.COMPARTIR = "F";
                }
                //LOPD - Permisos por paciente
                if (oPaciente.ENVIO_MEDICO == "True" || oPaciente.ENVIO_MEDICO == "T" || oPaciente.ENVIO_MEDICO == "on")
                {
                    oPaciente.ENVIO_MEDICO = "T";
                }
                else
                {
                    oPaciente.ENVIO_MEDICO = "F";
                }

                if (oPaciente.ENVIO_RESULTADOS == "True" || oPaciente.ENVIO_RESULTADOS == "T" || oPaciente.ENVIO_RESULTADOS == "on")
                {
                    oPaciente.ENVIO_RESULTADOS = "T";
                }
                else
                {
                    oPaciente.ENVIO_RESULTADOS = "F";
                }

                if (oPaciente.ENVIO_MAIL == "True" || oPaciente.ENVIO_MAIL == "T" || oPaciente.ENVIO_MAIL == "on")
                {
                    oPaciente.ENVIO_MAIL = "T";
                }
                else
                {
                    oPaciente.ENVIO_MAIL = "F";
                }

                if (oPaciente.ENVIO_SMS == "True" || oPaciente.ENVIO_SMS == "T" || oPaciente.ENVIO_SMS == "on")
                {
                    oPaciente.ENVIO_SMS = "T";
                }
                else
                {
                    oPaciente.ENVIO_SMS = "F";
                }

                if (oPaciente.ENVIO_PROPAGANDA == "True" || oPaciente.ENVIO_PROPAGANDA == "T" || oPaciente.ENVIO_PROPAGANDA == "on")
                {
                    oPaciente.ENVIO_PROPAGANDA = "T";
                }
                else
                {
                    oPaciente.ENVIO_PROPAGANDA = "F";
                }
                //ACTUALIZACIÓN DEL PACIENTE
                DateTime fechaNacimiento;
                string cadenaFechaN = "";
                if (DateTime.TryParse(oPaciente.FECHAN.ToString(), out fechaNacimiento))
                {
                    cadenaFechaN = fechaNacimiento.ToString("MM-dd-yyyy");
                }

                if (!String.IsNullOrEmpty(oPaciente.DNI))
                {
                    oPaciente.DNI = oPaciente.DNI.ToUpper();
                }
                if (oPaciente.PACIENTE1.IndexOf(", ") < 0)
                {
                    oPaciente.PACIENTE1 = oPaciente.PACIENTE1.Replace(",", ", ");
                }
                updatequery = "UPDATE PACIENTE SET   CID=" + oPaciente.CID + ",PACIENTE='" + oPaciente.PACIENTE1.ToUpper() +
                "',FECHAN='" + cadenaFechaN + "', SEXO='" + oPaciente.SEXO + "',PROFESION='" + oPaciente.PROFESION +
                "',DNI='" + oPaciente.DNI + "', POLIZA='" + oPaciente.POLIZA + "'" +
                ",CIP= '" + oPaciente.CIP + "', AVISO='" + oPaciente.AVISO + "',TARJETA='" + oPaciente.TARJETA +
                "',EMAIL='" + oPaciente.EMAIL + "',VIP='" + oPaciente.VIP + "',RIP='" + oPaciente.RIP + "'" +
                ",NUEVA_LOPD='" + oPaciente.NUEVA_LOPD + "',OTROS4='" + oPaciente.OTROS4 + "'" + ",COMENTARIO='" + oPaciente.COMENTARIO + "'" +
                ",OTROS3='" + oPaciente.OTROS3 + "'" + ",QRCOMPARTIRCASO ='" + oPaciente.COMPARTIR + "'" +
                ",QRENLACEDIRECTO ='" + oPaciente.ENLACE + "'" +
                ",TIPO_DOC =" + oPaciente.TIPO_DOC +
                ",RDNI='" + oPaciente.DNIRESPONSABLE + "'" + ",RNOMBRE='" + oPaciente.RESPONSABLE + "'" +
                ",ENVIO_MEDICO = '" + oPaciente.ENVIO_MEDICO + "'" + ",ENVIO_RESULTADOS = '" + oPaciente.ENVIO_RESULTADOS + "'" + ",ENVIO_MAIL = '" + oPaciente.ENVIO_MAIL + "'" +
                ",ENVIO_SMS = '" + oPaciente.ENVIO_SMS + "'" + ",ENVIO_PROPAGANDA = '" + oPaciente.ENVIO_PROPAGANDA + "'" +
                " WHERE OID=" + oPaciente.OID;

                updatequery = updatequery.Replace("''", "null");

                oConexion.Open();
                //FbTransaction myTransaction = oConexion.BeginTransaction();
                FbCommand myCommand = new FbCommand();

                myCommand.CommandText = updatequery;
                myCommand.Connection = oConexion;
                //myCommand.Transaction = myTransaction;

                // Execute Insert
                int oidPaciente = (int)myCommand.ExecuteNonQuery();
                oConexion.Close();
                myCommand.Dispose();

                //este indizador es para cuando agreguemos multiples direcciones
                for (int i = 0; i < oPaciente.DIRECCIONES.Count; i++)
                {

                    //SI LA DIRECCIÓN ES UNA NUEVA INSERCION.
                    if (oPaciente.DIRECCIONES.ElementAt(i).OID == -1)
                    {
                        oPaciente.DIRECCIONES.ElementAt(i).OWNER = oPaciente.OID;
                        int oidDireccion = DireccionRepositorio.Insertar(oPaciente.DIRECCIONES.ElementAt(i));
                    }
                    else
                    {
                        DireccionRepositorio.Editar(oPaciente.DIRECCIONES.ElementAt(i));
                    }

                }

                List<TELEFONO> oListTelPacBD = TelefonoRepositorio.Obtener(oPaciente.OID);


                for (int j = 0; j < oListTelPacBD.Count; j++)
                {

                    try
                    {
                        //Buscamos en el objeto enviado desde la Página si coincide el oid, sino lo eliminamos
                        if (oListTelPacBD.Where(t => t.OID == oPaciente.TELEFONOS.ElementAt(j).OID).Count() == 0)
                        {
                            TelefonoRepositorio.Delete(oListTelPacBD[j].OID.ToString());
                        }

                    }
                    catch (Exception)
                    {


                    }

                }

                //este indizador es para cuando agreguemos multiples telefonos
                for (int i = 0; i < oPaciente.TELEFONOS.Count; i++)
                {

                    //SI EL TELEFONO ES UNA NUEVA INSERCION.
                    if (oPaciente.TELEFONOS.ElementAt(i).OID == -1)
                    {
                        oPaciente.TELEFONOS.ElementAt(i).OWNER = oPaciente.OID;
                        int oidTelefono = TelefonoRepositorio.Insertar(oPaciente.TELEFONOS.ElementAt(i));
                    }
                    else
                    {
                        TelefonoRepositorio.Editar(oPaciente.TELEFONOS.ElementAt(i));
                    }

                }




                myCommand = new FbCommand();
                myCommand.CommandText = queryTexto;
                myCommand.Connection = oConexion;
                oConexion.Open();
                if (oPaciente.TEXTO != null && oPaciente.TEXTO.Length > 0)
                {
                    string TextoPaciente = TextosRepositorio.Obtener(oPaciente.OID).TEXTO;

                    if (string.IsNullOrEmpty(TextoPaciente))
                    {
                        queryTexto = "insert into textos  (OID,OWNER,TEXTO) values (gen_id(GENUID,1),";
                        queryTexto += oPaciente.OID + ",'" + oPaciente.TEXTO + "')";
                        queryTexto = queryTexto.Replace("''", "null");
                    }
                    else
                    {
                        queryTexto = "UPDATE  Textos  SET TEXTO='" + oPaciente.TEXTO + "' WHERE owner=" + oPaciente.OID;
                        queryTexto = queryTexto.Replace("''", "null");
                    }

                }
                else
                {
                    queryTexto = "delete from textos WHERE owner=" + oPaciente.OID;

                }
                myCommand.CommandText = queryTexto;
                myCommand.ExecuteNonQuery();
                myCommand.Dispose();
                oConexion.Close();
                return oPaciente.OID;
            }
            catch (Exception ex)
            {
                LogException.LogMessageToFile(ex.Message);
                string logMessage = "UPDATE QUERY PACIENTE =  " + updatequery;
                logMessage = logMessage + "\nUPDATE TEXTO = " + queryTexto;
                string fic = System.Configuration.ConfigurationManager.AppSettings["RutaAPILog"] + @"\excepciones\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + oPaciente.OID + "_" + oPaciente.PACIENTE1 + ".log";
                System.IO.File.WriteAllText(fic, logMessage);
                return -1;
            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)
                {
                    oConexion.Close();
                }
            }

        }

        public static int InsertDesdeApiManresa(API.Paciente oPaciente)
        {
            int oidPaciente;

            //  FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBDTest"].ConnectionString);
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand myCommand = new FbCommand();
            try
            {
                //para evitar duplicar pacientes que se envian desde Manresa hemos decido usar el CIP  para almacenar el identificador clave del Sofware de manresa                
                string InsertPacienteQuery = "UPDATE OR INSERT INTO PACIENTE ( COD_PAC,CID,PACIENTE, FECHAN, SEXO,COMENTARIO,AVISO, CIP";
                InsertPacienteQuery += " )";
                InsertPacienteQuery += " VALUES ( gen_id(GENCODPAC, 1),3820080" + ",'" + oPaciente.Apellidos.ToUpper() + ", " + oPaciente.Nombre.ToUpper();
                InsertPacienteQuery += "','" + oPaciente.FechaNacimiento.ToString() + "','" + oPaciente.Sexo + "','CREADO DESDE " + oPaciente.Origen.ToUpper() + "','F'," + DataBase.QuotedString(oPaciente.IdManresa) + ")  MATCHING  (CIP)";
                InsertPacienteQuery += " returning OID";
                InsertPacienteQuery = InsertPacienteQuery.Replace("''", "null");
                InsertPacienteQuery = InsertPacienteQuery.Replace(",,", ",null,");
                myCommand.CommandText = InsertPacienteQuery;
                myCommand.Connection = oConexion;

                // Execute Insert
                oidPaciente = (int)myCommand.ExecuteScalar();


            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)
                {

                    if (myCommand != null)
                    {
                        myCommand.Dispose();
                    }
                    oConexion.Close();
                }


            }
            return oidPaciente;
        }



        public static string RemoveDiacritics(string input)
        {
            string stFormD = input.Normalize(NormalizationForm.FormD);
            int len = stFormD.Length;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[i]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[i]);
                }
            }
            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }



        public static int InsertDesdeApiCitaOnline(API.Paciente oPaciente)
        {
            int oidPaciente;


            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand myCommand = new FbCommand();
            FbCommand myCommand2 = null;
            try
            {
              
                oPaciente.Dni = oPaciente.Dni.ToUpper().Replace(" ", "").Replace("-", "");

                //para evitar duplicar pacientes que se envian desde Manresa hemos decido usar el dni  para almacenar el identificador clave del Software de citaonline                
                string InsertPacienteQuery = "UPDATE OR INSERT INTO PACIENTE ( COD_PAC,CID,PACIENTE, FECHAN, SEXO,COMENTARIO,AVISO,DNI,EMAIL)";
                InsertPacienteQuery += " VALUES ( gen_id(GENCODPAC, 1)," + oPaciente.IOR_MUTUA + ",'" + RemoveDiacritics(oPaciente.Apellidos.ToUpper()) + ", " + RemoveDiacritics(oPaciente.Nombre.ToUpper());
                InsertPacienteQuery += "','" + oPaciente.FechaNacimiento.ToString() + "','" + oPaciente.Sexo + "','CITAONLINE','F'," + DataBase.QuotedString(oPaciente.Dni) + "," + DataBase.QuotedString(oPaciente.Email) + ") " +
                    " MATCHING  (DNI,FECHAN)";
                InsertPacienteQuery += " returning OID";

                InsertPacienteQuery = InsertPacienteQuery.Replace("''", "null");
                InsertPacienteQuery = InsertPacienteQuery.Replace(",,", ",null,");
                myCommand.CommandText = InsertPacienteQuery;
                myCommand.Connection = oConexion;

                // Execute Insert
                oidPaciente = (int)myCommand.ExecuteScalar();

                string InsertTelefono = "INSERT INTO TELEFONO  (IOR_TIPO,OID,OWNER,numero) values (3664935,gen_id(GENUID,1), ";
                InsertTelefono += oidPaciente + ",'" + oPaciente.Telefono;
                InsertTelefono += "') returning OID";



                myCommand2 = new FbCommand();
                myCommand2.Connection = oConexion;
                //myCommand2.Transaction = myTransaction;
                myCommand2.CommandText = InsertTelefono;
                myCommand2.ExecuteNonQuery();

                //myTransaction.Commit();

            }
            catch (Exception ex)
            {
                LogException.LogMessageToFile(ex.Message);
                throw;
            }
            finally
            {
                LogException.LogMessageToFile("ENTRANDO AL FINALLY");
                if (oConexion.State == System.Data.ConnectionState.Open)
                {

                    if (myCommand != null)
                    {
                        myCommand.Dispose();
                    }
                    if (myCommand2 != null)
                    {
                        myCommand2.Dispose();
                    }
                    oConexion.Close();
                }

            }

            return oidPaciente;
        }
        public static int Insertar(PACIENTE oPaciente)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            //FbTransaction myTransaction = oConexion.BeginTransaction();
            FbCommand myCommand = new FbCommand();
            try
            {

                oPaciente.AVISO = (oPaciente.AVISO == "True") ? "T" : "F";
                oPaciente.VIP = (oPaciente.VIP == "True") ? "T" : "F";
                //LOPDS
                oPaciente.NUEVA_LOPD = (oPaciente.NUEVA_LOPD == "True") ? "T" : "F";
                //COMPARTIR
                oPaciente.OTROS3 = (oPaciente.OTROS3 == "True") ? "T" : "F";
                oPaciente.RIP = (oPaciente.RIP == "True") ? "T" : "F";
                //CAMPO OTROS 4 ES EL IDIOMA
                oPaciente.OTROS4 = (String.IsNullOrEmpty(oPaciente.OTROS4)) ? "C" : oPaciente.OTROS4;

                DateTime fechaNacimiento;
                string cadenaFechaN = "";
                if (DateTime.TryParse(oPaciente.FECHAN.ToString(), out fechaNacimiento))
                {
                    cadenaFechaN = fechaNacimiento.ToString("MM-dd-yyyy");
                }


                if (!String.IsNullOrEmpty(oPaciente.DNI))
                {
                    oPaciente.DNI = oPaciente.DNI.ToUpper();
                }
                if (!oPaciente.PACIENTE1.Contains(", "))
                {
                    if (oPaciente.PACIENTE1.Contains(","))
                    {
                        oPaciente.PACIENTE1 = oPaciente.PACIENTE1.Replace(",", ", ");
                    }
                }
                //Inserción Paciente
                string InsertPacienteQuery = "INSERT INTO PACIENTE ( OID,CID, COD_PAC,PACIENTE, FECHAN, SEXO,PROFESION,DNI, TRAC, POLIZA,";
                InsertPacienteQuery += "  CIP, AVISO,TARJETA,EMAIL,VIP,RIP,COMENTARIO,RDNI,RNOMBRE,NUEVA_LOPD,OTROS3,OTROS4,QRCOMPARTIRCASO,QRENLACEDIRECTO, ";
                InsertPacienteQuery += " TIPO_DOC)";
                InsertPacienteQuery += " VALUES (gen_id(GENUID,1), " + oPaciente.CID + ",  gen_id(GENCODPAC, 1)" + "," + DataBase.QuotedString(oPaciente.PACIENTE1.ToUpper());
                InsertPacienteQuery += "," + DataBase.QuotedString(cadenaFechaN) + "," + DataBase.QuotedString(oPaciente.SEXO);
                InsertPacienteQuery += "," + DataBase.QuotedString(oPaciente.PROFESION) + "," + DataBase.QuotedString(oPaciente.DNI);
                InsertPacienteQuery += "," + oPaciente.TRAC + "," + DataBase.QuotedString(oPaciente.POLIZA) + "," + DataBase.QuotedString(oPaciente.CIP);
                InsertPacienteQuery += "," + DataBase.QuotedString(oPaciente.AVISO) + "," + DataBase.QuotedString(oPaciente.TARJETA) + "," + DataBase.QuotedString(oPaciente.EMAIL);
                InsertPacienteQuery += "," + DataBase.QuotedString(oPaciente.VIP) + "," + DataBase.QuotedString(oPaciente.RIP) + "," + DataBase.QuotedString(oPaciente.COMENTARIO);
                InsertPacienteQuery += "," + DataBase.QuotedString(oPaciente.DNIRESPONSABLE) + "," + DataBase.QuotedString(oPaciente.RESPONSABLE);
                InsertPacienteQuery += "," + DataBase.QuotedString(oPaciente.NUEVA_LOPD) + "," + DataBase.QuotedString(oPaciente.OTROS3) + "," + DataBase.QuotedString(oPaciente.OTROS4);
                InsertPacienteQuery += "," + DataBase.QuotedString(oPaciente.COMPARTIR) + "," + DataBase.QuotedString(oPaciente.ENLACE) + "," + oPaciente.TIPO_DOC;
                InsertPacienteQuery += ")";
                InsertPacienteQuery += " returning OID";
                InsertPacienteQuery = InsertPacienteQuery.Replace("''", "null");
                InsertPacienteQuery = InsertPacienteQuery.Replace(",,", ",null,");
                myCommand.CommandText = InsertPacienteQuery;
                myCommand.Connection = oConexion;
                //myCommand.Transaction = myTransaction;
                // Execute Insert
                int oidPaciente = (int)myCommand.ExecuteScalar();

                //este indizador es para cuando agreguemos multiples direcciones
                for (int i = 0; i < oPaciente.DIRECCIONES.Count; i++)
                {
                    oPaciente.DIRECCIONES.ElementAt(i).OWNER = oidPaciente;
                    int oidDireccion = DireccionRepositorio.Insertar(oPaciente.DIRECCIONES.ElementAt(i));
                }

                //este indizador es para cuando agreguemos multiples direcciones
                for (int i = 0; i < oPaciente.TELEFONOS.Count; i++)
                {
                    oPaciente.TELEFONOS.ElementAt(i).OWNER = oidPaciente;
                    int oidTelefono = TelefonoRepositorio.Insertar(oPaciente.TELEFONOS.ElementAt(i));
                }

                string InsertTexto = "";
                if (oPaciente.TEXTO != null && oPaciente.TEXTO.Length > 0)
                {
                    InsertTexto = "INSERT INTO Textos  (OID,OWNER,TEXTO) values (gen_id(GENUID,1), ";
                    InsertTexto += oidPaciente + ",'" + oPaciente.TEXTO.ToUpper() + "') returning OID";
                    InsertTexto = InsertTexto.Replace("''", "null");

                    myCommand.CommandText = InsertTexto;
                    myCommand.ExecuteNonQuery();
                }

                return oidPaciente;
            }
            catch (Exception ex)
            {

                return -1;
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

        public static VMLOPDPaciente getLOPDsettings(int idPaciente)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select ENVIO_MEDICO, ENVIO_RESULTADOS, ENVIO_SMS, ENVIO_PROPAGANDA, ENVIO_MAIL, LLAMADA_NOMBRE, ACCESO_WEB, CONSULTA_PREVIA from PACIENTE where oid=" + idPaciente, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            VMLOPDPaciente lopdSettings = new VMLOPDPaciente();

            try
            {
                
                while (oReader.Read())
                {

                    lopdSettings = new VMLOPDPaciente
                    {
                        ENVIO_MEDICO = DataBase.GetStringFromReader(oReader, "ENVIO_MEDICO"),
                        ENVIO_RESULTADOS = DataBase.GetStringFromReader(oReader, "ENVIO_RESULTADOS"),
                        ENVIO_MAIL = DataBase.GetStringFromReader(oReader, "ENVIO_MAIL"),
                        ENVIO_SMS = DataBase.GetStringFromReader(oReader, "ENVIO_SMS"),
                        ENVIO_PROPAGANDA = DataBase.GetStringFromReader(oReader, "ENVIO_PROPAGANDA"),
                        LLAMADA_NOMBRE = DataBase.GetStringFromReader(oReader, "LLAMADA_NOMBRE"),
                        ACCESO_WEB = DataBase.GetStringFromReader(oReader, "ACCESO_WEB"),
                        CONSULTA_PREVIA = DataBase.GetStringFromReader(oReader, "CONSULTA_PREVIA")
                    };
                }
                return lopdSettings;
            }catch (Exception ex)
            {
                return lopdSettings;
            }finally
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
    }
}
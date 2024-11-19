using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using FastReport.Export.Odf;

namespace RadioWeb.Models.Repos
{
    public class WorkListRepositorio
    {

    

       
        public static string RutaXerox = System.Configuration.ConfigurationManager.AppSettings["RutaXerox"];
        public static string RutaXerox2 = System.Configuration.ConfigurationManager.AppSettings["RutaXerox2"];
        public static string RutaXerox3 = System.Configuration.ConfigurationManager.AppSettings["RutaXerox3"];
        public static string RutaXerox4 = System.Configuration.ConfigurationManager.AppSettings["RutaXerox4"];
        public static string RutaXerox5 = System.Configuration.ConfigurationManager.AppSettings["RutaXerox5"];
    

        /// <summary>
        /// Elimina los acentos de una cadena
        /// </summary>
        /// <param name="inputString">Texto con acentos</param>
        /// <returns>Texto sin acentos</returns>
        public static string QuitAccents(string inputString)
        {
            Regex a = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
            Regex e = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
            Regex i = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
            Regex o = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
            Regex u = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
            Regex n = new Regex("[ñ|Ñ]", RegexOptions.Compiled);
            inputString = a.Replace(inputString, "a");
            inputString = e.Replace(inputString, "e");
            inputString = i.Replace(inputString, "i");
            inputString = o.Replace(inputString, "o");
            inputString = u.Replace(inputString, "u");
            inputString = n.Replace(inputString, "n");
            return inputString;
        }
        public static string EscribeLinea(int oid,string fechaExploracion)
        {
            StringBuilder lineaResultado = new StringBuilder("");
             char C = (char)9;
            //query Necesaria para generar el Fichero
            string queryWorkList = "select e.OID, e.HORA, p.FECHAN, p.TRAC, p.SEXO ,p.PACIENTE,p.OID as OIDPACIENTE,p.FECHAN, a.DES_FIL, d.USERNAME AS MODALITY,c.NOMBRE,x.NOMBRE AS CENTRO, t.NOMBRE AS CENTROAPARATO, D.CID, D.IDENTIFICADOR, e.IOR_COLEGIADO ";
            queryWorkList += "from EXPLORACION e join PACIENTE p on p.OID=e.IOR_PACIENTE join APARATOS a on a.OID=e.IOR_TIPOEXPLORACION join DAPARATOS d on d.OID=e.IOR_APARATO ";
            queryWorkList += "left join COLEGIADOS c on c.OID=e.IOR_COLEGIADO left join CENTROSEXTERNOS X on X.OID=e.IOR_CENTROEXTERNO left join CENTROS T on T.OID=D.CID ";
            queryWorkList += "where e.OID=" + oid;

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            FbCommand oCommand = new FbCommand(queryWorkList, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            string centro = "";
            //string SEXO = "F";
            string SEXO = "";
            while (oReader.Read())
            {
                EXPLORACION oExplo = ExploracionRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID"));
                if (oExplo.EMPRESA.NOMBRE.Contains("DELFOS"))
                {

                    C = (char)'#';
                }

                /* Comentamos codigo antiguo de selección de Sexo, y miramos el campo sexo para determinar el sexo del paciente.
                string tratamiento = oReader["TRAC"].ToString();
                if (tratamiento == "1" || tratamiento == "2" || tratamiento == "3" || tratamiento == "8")
                {
                    SEXO = "M";
                }
                */

                SEXO = DataBase.GetStringFromReader(oReader, "SEXO");

                if (SEXO.Equals("H"))
                {
                    SEXO = "M";
                }else if (SEXO.Equals("M"))
                {
                    SEXO = "F";
                }

                string fechaNacimiento = DataBase.GetDateTimeFromReader(oReader, "FECHAN").ToString();
                DateTime dateValue;

                if (DateTime.TryParse(fechaNacimiento, out dateValue))
                {
                    fechaNacimiento = dateValue.ToString("yyyyMMdd");
                }
                else
                {
                    fechaNacimiento = "";
                }


                string Physician = QuitAccents(oReader["NOMBRE"].ToString()).Replace(", ", "^").Replace(".", " ").Replace(",", " ");
                if (Physician.Length == 0)
                {
                    Physician = "No doctor";
                }
                string nombreDelPaciente = "";
                nombreDelPaciente = QuitAccents(oReader["PACIENTE"].ToString());
                //SI NO HAY UN ESPACIO DESPUES DE LA COMA
                if (nombreDelPaciente.Substring(nombreDelPaciente.IndexOf(",") + 1, 1) != " ")
                {
                    nombreDelPaciente = nombreDelPaciente.Replace(",", ", ");
                }
                string Paciente = nombreDelPaciente.Replace(",", ", ").Replace(", ", "^").Replace(".", " ").Replace(",", " ").Replace("^ ", "^");
                string Identificador = DataBase.GetStringFromReader(oReader, "IDENTIFICADOR");
                string Descripcion = "";

                if (!(Identificador == "lx-mr" || Identificador == "HDXTMR02"))
                {
                    Descripcion = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    if (oExplo.EMPRESA.NOMBRE.Contains("DELFOS"))
                    {

                        Descripcion = Descripcion.Replace("+","_").Replace("."," ");

                    }
                }
                string NomMaquina = "";
                if (Identificador == "HDXTMR02")
                {
                    NomMaquina = Identificador;
                }
                else
                {

                    NomMaquina = Descripcion;
                }
                lineaResultado.Append("ISO_IR 100" + C);
                lineaResultado.Append(oid.ToString() + C);//ACCESION NUMBER
                lineaResultado.Append(Paciente + C);//NOMBRE DEL PACIENTE
                lineaResultado.Append(String.Concat(DataBase.GetIntFromReader(oReader, "OIDPACIENTE").ToString(), C));//PATIENT ID
                lineaResultado.Append(fechaNacimiento + C);//PATIENT birthday
                lineaResultado.Append(SEXO + C);//sexo
                lineaResultado.Append("NoAlert" + C);//medical Alert
                lineaResultado.Append("Work" + C);//contrastes alergia
                lineaResultado.Append("1.2.250.1.59.1.999.10." + DateTime.Parse(fechaExploracion).ToString("yyyyMMdd") + "." + oid.ToString() + C);//Stydy instance UID
                lineaResultado.Append(Physician + C);//Request Physician
                //lineaResultado.Append(DataBase.GetIntFromReader(oReader,"IOR_COLEGIADO").ToString() + C);//Refering physician CAMBIODO MAYO 2016
                lineaResultado.Append(DataBase.GetIntFromReader(oReader, "OID").ToString() + C);//Refering physician
                lineaResultado.Append(Descripcion + C);//REQUESTED PROCEDURE DESC
                lineaResultado.Append(DataBase.GetStringFromReader(oReader, "MODALITY") + C);//MODALITY
                lineaResultado.Append("Contraste" + C);//requested contrast agente
                lineaResultado.Append(Identificador + C);//ScheduledStationAETitle
                lineaResultado.Append(DateTime.Parse(fechaExploracion).ToString("yyyyMMdd") + C);//ScheduledProcedureStepStartDate
                lineaResultado.Append(DataBase.GetStringFromReader(oReader, "HORA").Replace(":", "") + "00" + C);//ScheduledProcedureStepStartTime
                lineaResultado.Append("DOCTOR" + C);//ScheduledPerformingPhysiciansName
                lineaResultado.Append(Descripcion + C);//ScheduledProcedureStepDescription
                lineaResultado.Append(oid.ToString() + C);//ScheduledProcedureStepID
                lineaResultado.Append(NomMaquina + C);//SceduledStationName
                lineaResultado.Append("ROOM1" + C);// //ScheduledProcedureStepLocation
                lineaResultado.Append("PreMed" + C);// //Premedication
                lineaResultado.Append(oid.ToString() + C);//RequestedProcedureID
                lineaResultado.Append("Normal" + C);// //RequestedProcedurePriority


            }
            try
            {

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
         
            return lineaResultado.ToString();
        }

        public static int CrearFicheroER7(bool General, string dia, int oid, string RutaWL, bool Forzado = false,string accion= "Presencia")
        {

            if (Directory.Exists(RutaWL))
            {

                string query = @"SELECT e.OID, p.FECHAN, p.TRAC, a.FIL AS CODPRUEBACDPI, a.DES_FIL AS PRUEBACDPI, d.USERNAME AS MODALITY, 
                                c.NOMBRE, x.NOMBRE AS CENTRO, t.NOMBRE AS CENTROAPARATO, d.CID, e.IOR_COLEGIADO, 
                                r.NOMBRE AS MEDICOINFORMANTE, c.COD_MED, d.IDENTIFICADOR, s.ACTO AS CODPRUEBASERAM, 
                                s.DESCRIPCION AS DESCRIPCIONSERAM, inf.FECHA AS FECHAINFORME, d.COD_FIL AS CODAPARATO, 
                                e.IOR_PACIENTE, m.CODMUT, e.MOTIVO, e.FECHA, e.HORA, e.HORA_EX, e.NHCAP, e.FECHAMAXENTREGA
                         FROM EXPLORACION e
                         JOIN PACIENTE p ON p.OID = e.IOR_PACIENTE
                         JOIN APARATOS a ON a.OID = e.IOR_TIPOEXPLORACION
                         LEFT JOIN SERAM_AFFIDEA s ON s.OID = a.IOR_AFFIDEA
                         JOIN DAPARATOS d ON d.OID = e.IOR_APARATO
                         LEFT JOIN COLEGIADOS c ON c.OID = e.IOR_COLEGIADO
                         LEFT JOIN CENTROSEXTERNOS x ON x.OID = e.IOR_CENTROEXTERNO
                         LEFT JOIN CENTROS t ON t.OID = d.CID
                         LEFT JOIN MUTUAS m ON m.OID = e.IOR_ENTIDADPAGADORA
                         LEFT JOIN PERSONAL r ON r.OID = e.IOR_MEDICO
                         LEFT JOIN INFORMES inf ON inf.OWNER = e.OID
                         WHERE e.OID = @pOID";

                EXPLORACION oExplo = ExploracionRepositorio.Obtener(oid);
                string SEXO = oExplo.PACIENTE.SEXO == "H" ? "M" : oExplo.PACIENTE.SEXO == "M" ? "F" : "";
                string nombreDelPaciente = QuitAccents(oExplo.PACIENTE.PACIENTE1.ToString());
                if (nombreDelPaciente.Substring(nombreDelPaciente.IndexOf(",") + 1, 1) != " ")
                {
                    nombreDelPaciente = nombreDelPaciente.Replace(",", ", ");
                }
                string nombreCompleto = oExplo.PACIENTE.PACIENTE1;
                string Paciente = nombreDelPaciente.Replace(",", ", ").Replace(", ", "^").Replace(".", " ").Replace(",", " ").Replace("^ ", "^");
                string nombre = nombreCompleto.Substring(nombreCompleto.IndexOf(',') + 1).Trim();
                string apellidos = nombreCompleto.Substring(0, nombreCompleto.IndexOf(',')).Trim();
                int lastSpaceIndex = apellidos.LastIndexOf(' ');
                string apellido1 = apellidos.Substring(0, lastSpaceIndex).Trim() + "^" + nombre;
                string apellido2 = apellidos.Substring(lastSpaceIndex + 1).Trim();
                var primeraDireccion = oExplo.PACIENTE.DIRECCIONES?.FirstOrDefault();

                string direccion = primeraDireccion?.DIRECCION1 ?? string.Empty;
                string provincia = primeraDireccion?.PROVINCIA ?? string.Empty;
                string poblacion = primeraDireccion?.POBLACION ?? string.Empty;
                string cp = primeraDireccion?.CP ?? string.Empty;
                var primeraTelefono = oExplo.PACIENTE.TELEFONOS?.FirstOrDefault();
                string telefono = primeraTelefono?.NUMERO ?? string.Empty;
                using (FbConnection connection = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString)
)
                {
                    FbCommand command = new FbCommand(query, connection);
                    command.Parameters.AddWithValue("@pOID", oid);

                    connection.Open();
                    FbDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string estado = "";
                        string estado2 = "";

                        if (accion == "Nuevo") // AL CREARLA
                        {
                            estado = "NW";
                            estado2 = "SC";
                        }
                        else if (accion == "Presencia") // PRESENCIA
                        {
                            estado = "NW";
                            estado2 = "IP";
                        }
                        else if (accion == "Modificado") // MODIFICACIÓN
                        {
                            estado = "XO";
                            estado2 = "SC";
                        }
                        else if (accion == "Cancelado") // CANCELADA
                        {
                            estado = "CA";
                            estado2 = "CA";
                        }
                        else if (accion == "Realizado") // FINALIZADO
                        {
                            estado = "SC";
                            estado2 = "CM";
                        }

                   


                        // Obtener valores de la consulta
                        string fechaHoraActual = DateTime.Now.ToString("yyyyMMddHHmmss");
                        string vOIDPACIENTE = reader["IOR_PACIENTE"].ToString();
                        string vDNI = oExplo.PACIENTE.DNI;
                        string vAPELLIDO1 = apellido1;
                        string vAPELLIDO2 = apellido2; // Asignar valor según tu lógica de apellidos
                        string vFECHANACIMIENTO = reader["FECHAN"] == DBNull.Value
                         ? "18991230"
                         : Convert.ToDateTime(reader["FECHAN"]).ToString("yyyyMMdd");

                        string vSEXO = SEXO;
                        string vDIRECCION = direccion; // Completa según los datos disponibles
                        string vPOBLACION = poblacion; // Completa según los datos disponibles
                        string vCPOSTAL = cp  ; // Completa según los datos disponibles
                        string vTELEFONO = telefono; // Completa según los datos disponibles
                        string vAGENDA = reader["CODAPARATO"].ToString();
                        string vCENTRO = reader["CENTROAPARATO"].ToString();
                        string vESTADO = estado;
                        string vESTADO2 = estado2;
                        string vEmail = oExplo.PACIENTE.EMAIL;
                        string vIDAUTORIZACION = oExplo.NHCAP;
                        string vACCESSIONNUMBER = reader["OID"].ToString();
                        string vFECHAINFORME = fechaHoraActual;
                        string vCODMEDICOPETICION = "^^";
                        string vAETITLE = reader["IDENTIFICADOR"].ToString();
                        string vCODMUTUA = reader["CODMUT"].ToString();
                        string vCODPRUEBA = reader["CODPRUEBASERAM"].ToString();
                        string vDESCRIPCIONPRUEBA = reader["DESCRIPCIONSERAM"].ToString();
                        string vMOTIVO =oExplo.MOTIVO;
                        string vAPELLIDOSMEDICOPETICION = "";
                        string vNombreMEDICOPETICION = "";
                        if (!String.IsNullOrEmpty(oExplo.COLEGIADO.NOMBRE))
                        {
                            string[] nombreCompletoColegiado = oExplo.COLEGIADO.NOMBRE.Split(',');

                            string apellidosColegiado = nombreCompletoColegiado.Length > 0 ? nombreCompletoColegiado[0].Trim() : "";
                            string nombreColegiado = nombreCompletoColegiado.Length > 1 ? nombreCompletoColegiado[1].Trim() : "";

                            vAPELLIDOSMEDICOPETICION = apellidosColegiado;
                            vNombreMEDICOPETICION= nombreColegiado;
                        }
                        string vIDTARJETA = oExplo.PACIENTE.TARJETA;

                        if (!string.IsNullOrWhiteSpace(vIDTARJETA))
                        {
                            vIDTARJETA = "~" + vIDTARJETA.Trim() + "^^^HIS^PI^^^^" + vCODMUTUA;
                        }
                        else if (!string.IsNullOrWhiteSpace(oExplo.PACIENTE.POLIZA))
                        {
                            vIDTARJETA = "~" + oExplo.PACIENTE.POLIZA.Trim() + "^^^HIS^PI^^^^" + vCODMUTUA;
                        }
                        else
                        {
                            vIDTARJETA = string.Empty;
                        }

                        string SERARM;
                      

                        if (!string.IsNullOrWhiteSpace(vCODPRUEBA))
                        {
                            SERARM = "99SERARM";                        
                         
                        }
                        else
                        {
                            SERARM = reader["CODPRUEBACDPI"].ToString();
                         
                        }


                        
                        string vMODALIDAD = reader["MODALITY"].ToString();
                      
                        string vMEDICOINFORME = reader["MEDICOINFORMANTE"].ToString();
                        string vFECHAINICIO = Convert.ToDateTime(reader["FECHA"]).ToString("yyyyMMdd") + reader["HORA"].ToString().Replace(":", "") + "00";
                        string vFECHAFIN = Convert.ToDateTime(reader["FECHA"]).ToString("yyyyMMdd") + reader["HORA_EX"].ToString().Replace(":", "") + "00";

                        // Ruta del archivo de plantilla
                        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HL7_MACROS_WL.er7");
                        if (!File.Exists(templatePath))
                        {
                            throw new FileNotFoundException("El archivo de plantilla HL7_MACROS_WL.er7 no se encontró en la raíz del sitio.");
                        }

                        // Leer la plantilla
                        string hl7Template = File.ReadAllText(templatePath);

                        // Reemplazos en la plantilla
                        string hl7Message = hl7Template
                            .Replace("vFECHAHOY", fechaHoraActual.Trim())
                            .Replace("vCODIGO", String.Concat(oid.ToString(), fechaHoraActual.Trim()))
                            .Replace("vOIDPACIENTE", vOIDPACIENTE.Trim())
                            .Replace("vIDTARJETA", vIDTARJETA.Trim())
                            .Replace("vDNI", vDNI.Trim())
                            .Replace("vAPELLIDO1", vAPELLIDO1.Trim())
                            .Replace("vAPELLIDO2", vAPELLIDO2.Trim())
                            .Replace("vFECHANACIMIENTO", vFECHANACIMIENTO.Trim())
                            .Replace("vFECHAMAXENTREGA", oExplo.FECHAMAXENTREGA.HasValue ? oExplo.FECHAMAXENTREGA.Value.ToString("yyyyMMdd") : "")
                            .Replace("vPRIMERINFORME", "C")                            
                            .Replace("vSEXO", vSEXO.Trim())
                           .Replace("vDIRECCION", direccion)
                           .Replace("vEMAIL", vEmail)
                             .Replace("vPOBLACION", poblacion)
                             .Replace("vPROVINCIA", provincia)
                             .Replace("vCPOSTAL", cp)
                            .Replace("vTELEFONO", telefono)
                            .Replace("vTELEFONO", vTELEFONO.Trim())
                            .Replace("vAGENDA", vAGENDA.Trim())
                            .Replace("vNOMBREMEDICOPETICION", vNombreMEDICOPETICION.Trim())
                            .Replace("vAPELLIDOSMEDICOPETICION", vAPELLIDOSMEDICOPETICION.Trim())                            
                            .Replace("vIDAUTORIZACION", vIDAUTORIZACION.Trim())
                            .Replace("vCENTRO", vCENTRO.Trim())
                            .Replace("vESTADO",estado)
                            .Replace("v2ESTADO", estado2)
                            .Replace("vMOTIVO", vMOTIVO)
                            .Replace("vACCESSIONNUMBER", vACCESSIONNUMBER.Trim())
                            .Replace("vFECHAINFORME", vFECHAINFORME.Trim())
                            .Replace("vCODMEDICOPETICION", vCODMEDICOPETICION.Trim())
                            .Replace("vSERARM", SERARM)
                            .Replace("vAETITLE", vAETITLE.Trim())
                            .Replace("vCODMUTUA", vCODMUTUA.Trim())
                            .Replace("vCODPRUEBA", vCODPRUEBA.Trim())
                            .Replace("vDESCRIPCIONPRUEBA", vDESCRIPCIONPRUEBA.Trim())
                            .Replace("vMODALIDAD", vMODALIDAD.Trim())
                            .Replace("vMEDICOINFORME", vMEDICOINFORME.Trim())
                            .Replace("vFECHAINICIO", vFECHAINICIO.Trim())
                            .Replace("vFECHAFIN", vFECHAFIN.Trim());

                        // Ruta del archivo y nombre
                        string rutaArchivo = Path.Combine(RutaWL, $"{oid}_{fechaHoraActual}.er7");

                        // Generar el archivo
                        File.WriteAllText(rutaArchivo, hl7Message);
                    }
                }
            }
            return 1;

        }

        public static int CrearFicheroWL(bool General, string dia, int oid, string RutaWL, bool Forzado = false)
        {           

            if (Directory.Exists(RutaWL)) {
                char C = (char)9;
                EXPLORACION oExplo = ExploracionRepositorio.Obtener(oid);
            
                RutaWL = RutaWL + @"\" + DateTime.Parse(dia).ToString("yyyyMMdd") + "_E_" + oid.ToString() + ".WL";
                    


                string Cabecera = "SpecificCharacterSet" + C + "AccessionNumber" + C + "PatientsName" + C + "PatientID" + C +
                    "PatientsBirthDate" + C + "PatientsSex" + C + "MedicalAlerts" + C + "ContrastAllergies" + C +
                    "StudyInstanceUID" + C + "RequestingPhysician" + C + "ReferringPhysiciansName" + C +
                    "RequestedProcedureDescription" + C +
                    "Modality" + C + "RequestedContrastAgent" + C + "ScheduledStationAETitle" + C +
                    "ScheduledProcedureStepStartDate" + C +
                    "ScheduledProcedureStepStartTime" + C + "ScheduledPerformingPhysiciansName" + C +
                    "ScheduledProcedureStepDescription" + C +
                    "ScheduledProcedureStepID" + C + "ScheduledStationName" + C +
                    "ScheduledProcedureStepLocation" + C + "PreMedication" + C +
                    "RequestedProcedureID" + C + "RequestedProcedurePriority";

                System.IO.StreamWriter sw = new System.IO.StreamWriter(RutaWL);
                try
                {
                    sw.WriteLine(Cabecera);
                    sw.WriteLine(EscribeLinea(oid, dia));
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    sw.Close();
                }

                
            }

            return 1;
            
        }

        public static int BorrarFicheroWL(LISTADIA oExploracion,string RutaWL)
        {
            try
            {
                string fic = RutaWL;
                EXPLORACION oExplo = ExploracionRepositorio.Obtener(oExploracion.OID);
             
                 fic = fic + @"\" + oExploracion.FECHA.ToString("yyyyMMdd") + "_E_" + oExploracion.OID.ToString() + ".WL";
                if (File.Exists(fic))
                {
                System.IO.File.Delete(fic);
                }
                
            }
            catch (Exception)
            {
                
                throw;
            }
            return 1;
        }
        public static int BorrarFicheroWL(EXPLORACION oExploracion,string RutaWL)
        {
            try
            {
                string fic = RutaWL;               
           
                 fic = RutaWL + @"\" + oExploracion.FECHA.Value.ToString("yyyyMMdd") + "_E_" + oExploracion.OID.ToString() + ".WL";
                if (File.Exists(fic))
                {
                    System.IO.File.Delete(fic);
                }

            }
            catch (Exception)
            {

                throw;
            }
            return 1;
        }




    }
}
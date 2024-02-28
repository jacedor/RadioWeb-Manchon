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

        public static int CrearFicheroWL(bool General, string dia, int oid, string RutaWL, bool Forzado = false)
        {           

            if (Directory.Exists(RutaWL)) {
                char C = (char)9;
                EXPLORACION oExplo = ExploracionRepositorio.Obtener(oid);
                if (oExplo.EMPRESA.NOMBRE.Contains("DELFOS"))
                {
                    CENTROS oCentro = CentrosRepositorio.Obtener(oExplo.OWNER.Value);
                  
                    if (oCentro.NOMBRE.ToUpper().Contains("DELFOS"))
                    {
                        RutaWL = RutaWL + @"\delfos\";
                    }
                    else
                    {
                        RutaWL = RutaWL + @"\corachan\";
                    }
                }
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
                if (oExplo.EMPRESA.NOMBRE.Contains("DELFOS"))
                {
                    CENTROS oCentro = CentrosRepositorio.Obtener(oExplo.OWNER.Value);
                    if (oCentro.NOMBRE.ToUpper().Contains("DELFOS"))
                    {
                        fic = fic + @"\delfos\";
                    }
                    else
                    {
                        fic = fic + @"\corachan\";
                    }
                }
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
               
                if (oExploracion.EMPRESA.NOMBRE.Contains("DELFOS"))
                {
                    CENTROS oCentro = CentrosRepositorio.Obtener(oExploracion.OWNER.Value);
                    if (oCentro.NOMBRE.ToUpper().Contains("DELFOS"))
                    {
                        fic = fic + @"\delfos\";
                    }
                    else
                    {
                        fic = fic + @"\corachan\";
                    }
                }
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
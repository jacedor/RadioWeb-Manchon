using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{

    public class HorasHorarioRepositorio
    {


        public static List<HORAS_HORARIO> Lista(string FECHA, int IOR_APARATO)
        {
            FbCommand oComandHorario = null;

            FbCommand oComandHorasHorario = null;

            FbCommand oComandHorasAnuladas= null;

            List<HORAS_HORARIO> lHorasHorario = new List<HORAS_HORARIO>();
            List<HORASANULADAS> lHorasAnuladas = new List<HORASANULADAS>();
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            if (IOR_APARATO > 0)
            {
                string queryHorario = "select F.OID, F.FINICIO,F.FFIN, F.TIPO from FECHA_HORARIO F JOIN DAPA_HORARIO D ON (D.OID=F.IOR_HORARIO) WHERE D.IOR_APARATO= '";
                queryHorario += IOR_APARATO + "' AND (F.FINICIO <='" + DateTime.Parse(FECHA).ToString("yyyy-MM-dd HH:mm:ss") + "')";
                queryHorario += " AND (F.FFIN >='" + DateTime.Parse(FECHA).ToString("yyyy-MM-dd HH:mm:ss") + "') ORDER BY F.TIPO DESC,F.FFIN,F.FINICIO DESC";
                oComandHorario = new FbCommand(queryHorario, oConexion);
                FbDataReader oReaderHorario = oComandHorario.ExecuteReader( );
                try
                {
                    if (oReaderHorario.Read())
                    {
                        int oIDHorarioActivo = DataBase.GetIntFromReader(oReaderHorario, "OID");
                        oReaderHorario.Close();
                        int daySemana = ((int)DateTime.Parse(FECHA).Date.DayOfWeek + 1);
                        string queryHoras_Horario = "SELECT HORA, COLOR, TEXTODEFECTO from HORAS_HORARIO WHERE NDIA=" + daySemana + " AND IOR_FECHAHORARIO=" + oIDHorarioActivo + " ORDER BY HORA";
                        oComandHorasAnuladas = new FbCommand(queryHoras_Horario, oConexion);
                        FbDataReader oReaderHorasHorario = oComandHorasHorario.ExecuteReader();
                        string queryHoras_Anuladas = "select IOR_APARATO, FECHA, HORA,COMENTARIO from HORASANULADAS where FECHA='" + DateTime.Parse(FECHA).ToString("yyyy-MM-dd HH:mm:ss") + "' and IOR_APARATO=" + IOR_APARATO + " order by HORA ";

                        oComandHorasAnuladas = new FbCommand(queryHoras_Anuladas, oConexion);

                        FbDataReader oReaderHorasAnuladas = oComandHorasAnuladas.ExecuteReader();

                        //rellenamos la lista de horas anuladas
                        while (oReaderHorasAnuladas.Read())
                        {
                            HORASANULADAS oHorasAnuladas = new HORASANULADAS();
                            oHorasAnuladas.FECHA = DateTime.Parse(FECHA);
                            oHorasAnuladas.COMENTARIO = DataBase.GetStringFromReader(oReaderHorasAnuladas, "COMENTARIO");
                            oHorasAnuladas.HORA = DataBase.GetStringFromReader(oReaderHorasAnuladas, "HORA");

                            lHorasAnuladas.Add(oHorasAnuladas);
                        }
                        //rellenamos la lista de horas del horario
                        while (oReaderHorasHorario.Read())
                        {
                            HORAS_HORARIO oHorasDelHorario = new HORAS_HORARIO();
                            oHorasDelHorario.FECHA = DateTime.Parse(FECHA);
                            oHorasDelHorario.COLOR = DataBase.GetStringFromReader(oReaderHorasHorario, "COLOR")
                                .Replace("cl", "")
                                .Replace("$008000FF", "#DF01A5")
                                .Replace("$000080FF", "#DF7401")
                                .Replace("$00408000","Green")
                                .Replace("$FFFFFFFF","Black")
                                .Replace("$00400000","Black")
                                .Replace("$00004000","Black")
                                .Replace("$00804000","marron")
                                .Replace("$00A00000","Purple");

                            oHorasDelHorario.TEXTODEFECTO = DataBase.GetStringFromReader(oReaderHorasHorario, "TEXTODEFECTO");
                            oHorasDelHorario.HORA = DataBase.GetStringFromReader(oReaderHorasHorario, "HORA");
                            lHorasHorario.Add(oHorasDelHorario);
                        }

                        foreach (var horaanulada in lHorasAnuladas)
                        {
                            foreach (var horaHorario in lHorasHorario)
                            {
                                if (horaanulada.HORA == horaHorario.HORA)
                                {
                                    horaHorario.TEXTODEFECTO = horaanulada.COMENTARIO;
                                    horaHorario.COLOR = "Red";
                                    horaHorario.ANULADA = true;
                                    break;
                                }
                            }
                        }
                    }


                }
                catch (Exception)
                {

                }
                finally
                {
                    if (oConexion.State == System.Data.ConnectionState.Open)
                    {


                        oConexion.Close();
                        if (oComandHorasAnuladas != null)
                        {
                            oComandHorasAnuladas.Dispose();
                        }
                        if (oComandHorasHorario != null)
                        {
                            oComandHorasHorario.Dispose();
                        }
                        if (oComandHorario != null)
                        {
                            oComandHorario.Dispose();
                        }
                  
                        
                    }
                }








            }
            return lHorasHorario;
        }

        public static HORAS_HORARIO Obtener(string FECHA, int IOR_APARATO, string HHORA)
        {
            FbCommand oComandHorario = null;

            FbCommand oComandHorasHorario = null;

          
            List<HORAS_HORARIO> lHorasHorario = new List<HORAS_HORARIO>();
            List<HORASANULADAS> lHorasAnuladas = new List<HORASANULADAS>();
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            if (IOR_APARATO > 0)
            {
                string queryHorario = "select F.OID, F.FINICIO,F.FFIN, F.TIPO from FECHA_HORARIO F JOIN DAPA_HORARIO D ON (D.OID=F.IOR_HORARIO) WHERE D.IOR_APARATO= '";
                queryHorario += IOR_APARATO + "' AND (F.FINICIO <='" + DateTime.Parse(FECHA).ToString("yyyy-MM-dd HH:mm:ss") + "')";
                queryHorario += " AND (F.FFIN >='" + DateTime.Parse(FECHA).ToString("yyyy-MM-dd HH:mm:ss") + "') ORDER BY F.TIPO DESC,F.FFIN,F.FINICIO DESC";
               
                oComandHorario = new FbCommand(queryHorario, oConexion);
                FbDataReader oReaderHorario = oComandHorario.ExecuteReader();
                if (oReaderHorario.Read())
                {
                    int oIDHorarioActivo = DataBase.GetIntFromReader(oReaderHorario, "OID");
                    oReaderHorario.Close();
                    int daySemana = ((int)DateTime.Parse(FECHA).Date.DayOfWeek + 1);
                    string queryHoras_Horario = "SELECT HORA, COLOR, TEXTODEFECTO from HORAS_HORARIO WHERE NDIA=" + daySemana + " AND IOR_FECHAHORARIO=" + oIDHorarioActivo + " and hora=" + DataBase.QuotedString(HHORA) + " ORDER BY HORA";

                    oComandHorasHorario = new FbCommand(queryHoras_Horario, oConexion);
                    FbDataReader oReaderHorasHorario = oComandHorario.ExecuteReader();

                    
                    try
                    {
                        lHorasAnuladas = HorasAnulasRepositorio.Obtener(FECHA, IOR_APARATO, HHORA);

                        //rellenamos la lista de horas del horario
                        while (oReaderHorasHorario.Read())
                        {
                            HORAS_HORARIO oHorasDelHorario = new HORAS_HORARIO();
                            oHorasDelHorario.COLOR = DataBase.GetStringFromReader(oReaderHorasHorario, "COLOR").Replace("cl", "");
                            oHorasDelHorario.TEXTODEFECTO = DataBase.GetStringFromReader(oReaderHorasHorario, "TEXTODEFECTO");
                            oHorasDelHorario.HORA = DataBase.GetStringFromReader(oReaderHorasHorario, "HORA");
                            lHorasHorario.Add(oHorasDelHorario);
                        }

                        foreach (var horaanulada in lHorasAnuladas)
                        {
                            foreach (var horaHorario in lHorasHorario)
                            {
                                if (horaanulada.HORA == horaHorario.HORA)
                                {
                                    horaHorario.TEXTODEFECTO = horaanulada.COMENTARIO;
                                    horaHorario.COLOR = "Red";
                                    horaHorario.ANULADA = true;
                                    break;
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
                           
                            if (oComandHorasHorario != null)
                            {
                                oComandHorasHorario.Dispose();
                            }
                            if (oComandHorario != null)
                            {
                                oComandHorario.Dispose();
                            }

                        }
                    }


                }
            }
            if (lHorasHorario.Count > 0)
            {
                return lHorasHorario.First();
            }
            else
            {
                return null;
            }


        }
    }
}
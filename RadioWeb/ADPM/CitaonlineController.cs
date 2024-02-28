using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RadioWeb.ADPM
{
    public class CitaonlineController : ApiController
    {
        private enum Operacion
        {
            BUSCARHUECOREQUEST = 1,
            BUSCARHUECORESPONSE = 2,
            RESERVARHUECOREQUEST = 3,
            RESERVARHUECORESPONSE = 4,
            CANCELARREQUEST = 5,
            CANCELARESPONSE = 6

        }

        private void HttpGet(string URI, string parametros)
        {

            try
            {
                WebRequest myWebRequest = WebRequest.Create(URI + parametros);

                // Set the 'Timeout' property in Milliseconds.
                myWebRequest.Timeout = 10000;
                // This request will throw a WebException if it reaches the timeout limit before it is able to fetch the resource.
                WebResponse myWebResponse = myWebRequest.GetResponse();
                myWebResponse.Close();

            }
            catch (WebException e)
            {


            }

        }




        private string GenerateGUID(Operacion iOperacion, string centro = "")
        {
            string sPrefix = "";
            switch (iOperacion)
            {
                case Operacion.BUSCARHUECOREQUEST:
                    sPrefix = "BE";
                    break;
                case Operacion.BUSCARHUECORESPONSE:
                    sPrefix = "BS";
                    break;
                case Operacion.RESERVARHUECOREQUEST:
                    sPrefix = "RE";
                    break;
                case Operacion.RESERVARHUECORESPONSE:
                    sPrefix = "RS";
                    break;
                case Operacion.CANCELARREQUEST:
                    sPrefix = "CE";
                    break;
                case Operacion.CANCELARESPONSE:
                    sPrefix = "CS";
                    break;
            }


            return sPrefix + System.Guid.NewGuid().ToString() + centro + ".ini";


        }




        // GET api/citaonline
        public List<HUECO> Get(string origenPeticion, string fecha, string Centro, string codigoGrupo, string codigoActo, 
            string idEspecialidad, bool claustrofobia,int centroExternoOid, bool COLOPERADA,int oidMutua)
        {
            List<HUECO> oListaResult = new List<HUECO>();

            if ((codigoGrupo + codigoActo.Trim() == "RX ORT" && Centro == "MERIDIANA")
                || (codigoGrupo + codigoActo.Trim() == "RX ORT" && Centro == "BALMES")
                || string.IsNullOrWhiteSpace(codigoActo))
            {
                oListaResult.Add(new HUECO { CENTRO = Centro });
                return oListaResult;
            }

            if ((codigoGrupo + codigoActo.Trim() == "RX CC" && Centro != "TIBIDABO")
                || (codigoGrupo + codigoActo.Trim() == "RX CCP" && Centro != "TIBIDABO")
                || (codigoGrupo + codigoActo.Trim() == "RX CC1" && Centro != "TIBIDABO")
                || (codigoGrupo + codigoActo.Trim() == "RX TEF" && Centro != "TIBIDABO"))
            {
                oListaResult.Add(new HUECO { CENTRO = Centro });
                return oListaResult;
            }

            string ipAddress = System.Web.HttpContext.Current.Request.UserHostAddress;
            string fechaInicio = DateTime.Parse(fecha).ToString("dd/MM/yyyy");
            string HoraInicio = DateTime.Parse(fecha).ToString("HH:mm");

            LOGCITAONLINE oLog = new LOGCITAONLINE
            {
                FECHA = DateTime.Now.ToString("MM/dd/yyyy"),
                HORA = DateTime.Now.ToString("HH:MM"),
                TEXTO = ipAddress,
                GRUPO = codigoGrupo.Trim().PadRight(3, ' '),
                EXPLORACION = codigoActo.Trim(),
                OWNER=1, 
                CID= 0,               
                MODIF= DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"),
                APARATO= "null" ,
                USERNAME= "ADPMONLINE"               
                               
            };

           
            string query = "";
            //oListaResult = HorasLibreRepositorio.Lista(Centro,
            //    codigoGrupo.Trim().PadRight(3, ' ') + codigoActo.Trim(), 
            //    fechaInicio, 
            //    HoraInicio, 
            //    claustrofobia,
            //    COLOPERADA,
            //    ref query);

            oListaResult = HorasLibreRepositorio.ListaDesdePeticiones(Centro,
                codigoGrupo.Trim().PadRight(3, ' ') + codigoActo.Trim(),
                fechaInicio,
                HoraInicio,
                claustrofobia,
                COLOPERADA,
                oidMutua,
                ref query);
            oLog.TEXTO = oLog.TEXTO + " - " + query;
            //por petición de massana solo guardamos en el log una de las cuatro peticiones
            //en este caso la del centro CDPI
            if (oLog.TEXTO.Contains("CDPI"))
            {
                if (centroExternoOid > 0)
                {
                    oLog.OWNER = centroExternoOid;
                    oLog.USERNAME = centroExternoOid.ToString();
                }
                LogCitaOnlineRepositorio.Insertar(oLog);
            }           

            if (oListaResult.Count == 0)
            {
                oListaResult.Add(new HUECO { CENTRO = Centro,
                    FECHA = DateTime.Parse( fechaInicio).ToString("yyyy-MM-dd")
                });
            }

            return oListaResult;
        }
          

        public List<HUECO> Get(int oidPaciente)
        {

            List<HUECO> oListaResult = new List<HUECO>();

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            //string querySelect = "select * from LISTAONLINE  where ior_paciente=" + oidPaciente + " and username in ('ADPMONLINE','CLIONLINE','MORAONLINE') and  fecha>='TODAY' and estado='0'";
            string querySelect = "select * from LISTAONLINE  where ior_paciente=" + oidPaciente + "  and  fecha>='TODAY' and estado='0'";


            FbCommand oCommand = new FbCommand(querySelect, oConexion);
            try
            {
                
                FbDataReader oReader = oCommand.ExecuteReader();

                while (oReader.Read())
                {
                    oListaResult.Add(new HUECO
                    {

                        IDHUECO = oReader["OID"].ToString(),
                        FIL_EXPLORACION = oReader["EXPLORACION"].ToString(),
                        CODIGOAPARATO = oReader["GRUPO"].ToString(),
                        CENTRO = oReader["CENTRO"].ToString(),
                        FECHA = DateTime.Parse(oReader["FECHA"].ToString()).ToString("dd/MM/yyyy"),
                        HORA = oReader["HORA"].ToString(),
                        TEXTO = ExploracionRepositorio.Obtener((int)oReader["OID"]).APARATO.TEXTO
                    });


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
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    };
                }
            }



            return oListaResult;
        }





        // POST api/citaonline
        public Models.Clases.ExploracionCitaOnline Post(Models.Clases.ExploracionCitaOnline CITA)
        {
            //Obtenemos el tipo de Exploración por el OID que recibimos como parametro del querystrin
            APARATOS oTipoExploracion = Models.Repos.AparatoRepositorio.Obtener(int.Parse(CITA.OIDEXPLORACION));
            //Luego miramos si la prueba esta cubierta por la mutua o no
            string precioExploracion = Models.Repos.TarifasRepositorio.ObtenerPrecioExploracion(oTipoExploracion.OID, CITA.OIDMUTUA);

            //Si el CID es igual a 1 es especial,           
            //o Bien si la mutua no cubre la exploración, es decir, no hay precio para la misma
            //enviar un mail con los datos a citación@dr-manchon.com
            if (oTipoExploracion.CID == 1 || precioExploracion.Length == 0)
            {
                CITA.ID = -1;
            }
            if (precioExploracion.Length == 0)
            {
                CITA.ID = -2;
            }
            CITA.TEXTO = oTipoExploracion.TEXTO;
            return CITA;
        }

       

        // DELETE api/citaonline/5
        public void Delete(int oid)
        {
            EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oid);
            ExploracionRepositorio.CambiarEstado(int.Parse(oExploracion.ESTADO), 1, oExploracion.OID, "",true, "");
        }





    }
}

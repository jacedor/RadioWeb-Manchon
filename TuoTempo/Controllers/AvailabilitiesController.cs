using FirebirdSql.Data.FirebirdClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using TuoTempo.Models;
using Newtonsoft.Json;
using System.Data;

namespace TuoTempo.Controllers
{

    //      a.Documentación técnica: https://postman.tuotempo.com/
    //      b.Notion: https://www.notion.so/tuotempo-enterprise/Guia-de-Integraci-n-TuoTempo-ES-bb77b8c05b5744b7ad4647228d3afe04

    [BasicAuthentication]
    public class AvailabilitiesController : ApiController
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString; // Ajusta esto según tu conexión a Firebird

        private static Logger logger = LogManager.GetCurrentClassLogger();

      
        public class MyResponse
        {
            public string result { get; set; }

            [JsonProperty("return")]
            public object returnObject { get; set; }
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tuotempo/availabilities/{activity_lid}")]
        public IHttpActionResult ObtenerHuecos(int activity_lid, string start_day, string end_day, string location_lids="CUALQUIERA",
            int insurance_lid= 3820159,  string start_time="00:00",
    string end_time = "23:59", string min_time = "00:00", string max_time= "23:59",
    string resource_lids="-1", int results_number=100)
        {

            var request = System.Web.HttpContext.Current.Request;
            var fullUrl = request.Url.AbsoluteUri;

            // Loggear la URL de la solicitud
            logger.Info("Solicitud recibida en la URL: {0}", fullUrl);

            // El resto de la lógica del método
            logger.Info("Solicitud de ObtenerHuecos recibida: activity_lid={0}, start_day={1}, end_day={2}, location_lids={3}, insurance_lid={4}, start_time={5}, end_time={6}, min_time={7}, max_time={8}, resource_lids={9}, results_number={10}",
                activity_lid, start_day, end_day, location_lids, insurance_lid, start_time, end_time, min_time, max_time, resource_lids, results_number);


            List<Availabilities> availabilities = new List<Availabilities>();
            if (resource_lids == "-1")
            {
                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();
                    string resourceQuery = @"SELECT g.OID
                                         FROM aparatos a
                                         JOIN GAPARATOS g ON g.oid = a.OWNER
                                         WHERE a.oid = @activity_lid";

                    using (var resourceCommand = new FbCommand(resourceQuery, connection))
                    {
                        resourceCommand.Parameters.Add("@activity_lid", FbDbType.Integer).Value = activity_lid;

                        var result = resourceCommand.ExecuteScalar();
                        if (result != null)
                        {
                            resource_lids = result.ToString();
                        }
                        else
                        {
                            return BadRequest("No resource found for the given activity_lid.");
                        }
                    }
                }
            }
            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración

            List<string> locationLidsArray;

            if (location_lids == "CUALQUIERA" || location_lids==null)
            {
                locationLidsArray = new List<string>(); // Inicializas la lista vacía
                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();
                    var queryCentros = "SELECT OID FROM CENTROS WHERE TUOTEMPO='T'";

                    using (var command = new FbCommand(queryCentros, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Agregar el OID al arreglo (lista)
                                locationLidsArray.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            else
            {
                // Si no es "CUALQUIERA", se divide el string normalmente
                locationLidsArray = location_lids.Split(',').ToList();
            }

            try
            {
                

       

                string query = @"SELECT p.AVAILABILITY_LID, p.FECHA, p.START_TIME, p.END_TIME, p.LOCATION_LID,p.RESOURCE_LID, p.ACTIVITY_LID, p.INSURANCE_LID, p.CCANTIDAD, p.TEXTODEFECTO, p.HORARIO_APARATO 
                        FROM HUECOSLIBRES_TUOTEMPO (@OID_MUTUA, @FECHAINI, @FECHAFIN, @HORAMIN,@HORAMAX, @OID_GRUPO, @NUMERO_HUECOS, @OID_CENTRO, @OID_TIPOEXPLO) p; ";

                foreach (var locationLid in locationLidsArray)
                {
                    using (var connection = new FbConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = new FbCommand(query, connection))
                        {
                           
                            // Agrega o actualiza los parámetros aquí
                            command.Parameters.Clear(); // Asegúrate de limpiar los parámetros antiguos para evitar duplicados
                            command.Parameters.Add("@OID_MUTUA", FbDbType.Integer).Value = insurance_lid;
                            command.Parameters.Add("@FECHAINI", FbDbType.TimeStamp).Value = DateTime.Parse(start_day);
                            command.Parameters.Add("@FECHAFIN", FbDbType.TimeStamp).Value = DateTime.Parse(end_day);
                            // Agregar el resto de los parámetros
                            command.Parameters.Add("@HORAMIN", FbDbType.VarChar).Value = min_time;
                            command.Parameters.Add("@HORAMAX", FbDbType.VarChar).Value = max_time;
                            command.Parameters.Add("@OID_GRUPO", FbDbType.Integer).Value = int.Parse(resource_lids);
                            command.Parameters.Add("@NUMERO_HUECOS", FbDbType.Integer).Value = results_number;
                            command.Parameters.Add("@OID_CENTRO", FbDbType.Integer).Value = int.Parse(locationLid); // Usar cada valor de locationLid aquí
                            command.Parameters.Add("@OID_TIPOEXPLO", FbDbType.Integer).Value = activity_lid;

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var availability = new Availabilities
                                    {
                                        availability_lid = reader["AVAILABILITY_LID"].ToString(),
                                        date = DateTime.Parse( reader["FECHA"].ToString()).ToShortDateString(),
                                        start_time = reader["START_TIME"].ToString(),
                                        end_time = reader.IsDBNull(reader.GetOrdinal("END_TIME")) ? null : reader["END_TIME"].ToString(),
                                        location_lid = reader.IsDBNull(reader.GetOrdinal("LOCATION_LID")) ? null : reader["LOCATION_LID"].ToString(),
                                        resource_lid = reader.IsDBNull(reader.GetOrdinal("RESOURCE_LID")) ? null : reader["RESOURCE_LID"].ToString(),
                                        activity_lid = reader.IsDBNull(reader.GetOrdinal("ACTIVITY_LID")) ? null : reader["ACTIVITY_LID"].ToString(),
                                        insurance_lid = reader.IsDBNull(reader.GetOrdinal("INSURANCE_LID")) ? null : reader["INSURANCE_LID"].ToString(),
                                        price = reader.IsDBNull(reader.GetOrdinal("CCANTIDAD")) ? null : reader["CCANTIDAD"].ToString()
                                    };
                                    availabilities.Add(availability);
                                  
                                }
                            }
                        }
                    }
                }

                var response = new MyResponse
                {
                    result = "OK",
                    returnObject = availabilities
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms. Detalle: {ex.Message}");
                return InternalServerError();
            }
        }








        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tuotempo/availabilities/{activity_lid}/first")]
        public IHttpActionResult ObtenerPrimerHuecos(int activity_lid, int insurance_lid, string start_day, string end_day, string start_time,
        string end_time, string min_time, string max_time, string resource_lids, string location_lids)
        {
            List<Availabilities> availabilities = new List<Availabilities>();
         
            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
            try
            {
                // Divide el string de location_lids en un arreglo utilizando las comas como delimitador
                var locationLidsArray = location_lids.Split(',');
                string query = @"SELECT p.AVAILABILITY_LID, p.FECHA, p.START_TIME, p.END_TIME, p.LOCATION_LID,p.RESOURCE_LID, p.ACTIVITY_LID, p.INSURANCE_LID, p.CCANTIDAD, p.TEXTODEFECTO, p.HORARIO_APARATO 
                        FROM HUECOSLIBRES_TUOTEMPO (@OID_MUTUA, @FECHAINI, @FECHAFIN, @HORAMIN,@HORAMAX, @OID_GRUPO, @NUMERO_HUECOS, @OID_CENTRO, @OID_TIPOEXPLO) p; ";

                foreach (var locationLid in locationLidsArray)
                {
                    using (var connection = new FbConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = new FbCommand(query, connection))
                        {

                            // Agrega o actualiza los parámetros aquí
                            command.Parameters.Clear(); // Asegúrate de limpiar los parámetros antiguos para evitar duplicados
                            command.Parameters.Add("@OID_MUTUA", FbDbType.Integer).Value = insurance_lid;
                            command.Parameters.Add("@FECHAINI", FbDbType.TimeStamp).Value = DateTime.Parse(start_day);
                            command.Parameters.Add("@FECHAFIN", FbDbType.TimeStamp).Value = DateTime.Parse(end_day);
                            // Agregar el resto de los parámetros
                            command.Parameters.Add("@HORAMIN", FbDbType.VarChar).Value = min_time;
                            command.Parameters.Add("@HORAMAX", FbDbType.VarChar).Value = max_time;
                            command.Parameters.Add("@OID_GRUPO", FbDbType.Integer).Value = int.Parse(resource_lids);
                            command.Parameters.Add("@NUMERO_HUECOS", FbDbType.Integer).Value = 1;
                            command.Parameters.Add("@OID_CENTRO", FbDbType.Integer).Value = int.Parse(locationLidsArray[0]); // Usar cada valor de locationLid aquí
                            command.Parameters.Add("@OID_TIPOEXPLO", FbDbType.Integer).Value = activity_lid;

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var availability = new Availabilities
                                    {
                                        availability_lid = reader["AVAILABILITY_LID"].ToString(),
                                        date = DateTime.Parse(reader["FECHA"].ToString()).ToShortDateString(),
                                        start_time = reader["START_TIME"].ToString(),
                                        end_time = reader.IsDBNull(reader.GetOrdinal("END_TIME")) ? null : reader["END_TIME"].ToString(),
                                        location_lid = reader.IsDBNull(reader.GetOrdinal("LOCATION_LID")) ? null : reader["LOCATION_LID"].ToString(),
                                        resource_lid = reader.IsDBNull(reader.GetOrdinal("RESOURCE_LID")) ? null : reader["RESOURCE_LID"].ToString(),
                                        activity_lid = reader.IsDBNull(reader.GetOrdinal("ACTIVITY_LID")) ? null : reader["ACTIVITY_LID"].ToString(),
                                        insurance_lid = reader.IsDBNull(reader.GetOrdinal("INSURANCE_LID")) ? null : reader["INSURANCE_LID"].ToString(),
                                        price = reader.IsDBNull(reader.GetOrdinal("CCANTIDAD")) ? null : reader["CCANTIDAD"].ToString()
                                    };
                                    availabilities.Add(availability);

                                }
                            }
                        }
                    }
                }
                var retun= availabilities.OrderByDescending(p=>p.date).FirstOrDefault();

                var response = new MyResponse
                {
                    result = "OK",
                    returnObject = retun
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms. Detalle: {ex.Message}");
                return InternalServerError();
            }

        }




    }
}








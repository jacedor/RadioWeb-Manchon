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
        public IHttpActionResult ObtenerHuecos(int activity_lid, int insurance_lid, string start_day, string end_day, string start_time, 
            string end_time, string min_time, string max_time,string resource_lids, int results_number, string location_lids)
        {
            List<Insurance> huecos = new List<Insurance>();

            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
            try
            {

                var availabilities = new List<Availabilities>();

                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new FbCommand("HUECOSLIBRES_TUOTEMPO", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Agrega los parámetros aquí
                        command.Parameters.Add("@OID_MUTUA", FbDbType.Integer).Value = insurance_lid;
                        command.Parameters.Add("@FECHAINI", FbDbType.TimeStamp).Value = start_day;
                        command.Parameters.Add("@FECHAFIN", FbDbType.TimeStamp).Value = end_day;
                        // Agregar el resto de los parámetros
                        command.Parameters.Add("@HORAMIN", FbDbType.VarChar).Value = min_time;
                        command.Parameters.Add("@HORAMAX", FbDbType.VarChar).Value = max_time;
                        command.Parameters.Add("@OID_GRUPO", FbDbType.Integer).Value = resource_lids;
                        command.Parameters.Add("@NUMERO_HUECOS", FbDbType.Integer).Value = results_number;
                        command.Parameters.Add("@OID_CENTRO", FbDbType.Integer).Value = location_lids;
                        command.Parameters.Add("@OID_TIPOEXPLO", FbDbType.Integer).Value = activity_lid;


                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var availability = new Availabilities
                                {
                                    availability_lid = reader["AVAILABILITY_LID"].ToString(),
                                    date = reader["FECHA"].ToString(),
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

                // Crear la respuesta JSON
                var response = new MyResponse
                {
                    result = "OK",
                    returnObject = availabilities
                };

                // Devolver la respuesta
                return Ok(response);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms en /api/insurances - GET. Detalle: {ex.Message}");
                return InternalServerError();
               
            }
         
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tuotempo/availabilities/{activity_lid}/first")]
        public IHttpActionResult ObtenerPrimerHuecos(int activity_lid, int insurance_lid, string start_day, string end_day, string start_time,
        string end_time, string min_time, string max_time, string resource_lids, string location_lids)
        {
            List<Insurance> huecos = new List<Insurance>();

            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
            try
            {

                var availabilities = new List<Availabilities>();

                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new FbCommand("HUECOSLIBRES_TUOTEMPO", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Agrega los parámetros aquí
                        command.Parameters.Add("@OID_MUTUA", FbDbType.Integer).Value = insurance_lid;
                        command.Parameters.Add("@FECHAINI", FbDbType.TimeStamp).Value = start_day;
                        command.Parameters.Add("@FECHAFIN", FbDbType.TimeStamp).Value = end_day;
                        // Agregar el resto de los parámetros
                        command.Parameters.Add("@HORAMIN", FbDbType.VarChar).Value = min_time;
                        command.Parameters.Add("@HORAMAX", FbDbType.VarChar).Value = max_time;
                        command.Parameters.Add("@OID_GRUPO", FbDbType.Integer).Value = resource_lids;
                        command.Parameters.Add("@NUMERO_HUECOS", FbDbType.Integer).Value = 1;
                        command.Parameters.Add("@OID_CENTRO", FbDbType.Integer).Value = location_lids;
                        command.Parameters.Add("@OID_TIPOEXPLO", FbDbType.Integer).Value = activity_lid;


                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var availability = new Availabilities
                                {
                                    availability_lid = reader["AVAILABILITY_LID"].ToString(),
                                    date = reader["FECHA"].ToString(),
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

                // Crear la respuesta JSON
                var response = new MyResponse
                {
                    result = "OK",
                    returnObject = availabilities.First()
                };

                // Devolver la respuesta
                return Ok(response);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms en /api/insurances - GET. Detalle: {ex.Message}");
                return InternalServerError();

            }

        }




    }
}








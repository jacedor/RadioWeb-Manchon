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


namespace TuoTempo.Controllers
{

//      a.Documentación técnica: https://postman.tuotempo.com/
//      b.Notion: https://www.notion.so/tuotempo-enterprise/Guia-de-Integraci-n-TuoTempo-ES-bb77b8c05b5744b7ad4647228d3afe04

    [BasicAuthentication]
    public class LocationController : ApiController
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString; // Ajusta esto según tu conexión a Firebird

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Resource MapToResource(FbDataReader reader)
        {
            // Definir la consulta SQL con un parámetro

            string query = @"
             SELECT M.OID, M.NOMBRE
             FROM MUTUAS M 
            WHERE  M.TUOTEMPO = 'T'";

            List<string> oListaMutuaCobertura = new List<string>();

            using (var connection = new FbConnection(connectionString))
            {
                connection.Open();
                using (var command = new FbCommand(query, connection))
                {


                    using (var fbReader = command.ExecuteReader())
                    {
                        while (fbReader.Read())
                        {
                            oListaMutuaCobertura.Add(fbReader["OID"].ToString());
                        }
                    }
                }
            }
            return new Resource
            {
                resource_lid = reader["OID"]?.ToString() ?? string.Empty,
                name = reader["DES_GRUP"]?.ToString() ?? string.Empty,
                id_number = new id_number
                {
                    number = "111111111A",
                    type = 1
                },
                related = new related
                {
                    insurance_lids = oListaMutuaCobertura
                },

                web_enabled = true,
                active = true
            };
        }


        private Location MapToLocation(FbDataReader reader)
        {
            return new Location
            {
                location_lid = reader["OID"]?.ToString() ?? string.Empty,
                name = reader["NOMBRE"]?.ToString() ?? string.Empty,
                address = new address
                {
                    street = reader["DIRECCION"]?.ToString() ?? string.Empty,
                    street_number = reader["CP"]?.ToString() ?? string.Empty,
                    city = reader["CIUTAT"]?.ToString() ?? string.Empty,
                    province = "BARCELONA",
                    country = "ESPAÑA"
                },
                contact = new Contact
                {
                    mobile = reader["TELEFONO"]?.ToString() ?? string.Empty
                },
                web_enabled = true,
                active = true
            };
        }

        public class MyResponse
        {
            public string result { get; set; }

            [JsonProperty("return")]
            public object returnObject { get; set; }
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tuotempo/locations")]
        public IHttpActionResult GetLocations()
        {
            List<Location> locations = new List<Location>();

            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
            try
            {
                
                // Suponiendo que tienes una forma de obtener la IP y el ID del usuario
                var clientIp = HttpContext.Current?.Request?.UserHostAddress;
                var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo";

                logger.Info($"Inicio de solicitud: {startTime}. IP del cliente: {clientIp}, Usuario: {userId}, Endpoint: /api/locations - GET");

                // Tu lógica aquí...

                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM CENTROS WHERE TUOTEMPO='T'";


                    using (var command = new FbCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Llamada al método MapToLocation
                                Location location = MapToLocation(reader);
                                locations.Add(location);
                            }
                        }
                    }
                }

                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. Ubicaciones obtenidas: {locations.Count}.");



                // Crear la respuesta JSON
                var response = new MyResponse
                {
                    result = "OK",                    
                    returnObject = locations
                };

                // Devolver la respuesta
                return Ok(response);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms en /api/locations - GET. Detalle: {ex.Message}");
                return InternalServerError();
               
            }
         
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tuotempo/locations/{id}")]
        public IHttpActionResult GetLocation(int id)
        {
            Location location = null;
            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
                                             // Suponiendo que tienes una forma de obtener la IP y el ID del usuario
            var clientIp = HttpContext.Current?.Request?.UserHostAddress;
            var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo";
            try
            {
                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new FbCommand("SELECT * FROM CENTROS WHERE TUOTEMPO='T' AND OID=@id", connection))
                    {
                        // Use parameterized queries to prevent SQL injection
                        command.Parameters.AddWithValue("@id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read()) // Assumes ID is unique and only one record is returned
                            {
                                // Llamada al método MapToLocation
                                location = MapToLocation(reader);
                            }
                        }
                    }
                }

                if (location != null)
                {
                    var duration = DateTime.UtcNow - startTime; // Calcular duración
                    logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. Ubicacion obtenida: {location.name}.");


                    // Crear la respuesta JSON
                    var response = new MyResponse
                    {
                        result = "OK",
                        returnObject = location
                    };

                    // Devolver la respuesta
                    return Ok(response);
                    
                }
                else
                {
                    var duration = DateTime.UtcNow - startTime; // Calcular duración
                    logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. Ubicaciones no encontrada: {id}.");
                    return NotFound(); // Return an HTTP 404 if no location is found
                }
            }
            catch (Exception ex)
            {

                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms en /api/locations/id - GET. Detalle: {ex.Message}");
                return InternalServerError();
             
            }
        }

        //[System.Web.Http.HttpGet]
        //[System.Web.Http.Route("tuotempo/locations/{id}/resources")]
        //public IHttpActionResult GetLocationPorCentro(int id)
        //{
        //    Location location = null;
        //    var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
        //                                     // Suponiendo que tienes una forma de obtener la IP y el ID del usuario
        //    var clientIp = HttpContext.Current?.Request?.UserHostAddress;
        //    var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo";
        //    try
        //    {
        //        using (var connection = new FbConnection(connectionString))
        //        {
        //            connection.Open();

        //           string query= "select DISTINCT(D.OWNER),g.COD_GRUP,g.* " +
        //                "from DAPARATOS D JOIN GAPARATOS G ON G.OID=D.OWNER " +
        //                "WHERE G.TUOTEMPO='T' AND D.CID=@id ";

        //            using (var command = new FbCommand(query, connection))
        //            {
        //                // Use parameterized queries to prevent SQL injection
        //                command.Parameters.AddWithValue("@id", id);

        //                using (var reader = command.ExecuteReader())
        //                {
        //                    if (reader.Read()) // Assumes ID is unique and only one record is returned
        //                    {
        //                        // Llamada al método MapToLocation
        //                        location = MapToLocation(reader);
        //                    }
        //                }
        //            }
        //        }

        //        if (location != null)
        //        {
        //            var duration = DateTime.UtcNow - startTime; // Calcular duración
        //            logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. Ubicacion obtenida: {location.name}.");


        //            // Crear la respuesta JSON
        //            var response = new MyResponse
        //            {
        //                result = "OK",
        //                returnObject = location
        //            };

        //            // Devolver la respuesta
        //            return Ok(response);

        //        }
        //        else
        //        {
        //            var duration = DateTime.UtcNow - startTime; // Calcular duración
        //            logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. Ubicaciones no encontrada: {id}.");
        //            return NotFound(); // Return an HTTP 404 if no location is found
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        var duration = DateTime.UtcNow - startTime; // Calcular duración
        //        logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms en /api/locations/id - GET. Detalle: {ex.Message}");
        //        return InternalServerError();

        //    }
        //}
    }
}








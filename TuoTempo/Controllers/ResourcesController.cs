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

//GRUPOS DE APARATOS
//      a.Documentación técnica: https://postman.tuotempo.com/
//      b.Notion: https://www.notion.so/tuotempo-enterprise/Guia-de-Integraci-n-TuoTempo-ES-bb77b8c05b5744b7ad4647228d3afe04

    [BasicAuthentication]
    public class ResourceController : ApiController
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString; // Ajusta esto según tu conexión a Firebird

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Activity MapToActivity(FbDataReader reader)
        {
            // Definir la consulta SQL con un parámetro
            string query = @"
             SELECT M.OID, M.NOMBRE, P.CANTIDAD
             FROM PRECIOS P
             JOIN MUTUAS M ON M.OID = P.IOR_ENTIDADPAGADORA             
            WHERE P.IOR_TIPOEXPLORACION = @TipoExploracion AND M.TUOTEMPO = 'T'";

            List<string> oListaMutuaCobertura = new List<string>();

            using (var connection = new FbConnection(connectionString))
            {
                connection.Open();
                using (var command = new FbCommand(query, connection))
                {
                    // Asignar el valor del parámetro
                    command.Parameters.AddWithValue("@TipoExploracion", reader["OID"]?.ToString());

                    using (var fbReader = command.ExecuteReader())
                    {
                        while (fbReader.Read())
                        {
                            oListaMutuaCobertura.Add(fbReader["OID"].ToString());
                        }
                    }
                }
            }

            return new Activity
            {
                activity_lid = reader["OID"]?.ToString() ?? string.Empty,
                name = reader["TEXTO_INTERNET"]?.ToString() ?? string.Empty,
                preparation = reader["TEXTO"]?.ToString() ?? string.Empty,
                group = new group
                {
                    group_lid = reader["OWNER"]?.ToString() ?? string.Empty,
                    name = reader["DES_GRUP"]?.ToString() ?? string.Empty,
                },
                type = "presential",
                related = new related
                {
                    insurance_lids = oListaMutuaCobertura
                },
                duration = "15",
                web_enabled = true,
                active = true
            };
        }

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
                first_name = reader["DES_GRUP"]?.ToString() ?? string.Empty,
                id_number= new id_number
                {
                    number="111111111A",
                    type= 1
                },                          
                related =  new related
                {
                    insurance_lids = oListaMutuaCobertura
                },
                            
                web_enabled = true,
                active = true
            };
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tuotempo/resources")]
        public IHttpActionResult GetResources()
        {
            List<Resource> resources = new List<Resource>();

            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
            try
            {
                
                // Suponiendo que tienes una forma de obtener la IP y el ID del usuario
                var clientIp = HttpContext.Current?.Request?.UserHostAddress;
                var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo";

                logger.Info($"Inicio de solicitud: {startTime}. IP del cliente: {clientIp}, Usuario: {userId}, Endpoint: /api/resource - GET");

                // Tu lógica aquí...

                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM GAPARATOS  WHERE TUOTEMPO='T'";


                    using (var command = new FbCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Llamada al método MapToLocation
                                Resource resource = MapToResource(reader);
                                resources.Add(resource);
                            }
                        }
                    }
                }

                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. Resources obtenidas: {resources.Count}.");



                // Crear la respuesta JSON
                var response = new MyResponse
                {
                    result = "OK",                    
                    returnObject = resources
                };

                // Devolver la respuesta
                return Ok(response);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms en /api/resources - GET. Detalle: {ex.Message}");
                return InternalServerError();
               
            }
         
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tuotempo/locations/{id}/resources")]
        public IHttpActionResult GetResourcesByLocation(int id)
        {
            List<Resource> resources = new List<Resource>();

            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
            try
            {

                // Suponiendo que tienes una forma de obtener la IP y el ID del usuario
                var clientIp = HttpContext.Current?.Request?.UserHostAddress;
                var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo";

                logger.Info($"Inicio de solicitud: {startTime}. IP del cliente: {clientIp}, Usuario: {userId}, Endpoint: /api/resource - GET");

                // Tu lógica aquí...

                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM GAPARATOS  WHERE TUOTEMPO='T'";


                    using (var command = new FbCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Llamada al método MapToLocation
                                Resource resource = MapToResource(reader);
                                resources.Add(resource);
                            }
                        }
                    }
                }

                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. Resources por centro obtenidas: {resources.Count}.");



                // Crear la respuesta JSON
                var response = new MyResponse
                {
                    result = "OK",
                    returnObject = resources
                };

                // Devolver la respuesta
                return Ok(response);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms en /api/resources - GET. Detalle: {ex.Message}");
                return InternalServerError();

            }

        }
        
        
        //[System.Web.Http.HttpGet]
        //[System.Web.Http.Route("api/locations/{id}/resources")]
        //public IHttpActionResult GetActivitiesByResource(int id)
        //{
        //    List<Activity> resources = new List<Activity>();

        //    var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
        //    try
        //    {

        //        // Suponiendo que tienes una forma de obtener la IP y el ID del usuario
        //        var clientIp = HttpContext.Current?.Request?.UserHostAddress;
        //        var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo";

        //        logger.Info($"Inicio de solicitud: {startTime}. IP del cliente: {clientIp}, Usuario: {userId}, Endpoint: /api/GetActivitiesByResource - GET");

        //        // Tu lógica aquí...

        //        using (var connection = new FbConnection(connectionString))
        //        {
        //            connection.Open();

        //            var query = "SELECT * FROM APARATOS R JOIN GAPARATOS G ON G.OID=R.OWNER WHERE R.OWNER=@OID AND SALIDA_INTERNET='1'";


        //            using (var command = new FbCommand(query, connection))
        //            {
        //                // Use parameterized queries to prevent SQL injection
        //                command.Parameters.AddWithValue("@OID", id);

        //                using (var reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        // Llamada al método MapToLocation
        //                        Activity resource = MapToActivity(reader);
        //                        resources.Add(resource);
        //                    }
        //                }
        //            }
        //        }

        //        var duration = DateTime.UtcNow - startTime; // Calcular duración
        //        logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. Resources por centro obtenidas: {resources.Count}.");



        //        // Crear la respuesta JSON
        //        var response = new MyResponse
        //        {
        //            result = "OK",
        //            returnObject = resources
        //        };

        //        // Devolver la respuesta
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        var duration = DateTime.UtcNow - startTime; // Calcular duración
        //        logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms en /locations/:location_lid/resources - GET. Detalle: {ex.Message}");
        //        return InternalServerError();

        //    }

        //}

    }
}








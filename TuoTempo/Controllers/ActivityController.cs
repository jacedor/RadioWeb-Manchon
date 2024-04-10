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
using System.Security.Cryptography;


namespace TuoTempo.Controllers
{

//TIPOS DE EXPLORACIÓN
//      a.Documentación técnica: https://postman.tuotempo.com/
//      b.Notion: https://www.notion.so/tuotempo-enterprise/Guia-de-Integraci-n-TuoTempo-ES-bb77b8c05b5744b7ad4647228d3afe04

    [BasicAuthentication]
    public class ActivityController : ApiController
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString; // Ajusta esto según tu conexión a Firebird

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Activity MapToActivity(FbDataReader reader)
        {

            //var query = "SELECT * FROM APARATOS R JOIN GAPARATOS G ON G.OID=R.OWNER WHERE SALIDA_INTERNET='1'";
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

            //var query = "SELECT * FROM APARATOS R JOIN GAPARATOS G ON G.OID=R.OWNER WHERE SALIDA_INTERNET='1'";
            // Definir la consulta SQL con un parámetro
            string query2 = @"
            SELECT OID
                FROM CENTROS WHERE CID=1 AND OID IN (

                        select D.CID
                        from APARATOS A 
                        JOIN GAPARATOS G ON G.OID=A.OWNER 
                        JOIN DAPARATOS D ON D.OWNER= A.OWNER
                        WHERE G.IOR_EMPRESA=4 AND A.SALIDA_INTERNET='1'
                        AND G.TUOTEMPO='T' 
                        AND A.OID=@TipoExploracion)";

            List<string> oListaCentros = new List<string>();

            using (var connection = new FbConnection(connectionString))
            {
                connection.Open();
                using (var command = new FbCommand(query2, connection))
                {
                    // Asignar el valor del parámetro
                    command.Parameters.AddWithValue("@TipoExploracion", reader["OID"]?.ToString());

                    using (var fbReader = command.ExecuteReader())
                    {
                        while (fbReader.Read())
                        {
                            oListaCentros.Add(fbReader["OID"].ToString());
                        }
                    }
                }
            }


            return new Activity
            {
                activity_lid = reader["OID"]?.ToString() ?? string.Empty,
                name = reader["TEXTO_INTERNET"]?.ToString() ?? string.Empty,
                preparation= reader["TEXTO"]?.ToString() ?? string.Empty,
                group = new group
                {
                    group_lid = reader["OWNER"]?.ToString() ?? string.Empty,
                    name = reader["DES_GRUP"]?.ToString() ?? string.Empty,
                },   
                type="presential",                
                related =  new related
                {
                    insurance_lids= oListaMutuaCobertura,
                    resource_lids= oListaCentros
                },
                duration ="15",                
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
        [System.Web.Http.Route("tuotempo/activities")]
        public IHttpActionResult GetActivities()
        {
            List<Activity> activities = new List<Activity>();

            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
            try
            {
                
                // Suponiendo que tienes una forma de obtener la IP y el ID del usuario
                var clientIp = HttpContext.Current?.Request?.UserHostAddress;
                var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo";

                logger.Info($"Inicio de solicitud: {startTime}. IP del cliente: {clientIp}, Usuario: {userId}, Endpoint: /api/activities - GET");

                // Tu lógica aquí...

                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT R.*,G.DES_GRUP,G.OWNER FROM APARATOS R JOIN GAPARATOS G ON G.OID=R.OWNER WHERE SALIDA_INTERNET='1'";


                    using (var command = new FbCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Llamada al método MapToActivity
                                Activity TipoDeExploracion = MapToActivity(reader);
                                if (TipoDeExploracion.related.insurance_lids.Count>0)
                                {
                                    activities.Add(TipoDeExploracion);
                                }
                                
                            }
                        }
                    }
                }

                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. Activities obtenidas: {activities.Count}.");



                // Crear la respuesta JSON
                var response = new MyResponse
                {
                    result = "OK",                    
                    returnObject = activities
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
        [System.Web.Http.Route("tuotempo/insurances/{id}/activities")]
        public IHttpActionResult GetActivitiesByInsurance(int id)
        {
            List<Activity> activities = new List<Activity>();

            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
            try
            {

                // Suponiendo que tienes una forma de obtener la IP y el ID del usuario
                var clientIp = HttpContext.Current?.Request?.UserHostAddress;
                var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo";

                logger.Info($"Inicio de solicitud: {startTime}. IP del cliente: {clientIp}, Usuario: {userId}, Endpoint: /api/activities - GET");

                // Tu lógica aquí...

                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT APA.*,G.DES_GRUP,G.OWNER FROM PRECIOS P";
                    query += " JOIN MUTUAS M ON M.OID = P.IOR_ENTIDADPAGADORA";
                    query += " JOIN APARATOS APA ON APA.OID = P.IOR_TIPOEXPLORACION";
                    query += "  JOIN GAPARATOS G ON G.OID = APA.OWNER";
                    query += " WHERE SALIDA_INTERNET='1' and P.IOR_ENTIDADPAGADORA = @OID;";


                    using (var command = new FbCommand(query, connection))
                    {
                        // Use parameterized queries to prevent SQL injection
                        command.Parameters.AddWithValue("@OID", id);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Llamada al método MapToActivity
                                Activity TipoDeExploracion = MapToActivity(reader);
                              
                                    activities.Add(TipoDeExploracion);
                                

                            }
                        }
                    }

                    foreach (var item in activities)
                    {
                        string query2 = @"
                      SELECT OID
                        FROM CENTROS WHERE CID=1 AND OID IN (
                            select D.CID
                            from APARATOS A 
                            JOIN GAPARATOS G ON G.OID=A.OWNER 
                            JOIN DAPARATOS D ON D.OWNER= A.OWNER
                            WHERE G.IOR_EMPRESA=4 AND A.SALIDA_INTERNET='1'
                            AND G.TUOTEMPO='T' 
                            AND A.OID=@TipoExploracion)";

                        List<string> oListaCentros = new List<string>();

                        using (var connection2 = new FbConnection(connectionString))
                        {
                            connection2.Open();
                            using (var command = new FbCommand(query2, connection))
                            {
                                // Asignar el valor del parámetro
                                command.Parameters.AddWithValue("@TipoExploracion", item.activity_lid);

                                using (var fbReader = command.ExecuteReader())
                                {
                                    while (fbReader.Read())
                                    {
                                        oListaCentros.Add(fbReader["OID"].ToString());
                                    }
                                }
                                item.related.resource_lids = oListaCentros;
                            }
                        }

                    }
                }

                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. Activities obtenidas: {activities.Count}.");



                // Crear la respuesta JSON
                var response = new MyResponse
                {
                    result = "OK",
                    returnObject = activities
                };

                // Devolver la respuesta
                return Ok(response);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms en /api/activitiesByInsurance - GET. Detalle: {ex.Message}");
                return InternalServerError();

            }

        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tuotempo/resources/{id}/activities")]
        public IHttpActionResult GetActivitiesByResource(int id)
        {
            List<Activity> resources = new List<Activity>();

            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
            try
            {

                // Suponiendo que tienes una forma de obtener la IP y el ID del usuario
                var clientIp = HttpContext.Current?.Request?.UserHostAddress;
                var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo";

                logger.Info($"Inicio de solicitud: {startTime}. IP del cliente: {clientIp}, Usuario: {userId}, Endpoint: /api/GetActivitiesByResource - GET");

                // Tu lógica aquí...

                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();

                    var query = "SELECT R.*,G.DES_GRUP,G.OWNER FROM APARATOS R JOIN GAPARATOS G ON G.OID=R.OWNER WHERE R.OWNER=@OID AND SALIDA_INTERNET='1'";


                    using (var command = new FbCommand(query, connection))
                    {
                        // Use parameterized queries to prevent SQL injection
                        command.Parameters.AddWithValue("@OID", id);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Llamada al método MapToLocation
                                Activity resource = MapToActivity(reader);
                                resources.Add(resource);
                            }
                        }
                    }
                }

                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. GetActivitiesByResource por centro obtenidas: {resources.Count}.");



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
                logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms en GetActivitiesByResource - GET. Detalle: {ex.Message}");
                return InternalServerError();

            }

        }


    }
}








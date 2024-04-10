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
    public class InsurancesController : ApiController
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString; // Ajusta esto según tu conexión a Firebird

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Insurance MapToInsurance(FbDataReader reader)
        {
            return new Insurance
            {
                insurance_lid = reader["OID"]?.ToString() ?? string.Empty,
                name = reader["NOMBRE"]?.ToString() ?? string.Empty,
                patient_notice = new PatientNotice
                {
                    text =string.Empty,
                    show = false                  
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
        [System.Web.Http.Route("tuotempo/insurances")]
        public IHttpActionResult GetInsurances()
        {
            List<Insurance> insurances = new List<Insurance>();

            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
            try
            {
                
                // Suponiendo que tienes una forma de obtener la IP y el ID del usuario
                var clientIp = HttpContext.Current?.Request?.UserHostAddress;
                var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo";

                logger.Info($"Inicio de solicitud: {startTime}. IP del cliente: {clientIp}, Usuario: {userId}, Endpoint: /api/insurances - GET");

                // Tu lógica aquí...

                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM MUTUAS WHERE TUOTEMPO='T'";


                    using (var command = new FbCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Llamada al método MapToLocation
                                Insurance insurance = MapToInsurance(reader);
                                insurances.Add(insurance);
                            }
                        }
                    }
                }

                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. Insurances obtenidas: {insurances.Count}.");



                // Crear la respuesta JSON
                var response = new MyResponse
                {
                    result = "OK",                    
                    returnObject = insurances
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
        [System.Web.Http.Route("tuotempo/insurances/{id}/resources")]
        public IHttpActionResult GetResourcesByInsurances(int id)
        {
            List<Insurance> insurances = new List<Insurance>();

            var startTime = DateTime.UtcNow; // Tiempo de inicio para calcular la duración
            try
            {

                // Suponiendo que tienes una forma de obtener la IP y el ID del usuario
                var clientIp = HttpContext.Current?.Request?.UserHostAddress;
                var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo";

                logger.Info($"Inicio de solicitud: {startTime}. IP del cliente: {clientIp}, Usuario: {userId}, Endpoint: /tuotempo/insurances/id/resources - GET");

                // Tu lógica aquí...
                List<string> resource_ids = new List<string>();
                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();
                    var query = @"select DISTINCT(D.OWNER)  
                        from PRECIOS P 
                        JOIN GAPARATOS G ON G.OID=P.IOR_GAPARATO 
                        JOIN DAPARATOS D ON D.OWNER=G.OID 
                        WHERE  G.TUOTEMPO='T' AND P.IOR_ENTIDADPAGADORA=@id AND P.CANTIDAD>0";

                  
                    using (var command = new FbCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Llamada al método MapToLocation                               
                                resource_ids.Add((reader["OWNER"].ToString()));
                            }
                        }
                    }
                }

                var duration = DateTime.UtcNow - startTime; // Calcular duración
                logger.Info($"Solicitud completada en {duration.TotalMilliseconds} ms. Resources obtenidos: {insurances.Count}.");



                // Crear la respuesta JSON
                var response = new MyResponse
                {
                    result = "OK",
                    returnObject = resource_ids
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








using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Mvc;

namespace RadioWeb.ADPM
{
    public class dtoCalero
    {
        public string NOMBRESUCURSAL  { get; set; }
        public string FECHAEMISIONFACT { get; set; }
        public string NUMEROFACTURA { get; set; }
        public string  FECHAPLANIFICACION { get; set; }
        public string  FECHAREALIZACION { get; set; }
        public string CODIGOVISUALIZACION { get; set; }
        public string CODIGOTIPOIDENTIFICACION { get; set; }
        public string NUMEROIDENTIFICACION { get; set; }
        public string NUMEROHISTCLINICA { get; set; }
        public string APELLIDOSCOMPLETOPACIENTE { get; set; }
        public string  CODIGOPRESTACION { get; set; }
        public string CPT { get; set; }
        public string PRESTACION { get; set; }
        public string PROCEDIMIENTO { get; set; }
        public string APELLIDOSCOMPLETOMEDICO { get; set; }
        public string TARIFASERVICIOREALIZADO { get; set; }
       

    }
    public class CaleroController : ApiController
    {
        private RadioDBContext db = new RadioDBContext();
        // GET: api/Calero
        public List<dtoCalero> Get(string fechaInicio,string fechaFin,int centro,int estado)
        {
           
            string camposQueryStandard = "select * from CALERO_ACTIVIDAD_general('" + DateTime.Parse(fechaInicio).ToString("MM/dd/yyyy") + "','"+ DateTime.Parse(fechaFin).ToString("MM/dd/yyyy") + "'," + centro +"," + estado +") order by fechaplanificacion desc";

            IEnumerable<dtoCalero> oResult;
            oResult = db.Database.SqlQuery<dtoCalero>(camposQueryStandard).ToList<dtoCalero>();

            foreach (var item in oResult)
            {
                try
                {
                    item.TARIFASERVICIOREALIZADO = String.Format("{0:C}", item.TARIFASERVICIOREALIZADO.Replace(",", "."));

                }
                catch (Exception)
                {

                    
                }
            }
            return oResult.ToList();
        }

        // GET: api/Calero/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Calero
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Calero/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Calero/5
        public void Delete(int id)
        {
        }
    }
}

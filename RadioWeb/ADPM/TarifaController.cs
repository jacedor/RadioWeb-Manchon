using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RadioWeb.ADPM
{
    public class TarifaController : ApiController
    {
        // GET api/tarifa
        public string Get(int Mutua, int Aparato, string Exploracion, string OWNER)
        {
          Models.APARATOS oTipoExploracion=  Models.Repos.AparatoRepositorio.Obtener(Exploracion, OWNER);

         return   Models.Repos.TarifasRepositorio.ObtenerPrecioExploracion(oTipoExploracion.OID, Mutua);
            
        }

        // GET api/tarifa/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/tarifa
        public void Post([FromBody]string value)
        {
        }

        // PUT api/tarifa/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/tarifa/5
        public void Delete(int id)
        {
        }
    }
}

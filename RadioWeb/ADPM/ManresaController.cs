using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RadioWeb.ADPM
{
    public class ManresaController : ApiController
    {
        

        // POST api/manresa
        public void Post([FromBody]string value)
        {
        }

        // PUT api/manresa/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/manresa/5
        public HttpResponseMessage Delete(int id)
        {
            var response = new HttpResponseMessage();
            response = Request.CreateResponse(
                           HttpStatusCode.OK, "Exploracion Borrada.");
            // ExploracionRepositorio.DeleteDesdeApi( id);
            return response;
        }
    }
}

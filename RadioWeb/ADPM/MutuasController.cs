using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RadioWeb.Models.Repos;
using RadioWeb.Models;

namespace RadioWeb.ADPM
{
    public class MutuasController : ApiController
    {

        
        // GET api/mutuas
        public List<VMMutuasCitaOnline> Get(int oidAparato)
        {
            return Models.Repos.MutuasRepositorio.CitaOnlineList(oidAparato);
        }

     

        // POST api/mutuas
        public void Post([FromBody]string value)
        {
        }

        // PUT api/mutuas/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/mutuas/5
        public void Delete(int id)
        {
        }
    }
}

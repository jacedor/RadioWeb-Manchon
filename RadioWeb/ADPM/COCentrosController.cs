using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RadioWeb.ADPM
{
    public class COCentrosController : ApiController
    {
        // GET api/cocentros
        public List<CENTROS> Get(string Grupo)
        {
            return CentrosRepositorio.ObtenerPorGrupoAparato(Grupo);
        }

        // GET api/cocentros/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/cocentros
        public void Post([FromBody]string value)
        {
        }

        // PUT api/cocentros/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/cocentros/5
        public void Delete(int id)
        {
        }
    }
}

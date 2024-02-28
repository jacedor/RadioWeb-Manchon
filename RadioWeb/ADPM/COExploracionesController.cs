using RadioWeb.Models.Repos;
using RadioWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RadioWeb.ADPM
{
    public class COExploracionesController : ApiController
    {
      

        // GET api/coexploraciones
        public List<APARATOS> Get(string Grupo, bool Claustro=false)
        {
           // bool claustro = (Claustro == "T" ? true : false);
            return AparatoRepositorio.ObtenerParaInternet(Grupo, Claustro);
        }

        // GET api/coexploraciones
        public string Get(int id)
        {
            return AparatoRepositorio.ObtenerTextoAparato(id);
        }


        

        // POST api/coexploraciones
        public void Post([FromBody]string value)
        {
        }

        // PUT api/coexploraciones/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/coexploraciones/5
        public void Delete(int id)
        {
        }
    }
}

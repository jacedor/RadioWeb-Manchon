using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RadioWeb.ADPM
{
    public class CentroExternoController : ApiController
    {
        // GET api/centroexterno
        public string Get(int oid)
        {
            RadioWeb.Models.CENTROSEXTERNOS oCentroExterno = Models.Repos.CentrosExternosRepositorio.ObtenerParaInternet(oid);
          return oCentroExterno.CODMUT +  " - " +   oCentroExterno.NOMBRE;
        }

       
    }
}

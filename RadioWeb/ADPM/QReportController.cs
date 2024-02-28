using RadioWeb.DTO;
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
    public class QReportController : ApiController
    {
        public HttpResponseMessage Get(int id, string clave, string centro = "")
        {
            try
            {
                if (clave != "B66285511")
                {
                    HttpResponseMessage responseUnautorhize = Request.CreateResponse(HttpStatusCode.Unauthorized, "No autorizado");
                    return responseUnautorhize;
                }
                PACIENTE oPaciente = null;
                if (!String.IsNullOrEmpty(centro))
                {
                    if (centro.ToUpper() == "DELFOS")
                    {
                        oPaciente = PacienteRepositorio.ObtenerPorHC(id);
                    }
                }
                else
                {
                    oPaciente = PacienteRepositorio.Obtener(id);
                }

                if (oPaciente.OID <= 0)
                {
                    HttpResponseMessage responseBad = Request.CreateResponse(HttpStatusCode.NotFound, "No encontrado");
                    return responseBad;
                }
                PacienteQreport oPacienteResult = new PacienteQreport
                {
                    oid = oPaciente.OID,
                    nombre = oPaciente.PACIENTE1,
                    email = oPaciente.EMAIL,
                    dni = oPaciente.DNI,
                    qrcompartircaso = oPaciente.COMPARTIR,
                    codmut = oPaciente.CODMUTUA,
                    descmut = oPaciente.DESCMUTUA


                };
                foreach (var item in oPaciente.TELEFONOS)
                {
                    if (item.NUMERO.StartsWith("6"))
                    {
                        oPacienteResult.telefono = item.NUMERO;
                    }
                }

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, oPacienteResult);
                return response;
            }
            catch (Exception)
            {

                HttpResponseMessage responseError = Request.CreateResponse(HttpStatusCode.InternalServerError, "No encontrado");
                return responseError;
            }

        }

    }
}

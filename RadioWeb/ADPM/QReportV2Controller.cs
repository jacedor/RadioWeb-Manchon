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
    public class QReportV2Controller : ApiController
    {
        public HttpResponseMessage Get(int id,string clave, int accessionNumber = -1, string centro="")
        {
            try
            {
                if (clave!= "B66285511")
                {
                    HttpResponseMessage responseUnautorhize = Request.CreateResponse(HttpStatusCode.Unauthorized, "No autorizado");
                    return responseUnautorhize;
                }

                PACIENTE oPaciente = null;

                string iorColegiado = null;
                string nombreReferidor = null;
                string iorTipoExploracion = null;
                string descTipoExploracion = null;
                string iorEspecialidad = null;
                string descEspecialidad = null;

                if (!String.IsNullOrEmpty(centro))
                {
                    if (centro.ToUpper() == "DELFOS")
                    {
                        oPaciente = PacienteRepositorio.ObtenerPorHC(id);
                    }
                }
                else {
                    oPaciente = PacienteRepositorio.Obtener(id);
                }
               
                if (oPaciente.OID <= 0)
                {
                    HttpResponseMessage responseBad = Request.CreateResponse(HttpStatusCode.NotFound, "No encontrado");
                    return responseBad;
                }

                String codMutua = oPaciente.CODMUTUA;
                String descMutua = oPaciente.DESCMUTUA;

                //Si esta informado el accession number Buscamos exploracion por Accesion number
                if (accessionNumber > 0)
                {

                    EXPLORACION oExploracion = ExploracionRepositorio.Obtener(accessionNumber);
                    if (oExploracion.OID == -1)
                    {
                        HttpResponseMessage responseBad = Request.CreateResponse(HttpStatusCode.NotFound, "No encontrado");
                        return responseBad;
                    }

                    //SI el campo IOR_CENTROEXTERNO está informado
                    if (!string.IsNullOrEmpty(oExploracion.IOR_CENTROEXTERNO.ToString()) && oExploracion.IOR_CENTROEXTERNO != -1)
                    {
                        CENTROSEXTERNOS oCentroExterno = CentrosExternosRepositorio.ObtenerParaInternet((int)oExploracion.IOR_CENTROEXTERNO);
                        codMutua = oCentroExterno.CODMUT;
                        descMutua = oCentroExterno.NOMBRE;
                    }

                    iorColegiado = oExploracion.IOR_COLEGIADO.ToString();
                    if (!string.IsNullOrEmpty(iorColegiado))
                    {
                        nombreReferidor = ColegiadoRepositorio.Obtener(oExploracion.IOR_COLEGIADO ?? -1).NOMBRE;
                    }

                    iorTipoExploracion = oExploracion.IOR_TIPOEXPLORACION.ToString();
                    if (!string.IsNullOrEmpty(iorTipoExploracion))
                    {
                        descTipoExploracion = AparatoRepositorio.Obtener(oExploracion.IOR_TIPOEXPLORACION ?? -1).DES_FIL;
                    }
                 
                    iorEspecialidad = ColegiadoRepositorio.Obtener(oExploracion.IOR_COLEGIADO ?? -1).IOR_ESPECIALIDAD.ToString();
                    if (!string.IsNullOrEmpty(iorEspecialidad))
                    {
                        descEspecialidad = EspecialidadRepositorio.Obtener(Int32.Parse(iorEspecialidad)).DESCRIPCION;
                    }   
                }

                PacienteQreport oPacienteResult = new PacienteQreport
                {
                    oid= oPaciente.OID,
                    nombre = oPaciente.PACIENTE1,
                    email = oPaciente.EMAIL,
                    dni= oPaciente.DNI,
                    qrcompartircaso = oPaciente.COMPARTIR,
                    codmut= codMutua,
                    descmut = descMutua,
                    ior_colegiado = iorColegiado,
                    medico_referidor = nombreReferidor,
                    ior_tipoexploracion = iorTipoExploracion,
                    descexploracion = descTipoExploracion,
                    ior_especialidad = iorEspecialidad,
                    descespecialidad = descEspecialidad
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

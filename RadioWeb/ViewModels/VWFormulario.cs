using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace RadioWeb.ViewModels
{
    public class VWFormulario
    {

        public VWFormulario(int oid)
        {
            RadioDBContext db = new RadioDBContext();
            TIPOS = db.Formulario_Tipo_Elemento.ToList();
            CURRENTFORMULARIO = db.Formulario.Single(f => f.OID == oid);
            //Utilizado para vincular las preguntas tipo Radio con posibles respuestas
            RESPUESTAS = db.Formulario_Respuestas.ToList();
            FORMULARIOS = db.Formulario.ToList();
            //en la tabla pregunta obtenemos todas las preguntas del formulario actual
            PREGUNTAS = db.Formulario_Pregunta.Where(f => f.IOR_FORMULARIO == oid).ToList();
            //para cada pregunta tenemos que obtener las posibles respuestas
            foreach (FORMULARIO_PREGUNTA pregunta in PREGUNTAS)
            {
                pregunta.TIPO = TIPOS.Single(t => t.OID == pregunta.IOR_TIPO);
                List<FORMULARIO_PREGUNTAS_RESPUESTAS> oRespuestas = db.Formulario_Pregunta_Respuestas
                                                                    .Where(p => p.IOR_PREGUNTA == pregunta.OID)
                                                                     .ToList();

                pregunta.RESPUESTAS = new List<FORMULARIO_RESPUESTAS>();
                foreach (FORMULARIO_PREGUNTAS_RESPUESTAS respuesta in oRespuestas)
                {
                    pregunta.RESPUESTAS.Add(db.Formulario_Respuestas.Single(r=>r.OID==respuesta.IOR_RESPUESTA));
                }
            }

        }
    


        public List<FORMULARIO_TIPO_ELEMENTO> TIPOS { get; set; }
        public FORMULARIO CURRENTFORMULARIO{ get; set; }
        public List<FORMULARIO> FORMULARIOS { get; set; }
        public List<FORMULARIO_PREGUNTA> PREGUNTAS { get; set; }

        public List<FORMULARIO_RESPUESTAS> RESPUESTAS { get; set; }
        public int IOR_FORMULARIO { get; set; }

      



        
        public int OIDEXPLORACION { get; set; }
    }

   
}
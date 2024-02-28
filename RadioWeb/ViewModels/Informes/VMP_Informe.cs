using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.ViewModels.Informes
{
    public class VMP_Informe
    {
       

        public int OID { get; set; }
        public VMP_Informe()
        { }


        public VMP_Informe(int OID)
        {
            RadioDBContext db = new RadioDBContext();
            P_INFORMES oPlantilla;
            IDIOMAS = DataBase.Idiomas();
            TIPOS = DataBase.TiposPlantillas();

            //SI SE TRATA DE UNA PLANTILLA EXISTENTE
            if (OID > 0)
            {
                oPlantilla = db.P_Informes.Single(i => i.OID == OID);
                VALORACION oValoracion = db.Valoracion.Where(v => v.OWNER == OID).FirstOrDefault();
                this.OID = oPlantilla.OID;
                this.OWNER = oPlantilla.OWNER;
                this.TITULO = oPlantilla.TITULO;
                //this.FECHA = oPlantilla.FECHA.Value;
                this.TEXTOHTML = InformesRepositorio.ObtenerHtmlDelInforme(OID);
            }
            else {
               
            }
           
     

          

        }

        [NotMapped]
        public string ACTION
        {
            get
            {
                return (OID != 0 ? "Edit" : "Create");
            }
        }
        
        //GRUPO DE APARATOS
        public Nullable<int> OWNER { get; set; }

      
        [Required]
        public string TITULO { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fecha")]
        public DateTime FECHA { get; set; }


        public string HORAMOD { get; set; }

        public string HORA { get; set; }

     
        public Nullable<int> IOR_TECNICO { get; set; }

        public Nullable<int> IOR_MODALIDAD { get; set; }

        public Nullable<int> IOR_SITUACION { get; set; }

        public Nullable<int> IOR_TECNICA { get; set; }

        public int TIPO { get; set; }

        public Dictionary<int, string> TIPOS { get; set; }

        public string IDIOMA { get; set; }

        public Dictionary<string, string> IDIOMAS { get; set; }

        [NotMapped]
        [AllowHtml]
        public string TEXTOHTML { get; set; }

    }

    public class TIPOPLANTILLA
    {
        public int ID { get; set; }
        public String DESCRIPCION { get; set; }
    }
}
using RadioWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RadioWeb.ViewModels
{
    public class VWColegiado
    {

        public VWColegiado()
        {
            this.TRATAMIENTOS = new List<TRATAMIENTOS>();
            this.TRATAMIENTOS.Add(new TRATAMIENTOS { ID = 1, DESCRIPCION = "Dr." });
            this.TRATAMIENTOS.Add(new TRATAMIENTOS { ID = 2, DESCRIPCION = "Dra." });

            this.DIRECCIONES = new HashSet<DIRECCION>();
            this.TELEFONOS = new HashSet<TELEFONO>();
        }
        public int OID { get; set; }

        [Required]
        [DisplayName("Nombre")]
        public string NOMBRE { get; set; }

        [Required]
        [DisplayName("Apellidos")]
        public string APELLIDOS { get; set; }

        [Required]
        [DisplayName("Tratamiento")]
        public string TRATA { get; set; }

        public List<TRATAMIENTOS> TRATAMIENTOS { get; set; }

        [DisplayName("D.N.I.")]
        public string NIF { get; set; }

        [Display(Name = "Email")]        
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string EMAIL { get; set; }

        [DisplayName("Colegiado")]
        public string COD_MED { get; set; }

        [DisplayName("Comentario")]
        public string ESPEC { get; set; }


        [DisplayName("Especialidad")]
        public int IOR_ESPECIALIDAD { get; set; }

        public IEnumerable<ESPECIALIDADES> ESPECIALIDADES { get; set; }

        [DisplayName("Centro Adscrito")]
        public Nullable<int> IOR_CENTRO { get; set; }


        public IEnumerable<CENTROSEXTERNOS> CENTROSEXTERNOS { get; set; }

        public string TEXTO { get; set; }

        public string HEADING { get; set; }

        public string ACTION
        {
            get
            {
                return (OID != 0) ? "Update" : "Create";
            }

        }

        public virtual ICollection<DIRECCION> DIRECCIONES { get; set; }
        public virtual ICollection<TELEFONO> TELEFONOS { get; set; }

        //Campo de logica de negocio que se usa para saber si estamos asociando una exploración a dicho colegiado
        public int OIDEXPLORACION { get; set; }
    }

    public class TRATAMIENTOS {
        public int ID { get; set; }
        public String DESCRIPCION { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RadioWeb.Models;
using System.ComponentModel.DataAnnotations;
using RadioWeb.Models.Repos;
using System.ComponentModel;

namespace RadioWeb.ViewModels
{
    public class VWFactura
    {
        public VWFactura()
        {
            EmpresaRepositorio oRepoEmpresa = new EmpresaRepositorio();
            this.NOMBREEMPRESA = oRepoEmpresa.Obtener(4).NOMBRE;
            this.LINEAS = new HashSet<LINEAS_FACTURAS>();
            this.LINEAS.Add(new LINEAS_FACTURAS
            {
                BORRADO = "F",
                PERMISO="T",
                OWNER = -1,
                TEXTO = "ESPECIFIQUE TEXTO"
            });
            this.IOR_EMPRESA = 4;
          
        }
      

        [Required]
        public int OID { get; set; }


        [Required]
        [DisplayName("PAGADOR")]
        public string NOMBRE { get; set; }

        public string DIRECCION { get; set; } = "";

        public string CP { get; set; } = "";

        public string CIUDAD { get; set; } = "";

        public string PROVINCIA { get; set; } = "";

        [Required]
        public Nullable<int> IOR_EMPRESA { get; set; }

        [Required]
        [DisplayName("NUMERO FACTURA")]
        public Nullable<double> NUM_FAC { get; set; }

        [DisplayName("FECHA FACTURA")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FECHA_FAC { get; set; }

        [DisplayName("FECHA INICIAL")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FECHA_INICIAL { get; set; }

        [DisplayName("FECHA FINAL")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FECHA_FINAL { get; set; }

        [DisplayName("FECHA EXPLORACION")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FECHAEXPLORACION { get; set; }


        [Required]
        [DisplayName("SERIE")]   
        [MaxLength(1, ErrorMessage = "Serie solo admite un digito")]
        public string COD1 { get; set; }

        //Se utiliza para saber si una factura es multiple o simple
        public string COD2 { get; set; }

        //se utiliza para saber si una factura ya se ha calculado
        public string COD3 { get; set; }

        public string DNI { get; set; } = "";

        public string EMPRESA { get; set; }

        [Required]        
        public string TOTALSINIVA { get; set; }

        [Required]        
        public string IVAIMPUTADO { get; set; }

        [Required]            
        public string TOTALCONIVA { get; set; }

        [Required]
        public float IVA { get; set; }

        [Required]
        public Nullable<int> IOR_PAC { get; set; }
        public Nullable<int> IOR_GPR { get; set; }
        public Nullable<int> IOR_ENTIDADPAGADORA { get; set; }

        //[DataType("CENTROS")]
        //[DisplayName("CENTRO")]
        //public int IOR_CENTRO { get; set; }

        public Nullable<int> IOR_MONEDA { get; set; }
        public string SIMBOLO { get; set; }
        public int? IOR_EXPLORACION { get; set; }

        [DataType("CENTROS")]
        [DisplayName("CENTRO")]
        public Nullable<int> OWNER { get; set; }


        public string HEADING { get; set; }

        public string ACTION
        {
            get
            {
                return (OID != 0) ? "Edit" : "Create";
            }

        }

        public virtual ICollection<LINEAS_FACTURAS> LINEAS { get; set; }
        //public EXPLORACION EXPLORACION { get; set; }


        [DisplayName("Comentario")]
        public string COMENTARIO { get; set; }
        public string PACIENTE { get; set; }
        public string NOMBREEMPRESA { get; set; }
        public string CIFEMPRESA { get; set; }
        public string LOGO { get; set; }

        public string URLPREVIA { get; set; }

    }
}
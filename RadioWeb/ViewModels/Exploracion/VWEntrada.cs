using RadioWeb.Models;
using RadioWeb.Models.Validation;
using RadioWeb.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels.Exploracion
{
    public class VWEntrada
    {
        public VWEntrada()
        {
            this.DIRECCIONES = new HashSet<DIRECCION>();
            this.TELEFONOS = new HashSet<TELEFONO>();
            this.TRATAMIENTOS = DataBase.Tratamientos();
        }

        public Dictionary<string,string> TRATAMIENTOS { get; set; }

        public int OIDEXPLORACION { get; set; }

        public string HORALL { get; set; }

        public string HORA { get; set; }

        [ContieneComa(ErrorMessage = "El nombre debe contener una coma")]
        public string PACIENTE { get; set; }

        [DisplayName("Qreport")]
        public bool QRCOMPARTIRCASO { get; set; }

        public string DNI { get; set; }

        public string DNIRESPONSABLE { get; set; }

        public string RESPONSABLE { get; set; }

        public string IDIOMA { get; set; }

        public string FACTURADA { get; set; }
        public string PAGADA { get; set; }        

        public string INTOCABLE { get; set; }

        [DataType("SEXO")]
        [DisplayName("SEXO")]
        public string SEXO { get; set; }

        public Dictionary<string, string> IDIOMAS { get; set; }

        [DisplayName("FECHA NACIMIENTO")]
        public string FECHANACIMIENTO { get; set; }

        [DisplayName("TARJETA")]
        [MaxLength(30, ErrorMessage = "La Tajerta no puede ser superior a 30 dígitos")]
        public string TARJETA { get; set; }

        [DisplayName("REGISTRO")]
        [MaxLength(20, ErrorMessage = "El Registre no puede ser superior a 20 dígitos")]
        public string REGISTRE { get; set; }

        [DisplayName("CIP")]
        [MaxLength(30, ErrorMessage = "El CIP no puede ser superior a 15 dígitos")]
        public string CIP { get; set; }

        [DisplayName("TARIFA/MUTUA")]
        public int IOR_ENTIDADPAGADORA { get; set; }

        [DisplayName("F. INFORME"), DataType("FECHAPICKER")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> FECHAMAXENTREGA { get; set; }

        [DisplayName("AUTORIZACIÓN")]
        public string NHCAP { get; set; }

        public int IOR_PACIENTE { get; set; }

        [DataType("CONDICION")]
        [DisplayName("PARAM1")]
        public int IOR_CONDICION{ get; set; }

        public string MUTUADESCRIPCION { get; set; }

        public List<MUTUAS> MUTUAS { get; set; }

        public int TELEFONOOID { get; set; }

        public string TELEFONO { get; set; }

        public string EMAIL { get; set; }

        public string ESPERA { get; set; }

        [DisplayName("MÉDICO INFORME")]
        public int IOR_MEDICOINFORMANTE { get; set; }        

        public string NOMBREMEDICOINFORMANTE { get; set; }

        [DisplayName("CENTRO EXTERNO")]
        public int IOR_CENTROEXTERNO { get; set; }
     
        public string NOMBRECENTROEXTERNO { get; set; }

        public int IOR_APARATO { get; set; }

        [DisplayName("CARDIOLOGO/CIRUJANO")]
        public int IOR_CARDIOLOGO { get; set; }

        public EMPRESAS EMPRESA { get; set; }

        public List<PERSONAL> CARDIOLOGOS { get; set; }

        public List<PERSONAL> MEDICOS { get; set; }

        public List<CENTROSEXTERNOS> CENTROEXTERNOS { get; set; }

        [DisplayName("MÉDICO REFERIDOR")]
        public int IOR_MEDICOREFERIDOR { get; set; }

        public string NOMBREMEDICOREFERIDOR { get; set; }
        public string SIMBOLO { get; set; }

        [DisplayName("TRATAMIENTO")]
        public Nullable<short> TRAC { get; set; }

        public List<COLEGIADOS> MEDICOSREFERIDORES { get; set; }

        [DisplayName("TIPO EXPLORACIÓN")]
        [DataType("TIPOEXPLORACION")]
        public int IOR_TIPOEXPLORACION { get; set; }

        public bool INFORMADA { get; set; }

        public int IOR_INFORME { get; set; }

        [Required]
        [DataType("MONEDA")]
        public double IMPORTE { get; set; }

        public List<APARATOS> TIPOSEXPLORACION { get; set; }

        public CONSUMIBLES DEFAULTCONSUMIBLE { get; set; }

        [DisplayName("NUM. TICKET")]
        public string TICKET_KIOSKO { get; set; }


        public virtual ICollection<DIRECCION> DIRECCIONES { get; set; }
        public virtual ICollection<TELEFONO> TELEFONOS { get; set; }


    }
}
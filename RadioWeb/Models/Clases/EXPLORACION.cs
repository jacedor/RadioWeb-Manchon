//------------------------------------------------------------------------------
// <auto-generated>
//    Este código se generó a partir de una plantilla.
//
//    Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//    Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RadioWeb.Models
{
    using global::ADPM.Common.Web.DataAnnotations;
    using RadioWeb.Models.Validation;
    using Repos;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Utils;

    public partial class EXPLORACION
    {
        public EXPLORACION()
        {
            this.Q_ALFAS = DescuentosRepositorio.Obtener();
        }

        public enum ESTADOS
        {
            PENDIENTE ,
            BORRADO ,
            PRESENCIA,
            CONFIRMADO, 
            NOPRESENTADO,
            LLAMAANULANDO
        }
        public int OID { get; set; }

        public Nullable<int> VERS { get; set; }

        public Nullable<int> CID { get; set; }

        public string CANAL { get; set; }

        public Nullable<int> OWNER { get; set; }

        public string USERNAME { get; set; }
        public Nullable<System.DateTime> MODIF { get; set; }
        //Virtual EMPRESA
        public Nullable<int> IOR_EMPRESA { get; set; }
        public string BORRADO { get; set; }

        [Required(ErrorMessage = "Campo Obligatorio")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> FECHA { get; set; }

        [Required(ErrorMessage = "Campo Obligatorio")]
        public string HORA { get; set; }
        public string COD_CAP { get; set; }
        public string CABINF_EXPLO { get; set; }

        [DataType("MONEDA")]
        [Required(ErrorMessage = "Cantidad Obligatoria")]
        public Nullable<double> CANTIDAD { get; set; }

        public string ESTADO { get; set; }

        [DataType("ESTADOEXPLORACION")]
        public string ESTADODESCRIPCION { get; set; }

        public string TIPOEXPLORACIONDESC { get; set; }

        public string FACTURADA { get; set; }

        [DataType("BooleanString")]
        public string GDS { get; set; }

        [DataType("BooleanString")]
        public string PAGADO { get; set; }
        public string APLAZADO { get; set; }
        public string INFORMADA { get; set; }

        public string RECORDED { get; set; }


        [DataType("BooleanString")]
        [DisplayName("CONSEN")]
        public string FIRMA_CONSEN { get; set; }
        public string LOPD { get; set; }
        public string RUTA_LOPD { get; set; }
        public string RUTA_CONSEN { get; set; }
        public string NOMODIFICA { get; set; }
        public string NUM_FAC { get; set; }
        public string NUMEROS { get; set; }
        public Nullable<double> ALFA { get; set; }
        public string Q_ALFA { get; set; }
        public string ARCHIVOBADALONA { get; set; }
        
        
        [Display(Name ="DESCUENTOS")]
        public List<DESCUENTOS> Q_ALFAS { get; set; }

        [DataType("BooleanString")]
        [Display(Name = "AUTORIZADO")]
        public string PERMISO { get; set; }

        public string MEDICO { get; set; }

        public Nullable<short> PRIVADO { get; set; }

        public string IDENTIFICA { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> FECHA_IDEN { get; set; }
        public string HORA_IDEN { get; set; }

        public string HORA_LL { get; set; }
        public string HORA_EX { get; set; }
        public string HORAMOD { get; set; }

        [DisplayName("PESO"), DataType("String")]
        public Nullable<short> PESO { get; set; }

       
        public Nullable<System.DateTime> FECHADERIVACION { get; set; }
        [DisplayName("INF. RECOGIDO"), DataType("RECOGIDOINFORME")]
        public string RECOGIDO { get; set; }
        public string CONTRASTE { get; set; }
        public string TIPOPAGO { get; set; }
        public string MEDICO2 { get; set; }

        [DataType("StringMultiline")]
        public string MOTIVO { get; set; }
        public string NOFACTURAB { get; set; }

        [Display(Name ="AUTORIZACIÓN")]
        public string NHCAP { get; set; }

        public string CITAICS { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        [DisplayName("FECHA ENVIO")]
        [DataType("FECHAPICKER")]
        public Nullable<System.DateTime> FECHAENVIO { get; set; }

        [DataType("BooleanString")]
        public string PAGAR { get; set; }

        public Nullable<double> DEPOSITO { get; set; }
        public string INTOCABLE { get; set; }

        [DisplayName("TIPO")]
        public Nullable<int> IOR_GPR { get; set; }

        //Virtual MUTUA      
        [DataType("MUTUASLIST")]
        [EnteroPositivo(ErrorMessage = "Debe asignar una mutua")]
        [DisplayName("MUTUA")]
        public Nullable<int> IOR_ENTIDADPAGADORA { get; set; }

        
        public int IOR_PACIENTE { get; set; }
        //Virtual COLEGIADO
        [DisplayName("MÉDICO REFERIDOR")]
        public Nullable<int> IOR_COLEGIADO { get; set; }
        //Virtual COLEGIADO
        [DisplayName("CAR/CIR"), DataType("CARDIOLOGO")]
        public Nullable<int> IOR_CIRUJANO { get; set; }

        [DisplayName("PARAM1"), DataType("CONDICION")]
        public Nullable<int> IOR_CONDICION { get; set; }


        [DisplayName("Cárdiologo"), DataType("CARDIOLOGO")]
        public Nullable<int> IOR_CARDIOLOGO { get; set; }

        //Virtual MEDICO
        [DisplayName("MÉD. INFORMANTE"), DataType("MEDICO")]
        public Nullable<int> IOR_MEDICO { get; set; }

        public Nullable<int> IOR_MEDREVISION { get; set; }
        //Virtual APARATO
        [DataType("DAPARATO")]
        [EnteroPositivo(ErrorMessage = "Debe asignar una aparato")]
        [DisplayName("APARATO")]
        public Nullable<int> IOR_APARATO { get; set; }
       
        public Nullable<int> IOR_GRUPO { get; set; }

        public Nullable<int> IOR_MASTER { get; set; }
        [NotMapped]
        public Nullable<int> IOR_PETICIONPRUEBAORIGEN { get; set; }

        [DataType("TIPOEXPLORACION")]
        [DisplayName("TIPO EXPLORACION")]
        public Nullable<int> IOR_TIPOEXPLORACION { get; set; }
        public Nullable<int> IOR_FACTURA { get; set; }

        public Nullable<int> IOR_CODIGORX { get; set; }
        public string HAYCOMEN { get; set; }

        [DataType("StringMultiline")]
        public string TEXTO { get; set; }
        //Virtual MONEDA
        public Nullable<int> IOR_MONEDA { get; set; }

        [DisplayName("F. FAC.")]
        [DataType("FECHAPICKER")]
        public Nullable<System.DateTime> FECHA_FAC { get; set; }

       

            
        [TableIcon(Action = "Test", Controller = "Controller",
           FaIconTrue = "fa fa-eyedropper", 
           ColorTrue = "green")]
        public string HAYCONSUMIBLE { get; set; }

        [DisplayName("TECNICO"),DataType("TECNICO")]
        public Nullable<int> IOR_TECNICO { get; set; }

        public string USERMOD { get; set; }

       

        [DisplayName("ESTUDIANTE"), DataType("ESTUDIANTE")]
        public Nullable<int> IOR_ESTUDIANTE { get; set; }

        [DisplayName("CENTRO EXTERNO"), DataType("CENTROEXTERNO")]
        public Nullable<int> IOR_CENTROEXTERNO { get; set; }

        [Display(Name ="ID EXT.")]
        public string IDCITAONLINE { get; set; }
        //public string GUARDARYVOLVER { get; set; }

                
        [DisplayName("FECHA MAXIMA ENTREGA"), DataType("FECHAPICKER")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> FECHAMAXENTREGA { get; set; }

        public string DIASEMANA { get; set; }

        public string REGISTRE { get; set; }

        public virtual string TEXTOHISTORIACLINICA { get; set; }
        public virtual INFOMUTUAS INFOMUTUA{ get; set; }
        public virtual DAPARATOS DAPARATO { get; set; }
        //Lista de Aparatos para los aparatos complejos
        public virtual List<DAPARATOS> DAPARATOS { get; set; }
        public virtual GAPARATOS GAPARATO { get; set; }
        public virtual APARATOS APARATO { get; set; }
        //DADO UNA ENTIDAD SE APLICA UNA TARIFA A UNA EXPLORACION. DADA ESTA ENTIDAD
        //UNA EXPLORACION PUEDE TENER ENE POSIBLES EXPLORACIONES CUBIERTAS POR ESTA ENTIDAD
        //QUE ALMACENAMOS EN ESTA LISTA
        public virtual List< APARATOS>  EXPLORACIONESCONTARIFA { get; set; }

        //ESTOS SON LOS CÓDIGOS DE EQUIVALENCIAS DEL ICS
        public virtual List<RX> CODIGOSICS { get; set; }
        //esto es el medico externo que ha enviado la exploracion
        public virtual COLEGIADOS COLEGIADO { get; set; }
        public virtual CENTROSEXTERNOS CENTROEXTERNO { get; set; }
        public virtual EMPRESAS EMPRESA { get; set; }

        public virtual PERSONAL MEDICOINFORMANTE { get; set; }
        public virtual GPR GPR { get; set; }
        [UIHint("PACIENTE")]
        public virtual PACIENTE PACIENTE { get; set; }

        //creemos que es campo es obsoleto
        public virtual MEDICOS oMEDICO { get; set; }
        public virtual MONEDAS MONEDA { get; set; }

        public List<EXP_CONSUM> CONSUMIBLES { get; set; }
        public List<PAGOS> PAGOS { get; set; }
        public virtual MUTUAS ENTIDAD_PAGADORA { get; set; }
        public virtual List<IMAGENES> IMAGENES { get; set; }
        public virtual List<REFRACTOMETROS> DOCUMENTOS { get; set; }
        public virtual List<LOGUSUARIOS> LOGUSUARIOS { get; set; }
        public virtual List<LOGVIDSIGNER> LOGVIDSIGNER { get; set; }
        public virtual List<LISTADIA> EXPLORAMISMODIA { get; set; }

        [UIHint("DOCSPRINTEXPLORACION")]
        public virtual List<REFRACTOMETROS> DOCUMENTOSIMPRIMIBLES { get; set; }

        public string OTRASEXPLORACIONESRECOGIDAS { get; set; }

        [NotMapped]
        public string URLPREVIAEXPLORACION { get; set; }

        public string QREPORT_ENVIADO { get; set; }

        [DisplayName("NUM. TICKET")]
        public string TICKET_KIOSKO { get; set; }

    }
}
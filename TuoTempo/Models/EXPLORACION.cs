//------------------------------------------------------------------------------
// <auto-generated>
//    Este código se generó a partir de una plantilla.
//
//    Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//    Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TuoTempo.Models
{
   
    using Repos;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Xml;

    public partial class EXPLORACION
    {
        public EXPLORACION()
        {
            // this.Q_ALFAS = DescuentosRepositorio.Obtener();
            this.OWNER = 1;
            this.IOR_EMPRESA = 4;
            this.BORRADO = "F";
            this.ESTADO = "0";
            this.FACTURADA = "F";
            this.APLAZADO = "F";
            this.INFORMADA = "F";
            this.NOMODIFICA = "F";
            this.RECOGIDO = "F";
            this.PERMISO = "T";
            this.NOFACTURAB = "T";
            this.PAGAR = "T";
            this.INTOCABLE= "F";
            this.PACS_IMG_SN= "F";
            this.ENVIAR_MAIL= "F";
            this.PACS_IMG_SN= "F";
            this.IOR_MASTER= 0;
            this.IOR_CODIGORX=-1;
            this.FECHA_IDEN = DateTime.Now;

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
        public int VERS { get; set; }
        public int CID { get; set; }
        public string CANAL { get; set; }
        public int OWNER { get; set; }
        public string USERNAME { get; set; }
        public DateTime MODIF { get; set; }
        public int IOR_EMPRESA { get; set; }
        public string BORRADO { get; set; }
        public DateTime FECHA { get; set; }
        public string HORA { get; set; }
        public string COD_CAP { get; set; }
        public double CANTIDAD { get; set; }
        public string ESTADO { get; set; }
        public string FACTURADA { get; set; }
        public string PAGADO { get; set; }
        public string APLAZADO { get; set; }
        public string INFORMADA { get; set; }
        public string NOMODIFICA { get; set; }
        public string NUM_FAC { get; set; }
        public string NUMEROS { get; set; }
        public double ALFA { get; set; }
        public string Q_ALFA { get; set; }
        public string PERMISO { get; set; }
        public string MEDICO { get; set; }
        public short PRIVADO { get; set; }
        public string IDENTIFICA { get; set; }
        public DateTime FECHA_IDEN { get; set; }
        public string HORA_IDEN { get; set; }
        public string HORA_LL { get; set; }
        public string HORA_EX { get; set; }
        public string HORAMOD { get; set; }
        public short PESO { get; set; }
        public string RECOGIDO { get; set; }
        public string CONTRASTE { get; set; }
        public string TIPOPAGO { get; set; }
        public string MEDICO2 { get; set; }
        public string NOFACTURAB { get; set; }
        public string NHCAP { get; set; }
        public string CITAICS { get; set; }
        public DateTime FECHAENVIO { get; set; }
        public string PAGAR { get; set; }
        public double DEPOSITO { get; set; }
        public string INTOCABLE { get; set; }

        // Continuación de las propiedades
        public int IOR_GPR { get; set; }
        public int IOR_ENTIDADPAGADORA { get; set; }
        public int IOR_PACIENTE { get; set; }
        public int IOR_COLEGIADO { get; set; }
        public int IOR_MEDICO { get; set; }
        public int IOR_APARATO { get; set; }
        public int IOR_GRUPO { get; set; }
        public int IOR_TIPOEXPLORACION { get; set; }
        public int IOR_FACTURA { get; set; }
        public int IOR_CODIGORX { get; set; }
        public string HAYCOMEN { get; set; }
        public int IOR_MONEDA { get; set; }
        public DateTime FECHA_FAC { get; set; }
        public string HAYCONSUMIBLE { get; set; }
        public int IOR_TECNICO { get; set; }
        public string USERMOD { get; set; }
        public DateTime FECHADERIVACION { get; set; }
        public int IOR_ESTUDIANTE { get; set; }
        public int IOR_CENTROEXTERNO { get; set; }
        public string IDCITAONLINE { get; set; }
        public DateTime FECHAMAXENTREGA { get; set; }
        public string FIRMA_CONSEN { get; set; }
        // Tipo blob puede ser representado como un array de bytes en C#
        public byte[] MOTIVO { get; set; }
        public int IOR_CIRUJANO { get; set; }
        public int IOR_MASTER { get; set; }
        public int IOR_MEDREVISION { get; set; }
        public string REGISTRE { get; set; }
        public string CABINF_EXPLO { get; set; }
        public string ARCHIVOBADALONA { get; set; }
        public string GDS { get; set; }
        public int IOR_CARDIOLOGO { get; set; }
        public int IOR_CONDICION { get; set; }
        public string ENVIAR_MAIL { get; set; }
        public string RECORDED { get; set; }
        public int IOR_SITIO { get; set; }
        public string PACS_IMG_SN { get; set; }
        public int IOR_MOTDESPROG { get; set; }
        public string QREPORT_ENVIADO { get; set; }
        public int IOR_RECOGIDO { get; set; }
        public DateTime FECHA_RECOGIDA { get; set; }
        public string HORA_RECOGIDA { get; set; }
        public string NOTIFICADO_SN { get; set; }
        public string TICKET_KIOSKO { get; set; }
        public string JUSTIFICACION { get; set; }
        public string FIRMA_MEDICO { get; set; }
        // ... Continuar con el resto de las propiedades
        // Asegúrate de utilizar los tipos de datos adecuados para cada propiedad.
    }

}

using RadioWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels
{
    public class VWImprimirFicha
    {
        public VWImprimirFicha()
        {
            this.EXPLOSASOCIADAS = new List<ViewModels.OTRASEXPLORACIONES>();
        }

        public int OID { get; set; }
        public int CID { get; set; }
        public string esMamo { get; set; }
        public DateTime FECHA { get; set; }
        public string HORA { get; set; }
        public string HORA_LL { get; set; }
        public string EXPLO{ get; set; }
        public string TEXTO { get; set; }

        public string APARATO { get; set; }
        public string  COD_GRUP { get; set; }
        public string DES_FIL { get; set; }

        public string COD_PAC { get; set; }
        public string DNI { get; set; }
        public long EDAD { get; set; }        
        public string EMAIL { get; set; }
        public string FECHAN { get; set; }
        public String FECHAMAXIMA { get; set; }
        public int IOR_PACIENTE { get; set; }
        public string PACIENTE { get; set; }
        public string MOVILPACIENTE { get; set; }
        public string PROFESION { get; set; }
        public string SEXO { get; set; }
        public int TRAC { get; set; }
        public string TEXTOPACIENTE { get; set; }
        public string HISTORIACLINICA { get; set; }

        public string HORARIOREGOGIDA { get; set; }
        public string HORARIOREGOGIDA2 { get; set; }
        public string DIRECCION { get; set; }
        public string CP { get; set; }
        public string POBLACION { get; set; }
        public string PROVINCIA { get; set; }

        public string NUMERO { get; set; }

        public string NUMEROMOVIL { get; set; }
        public int OWNER { get; set; }
        public string CODMUT { get; set; }
        public string NOMBRE { get; set; }
        public string POLIZA { get; set; }

        public string NOMBRECOLEGIADO { get; set; }

        public string COD_MED { get; set; }
        public string ESPECIALIDAD { get; set; }
        public string ESPEC { get; set; }
        public string TRATA { get; set; }

        public string NOMBRECENTRO { get; set; }
        public string DIRECCIONCENTRO { get; set; }
        public string CPCENTRO { get; set; }
        public string CIUTATCENTRO { get; set; }
        public string CONDICION { get; set; }
        

        public int CENTROEXTERNOOID { get; set; }
        public string CENTROEXTERNO { get; set; }
        public string QRCOMPARTIRCASO { get; set; }

        public byte[] BARCODEIMAGE { get; set; }
        public List<OTRASEXPLORACIONES> EXPLOSASOCIADAS { get; set; }
        public List<VID_RESPUESTAS> RESPUESTAS { get; set; }
        public string TELEFONOCENTRO { get; internal set; }
        public byte[] CODIGOBARRAS { get; set; }
        public int IOR_MASTER { get; set; }
    }

    public class OTRASEXPLORACIONES
    {
        public int OID { get; set; }
        public string HORA { get; set; }
        public DateTime FECHA { get; set; }
        public string COD_MUT{ get; set; }
        public string COD_FIL { get; set; }
        public string FIL { get; set; }
        public string DES_FIL { get; set; }
        public int IOR_MASTER { get; set; }
    }
}
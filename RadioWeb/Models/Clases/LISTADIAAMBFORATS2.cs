using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models
{

    public class LISTADIAAMBFORATS2
    {
        public string ANULADA { get; set; }
        public string APLAZADO { get; set; }
        public string ANULACONSENTIMIENTO{ get; set; }
        public Nullable<int> CANCELADO { get; set; }
        public Nullable<double> CANTIDAD { get; set; }
        public Nullable<int> CENTRO { get; set; }
        public string CID { get; set; }
        //color que se muestra en la casilla de la lista
        public string CIDCOLOR { get; set; }        
        public string COLORHORARIO { get; set; }
        public string COLEGIADO { get; set; }
        public string COLOR { get; set; }
        public string COLORESTADO { get; set; }
        public string COD_MED { get; set; }
        public string COD_MUT { get; set; }
        public string DES_MUT { get; set; }
        public string COD_FIL { get; set; }
        public string DES_FIL { get; set; }
        public string DESAPARATO { get; set; }
        public Nullable<int> EDAD { get; set; }
        

        //SI ESTA HECHA, EN ESPERA, SI ESTA BORRADO
        public string ESTADO { get; set; }
        public string ESTADODESC { get; set; }
        public string ESHUECO { get; set; }
        public string CLASEESPECIAL { get; set; }
        public DateTime FECHA { get; set; }
        public DateTime? FECHAMAXENTREGA { get; set; }
        public string FACTURADA { get; set; }
        public string FIL { get; set; }
        public Nullable<int> GRUPOAPA { get; set; }
        public string HAYCOMEN { get; set; }
        public string HAYCONSUMIBLE { get; set; }
        //Hora del horario AQUI ES DONDE SE APLICA EL COLOR, SI ESTA LIBRE SE APLICA A TODA LA FILA SINO SE QUEDA EN LA COLUMNA
        public string HHORA { get; set; }
        public Nullable<int> IOR_PACIENTE { get; set; }
        public Nullable<int> IOR_GPR { get; set; }  
        public Nullable<int> OID { get; set; }
        public string HORA_LL { get; set; }
        public string HORA { get; set; }
        public string RUTA_CONSEN { get; set; }
        public string RUTA_LOPD { get; set; }
        public string HORA_EX { get; set; }
        public string INFORMADA { get; set; }
        public string RECORDED { get; set; }
        public string QRCOMPARTIRCASO { get; set; }
        public Nullable<int> IOR_MASTER { get; set; }
        public string FIRMA_CONSEN { get; set; }
        public string LOPD { get; set; }
        public string INTOCABLE { get; set; }
        public int? CITAEXTERNA { get; set; }
        //campo calculado QUE ES LA HORA ACTUAL MENOS LA HORA DE LLEGADA, SI LA HORA_EX ESTA LLENA LA DIFERENCIA CON ESTA
        public int? ESPERA { get; set; }        
        public string PACIENTE { get; set; }
        public string PAGADO { get; set; }
        public string MEDICO { get; set; }
        public string NUMEROS { get; set; }
        public string TEXTO { get; set; }
        public string TECNICO { get; set; }
        //NO ENSEÑAR
        public Nullable<int> PRIVADO { get; set; }
        //NO ENSEÑAR       
        public string SIMBOLO { get; set; }       
        public string NOFACTURAB { get; set; }
        //CAMPOS QUE NO ESTAN EN LISTADIA
        public Nullable<int> ORDER_HHORA { get; set; }
        public Nullable<int> ORDER_HORA { get; set; }
        public string VIP { get; set; }
        public string ACTIVA { get; set; }
        public Nullable<int> VERS { get; set; }     
        public string SUBTEXTO { get; set; }

        public Nullable<int> IOR_APARATO { get; set; }
        public PACIENTE PACIENTEOBJECT { get; set; }

        public string TEXTOAGENDA { get; set; }
    }
}
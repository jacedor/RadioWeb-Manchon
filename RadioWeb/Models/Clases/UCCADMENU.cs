using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace RadioWeb.Models
{
    [Table("UCCADMENU")]
    public class UCCADMENU
    {
        [Key]
        public int ID { get; set; }
        public int IDUSER { get; set; }
        public string DESCRIPCION { get; set; }
        public string ICONO { get; set; }
        public int IDPARENT { get; set; }
        public int ORDEN { get; set; }
        public string URL { get; set; }
       


    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace RadioWeb.Models
{
    [Table("UCCADUSER")]
    public class UCCADUSER
    {
        [Key]
        public int IDUSER { get; set; }
        public string NOME { get; set; }
        public string LOGIN { get; set; }
        public string SENHA { get; set; }
        public string EMAIL { get; set; }
        public string PRIVILEGIADO { get; set; }
        public string TIPO { get; set; }
        public int? PERFIL { get; set; }
        public string PASSWORDWEB { get; set; }
        public string PASSWORD { get; set; }
        public string ENCRIPTADO { get; set; }
        public int? CENTROASOCIADO { get; set; }


    }
}
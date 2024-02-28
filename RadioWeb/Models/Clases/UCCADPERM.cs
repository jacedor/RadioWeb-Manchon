using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace RadioWeb.Models
{
    [Table("UCCADPERM")]
    public class UCCADPERM
    {
        
        [Key]
        [Column(Order = 1)]
        public int IDUSER { get; set; }
        [Key]
        [Column(Order = 2)]
        public string MODULO { get; set; }
        [Key]
        [Column(Order = 3)]
        public string OBJNAME { get; set; }
        public int ESTADO { get; set; }
        public string VALOR { get; set; }
        


    }

}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace RadioWeb.Models
{
    [Table("UCCADMENUPERM")]
    public class UCCADMENUPERM
    {
        [Key]
        [Column(Order = 1)]
        public int IDMENU { get; set; }
        [Key]
        [Column(Order = 2)]
        public int IDUSER { get; set; }
       


    }
}
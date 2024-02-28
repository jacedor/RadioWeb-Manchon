namespace RadioWeb.Models
{

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class FK_CHECK
    {
        public string TABLA { get; set; }
        public string DEPENDENCIA { get; set; }
        public string FIELD_RELATED { get; set; }
        public int DELETE_RULE { get; set; }

    }
}
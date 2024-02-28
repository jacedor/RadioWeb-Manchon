using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;

namespace RadioWeb.Models.VidSigner
{
    public class Document
    {
        public string DocGui { get; set; }
        public string DocContent { get; set; }
        public string DocStatus { get; set; }
        public string FileName { get; set; }
        public bool OrderedSignatures { get; set; }
        public string NotificationURL { get; set; }

        public VidSignerClient ClienteVidSigner { get; set; }
        public List<Signer> Signers { get; set; }


        public int OIDPACIENTE { get; set; }
        public int OIDEXPLORACION { get; set; }
        public int OID { get; set; }


    }
}

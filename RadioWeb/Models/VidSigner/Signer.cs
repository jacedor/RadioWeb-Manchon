using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace RadioWeb.Models.VidSigner
{
    public class Signer
    {
        public string DeviceName { get; set; }
        public string NumberID { get; set; }
        public string PictureURI { get; set; }
        public string SignerGUI { get; set; }
        public string SignerName { get; set; }
        public string SignerFullName { get; set; }
        public string TypeOfID { get; set; }
        public string Email { get; set; }
        public Visible Visible { get; set; }
        public Form Form { get; set; }

    }

    public class Visible
    {
        public int Page { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int SizeX { get; set; }        
        public int SizeY { get; set; }
        //palabra clave que se usa en lugar de la posición para estampar la firma
        public string Anchor { get; set; }
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuoTempo.Models
{
  

   
        public partial class Paciente
        {
            public Paciente()
            {
                Origen = "TuoTempo";
            }

            public int OID { get; set; }
            public string Nombre { get; set; }
            public string Apellidos { get; set; }
            public string Dni { get; set; }
            public string Sexo { get; set; }
            public string FechaNacimiento { get; set; }
            public string Telefono { get; set; }
            public string IdManresa { get; set; }
            public string IdEnvio { get; set; }
            public string Origen { get; set; }
            public string Email { get; set; }
            public string IOR_MUTUA { get; set; }

       

        }
    }


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Logica
{
    public class Dia
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int PorcentageOcupacion { get; set; }
        public int PorcentageLibre { get; set; }
        public string Descripcion { get; set; }
        public string Texto { get; set; }
        public bool EsFestivo { get; set; }

        public Dia(int id, DateTime date, int countExploraciones, string Desc, string Texto, bool EsFestivo, int PorcentageOcupacion=0) 
        {
            this.Id = id;
            this.Date = date;
            if (countExploraciones != 0) {
                this.Count = countExploraciones;
            }
            
            this.Descripcion = date.Day.ToString();
            if (!string.IsNullOrEmpty(Texto)) {
                this.Texto = RadioWeb.Utils.DataBase.convertRtf(Texto);
            }

            if (PorcentageOcupacion != 0)
            {
                this.PorcentageOcupacion = PorcentageOcupacion;
                this.PorcentageLibre = 100 - this.PorcentageOcupacion;
            }
            else {
                this.PorcentageLibre = 100;
            }

            this.EsFestivo = EsFestivo;
        }

      
        
    }
}
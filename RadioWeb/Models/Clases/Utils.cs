using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;



namespace RadioWeb.Models.Utilidades
{



    public class CriteriosBusqueda
    {


       

        public static Dictionary<string, string> GetCriteriosDeBusqueda(Type claseManchon)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Type t = typeof(PACIENTE);
            var props = t.GetProperties();
            foreach (var prop in props)
            {
                var propattr = prop.GetCustomAttributes(true);
                
                foreach (System.Attribute attr in propattr)
                {
                    if (attr is CampoManchonAttribute)
                    {
                        CampoManchonAttribute a = (CampoManchonAttribute)attr;
                        if (a.Busqueda)
                        {

                            var attribute = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                            result.Add(prop.Name, ((System.ComponentModel.DisplayNameAttribute)(attribute[0])).DisplayName);
                        }
                        
                        //System.Console.WriteLine("   {0}, version {1:f}", a.GetName(), a.version);
                    }
                }
            }
            return result;


        }
    }

    public class JsonObject {
        public int id { get; set; }
        public string text { get; set; }
    
    }
}

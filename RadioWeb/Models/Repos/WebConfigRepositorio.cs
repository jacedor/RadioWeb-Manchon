using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;


namespace RadioWeb.Models.Repos
{
    public class WebConfigRepositorio
    {

        private RadioDBContext db = new RadioDBContext();

        public string ObtenerValor(string clave)
        {
            try
            {
#if DEBUG

                string Valor = db.WebConfig.SingleOrDefault(c => c.CLAVE.ToUpper() == clave.ToUpper() && c.VERS == 1).VALOR;
                return Valor;
#else
             string Valor = db.WebConfig.SingleOrDefault(c => c.CLAVE.ToUpper() == clave.ToUpper() && c.VERS==0).VALOR;          
                 return Valor;
#endif
            }
            catch (Exception ex)
            {

                LogException.LogMessageToFile("ERROR CLAVE NO ENCONTRADA" + clave);
                throw ex;
            }
            finally
            {

            }



        }



    }
}
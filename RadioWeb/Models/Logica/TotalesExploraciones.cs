using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Logica
{
    public class TotalesExploraciones
    {
        private static Dictionary<int, string> GruposDeAparatos;
        private static Dictionary<int, string> Aparatos;

        public TotalesExploraciones()
        {
            if (GruposDeAparatos == null)
            {
                GruposDeAparatos = new Dictionary<int, string>();
                FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
                oConexion.Open();
                FbCommand oCommand = new FbCommand();

                oCommand.CommandText = "select OID,COD_GRUP FROM GAPARATOS";


                oCommand.Connection = oConexion;

                FbDataReader oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    GruposDeAparatos.Add(DataBase.GetIntFromReader(oReader, "OID"), DataBase.GetStringFromReader(oReader, "COD_GRUP"));
                }
                oConexion.Close();
            }
            if (Aparatos == null)
            {
                Aparatos = new Dictionary<int, string>();
                FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
                oConexion.Open();
                FbCommand oCommand = new FbCommand();

                oCommand.CommandText = "select * from daparatos where IOR_EMPRESA=4  order by cod_fil";


                oCommand.Connection = oConexion;

                FbDataReader oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    Aparatos.Add(DataBase.GetIntFromReader(oReader, "OID"), DataBase.GetStringFromReader(oReader, "cod_fil"));
                }
                oConexion.Close();
            }
        }

        public string FECHA { get; set; }
        public int IOR_GPR { get; set; }
        public int IOR_GRUPO { get; set; }
        public string COD_GRUPO
        {
            get {
                return GruposDeAparatos[this.IOR_GRUPO];
            }           

        }
        public string COD_APARATO
        {
            get
            {
                return Aparatos[this.IOR_APARATO];
            }

        }
        public int IOR_APARATO { get; set; }
        public int TOTAL { get; set; }
    }
}
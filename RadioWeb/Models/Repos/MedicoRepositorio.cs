using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;


namespace RadioWeb.Models.Repos
{
    public class MedicoRepositorio
    {
        

        public static MEDICOS Obtener(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from medicos where oid=" + oid, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            MEDICOS oMedico = new MEDICOS();

            while (oReader.Read())
            {
                oMedico.OID = DataBase.GetIntFromReader(oReader, "OID");
                oMedico.BORRADO = oReader["BORRADO"].ToString();
                oMedico.CANAL = oReader["CANAL"].ToString();
                oMedico.CID = DataBase.GetIntFromReader(oReader, "CID");
                oMedico.COLEGIADO = oReader["COLEGIADO"].ToString();

                EmpresaRepositorio oEmpresa = new EmpresaRepositorio();
                oMedico.EMPRESA = oEmpresa.Obtener((int)oReader["IOR_EMPRESA"]);
                oMedico.MEDICO = oReader["MEDICO"].ToString();
                oMedico.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                oMedico.NOMBRE_MED = oReader["NOMBRE_MED"].ToString();
                oMedico.NUMERO = oReader["NUMERO"].ToString();
                oMedico.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                oMedico.USERNAME = oReader["USERNAME"].ToString();
                oMedico.USUARIOS = oReader["USUARIOS"].ToString();
                oMedico.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                

            }
            oCommand.Dispose();
            oConexion.Close();
            return oMedico;
        }


      
    }
}
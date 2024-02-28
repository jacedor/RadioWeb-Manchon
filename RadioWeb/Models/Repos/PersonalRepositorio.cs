using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using ADPM.Common;

namespace RadioWeb.Models.Repos
{

    public class PersonalRepositorio
    {

        public static PERSONAL Obtener(string LOGIN)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("SELECT * FROM PERSONAL a where (a.BORRADO ='F' OR A.BORRADO IS  null) and a.login=" + LOGIN.QuotedString(), oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            PERSONAL oPersonal = new PERSONAL();
            while (oReader.Read())
            {

                oPersonal.OID = (int)oReader["OID"];
                oPersonal.BORRADO = oReader["BORRADO"].ToString();
                oPersonal.CANAL = oReader["CANAL"].ToString();
                oPersonal.CID = DataBase.GetIntFromReader(oReader, "CID");
                oPersonal.COD = DataBase.GetStringFromReader(oReader, "COD");
                oPersonal.DESCRIPCION = DataBase.GetStringFromReader(oReader, "DESCRIPCION");
                oPersonal.DNI = DataBase.GetStringFromReader(oReader, "DNI");
                oPersonal.FECHAN = DataBase.GetDateTimeFromReader(oReader, "FECHAN");
                oPersonal.IOR_CARGO = DataBase.GetIntFromReader(oReader, "IOR_CARGO");
                oPersonal.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                oPersonal.LOGIN = DataBase.GetStringFromReader(oReader, "LOGIN");
                oPersonal.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                oPersonal.NIF = DataBase.GetStringFromReader(oReader, "NIF");
                oPersonal.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                oPersonal.NUMERO = DataBase.GetStringFromReader(oReader, "NUMERO");
                oPersonal.TRATA = DataBase.GetStringFromReader(oReader, "TRATA");
                oPersonal.HORAMOD = oReader["HORAMOD"].ToString();


            }
            oCommand.Dispose();
            oConexion.Close();
            return oPersonal;
        }
        public static PERSONAL Obtener(int OID)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("SELECT * FROM PERSONAL a where (a.BORRADO ='F' OR A.BORRADO IS  null) and a.IOR_CARGO =1 and a.OID=" + OID + " order by a.cod", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            PERSONAL oPersonal = new PERSONAL();
            while (oReader.Read())
            {

                oPersonal.OID = (int)oReader["OID"];
                oPersonal.BORRADO = oReader["BORRADO"].ToString();
                oPersonal.CANAL = oReader["CANAL"].ToString();
                oPersonal.CID = DataBase.GetIntFromReader(oReader, "CID");
                oPersonal.COD = DataBase.GetStringFromReader(oReader, "COD");
                oPersonal.DESCRIPCION = DataBase.GetStringFromReader(oReader, "DESCRIPCION");
                oPersonal.DNI = DataBase.GetStringFromReader(oReader, "DNI");
                oPersonal.FECHAN = DataBase.GetDateTimeFromReader(oReader, "FECHAN");
                oPersonal.IOR_CARGO = DataBase.GetIntFromReader(oReader, "IOR_CARGO");
                oPersonal.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                oPersonal.LOGIN = DataBase.GetStringFromReader(oReader, "LOGIN");
                oPersonal.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                oPersonal.NIF = DataBase.GetStringFromReader(oReader, "NIF");
                oPersonal.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                oPersonal.NUMERO = DataBase.GetStringFromReader(oReader, "NUMERO");
                oPersonal.TRATA = DataBase.GetStringFromReader(oReader, "TRATA");
                oPersonal.HORAMOD = oReader["HORAMOD"].ToString();


            }
            oCommand.Dispose();
            oConexion.Close();
            return oPersonal;
        }


        public static List<PERSONAL> ObtenerMedicos(bool mostrarBorrados = false)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            string query = "SELECT * FROM PERSONAL a where";

            if (!mostrarBorrados)
            {
                query = query + " (a.BORRADO = 'F' OR A.BORRADO IS  null) and ";
            }
            query = query + " a.IOR_CARGO =1 order by a.cod";

            FbCommand oCommand = new FbCommand(query, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            List<PERSONAL> oLista = new List<PERSONAL>();

            while (oReader.Read())
            {
                PERSONAL oPersonal = new PERSONAL();
                oPersonal.OID = (int)oReader["OID"];
                oPersonal.BORRADO = oReader["BORRADO"].ToString();
                oPersonal.CANAL = oReader["CANAL"].ToString();
                oPersonal.CID = DataBase.GetIntFromReader(oReader, "CID");
                oPersonal.COD = DataBase.GetStringFromReader(oReader, "COD");
                oPersonal.DESCRIPCION = DataBase.GetStringFromReader(oReader, "DESCRIPCION");
                oPersonal.DNI = DataBase.GetStringFromReader(oReader, "DNI");
                oPersonal.FECHAN = DataBase.GetDateTimeFromReader(oReader, "FECHAN");
                oPersonal.IOR_CARGO = DataBase.GetIntFromReader(oReader, "IOR_CARGO");
                oPersonal.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                oPersonal.LOGIN = DataBase.GetStringFromReader(oReader, "LOGIN");
                oPersonal.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                oPersonal.NIF = DataBase.GetStringFromReader(oReader, "NIF");
                oPersonal.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                oPersonal.NUMERO = DataBase.GetStringFromReader(oReader, "NUMERO");
                oPersonal.TRATA = DataBase.GetStringFromReader(oReader, "TRATA");
                oPersonal.HORAMOD = oReader["HORAMOD"].ToString();
                oLista.Add(oPersonal);

            }
            oCommand.Dispose();
            oConexion.Close();
            return oLista;
        }

        public static List<PERSONAL> ObtenerCardiologos()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("SELECT * FROM PERSONAL a where a.oid<=0 or ((a.BORRADO ='F' OR A.BORRADO IS  null) and a.IOR_CARGO =20306241 or a.CID=1) order by a.cod", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            List<PERSONAL> oLista = new List<PERSONAL>();

            while (oReader.Read())
            {
                PERSONAL oPersonal = new PERSONAL();
                oPersonal.OID = (int)oReader["OID"];
                oPersonal.BORRADO = oReader["BORRADO"].ToString();
                oPersonal.CANAL = oReader["CANAL"].ToString();
                oPersonal.CID = DataBase.GetIntFromReader(oReader, "CID");
                oPersonal.COD = DataBase.GetStringFromReader(oReader, "COD");
                oPersonal.DESCRIPCION = DataBase.GetStringFromReader(oReader, "DESCRIPCION");
                oPersonal.DNI = DataBase.GetStringFromReader(oReader, "DNI");
                oPersonal.FECHAN = DataBase.GetDateTimeFromReader(oReader, "FECHAN");
                oPersonal.IOR_CARGO = DataBase.GetIntFromReader(oReader, "IOR_CARGO");
                oPersonal.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                oPersonal.LOGIN = DataBase.GetStringFromReader(oReader, "LOGIN");
                oPersonal.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                oPersonal.NIF = DataBase.GetStringFromReader(oReader, "NIF");
                oPersonal.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                oPersonal.NUMERO = DataBase.GetStringFromReader(oReader, "NUMERO");
                oPersonal.TRATA = DataBase.GetStringFromReader(oReader, "TRATA");
                oPersonal.HORAMOD = oReader["HORAMOD"].ToString();
                oLista.Add(oPersonal);

            }
            oCommand.Dispose();
            oConexion.Close();
            return oLista;
        }

        public static List<PERSONAL> ObtenerTecnicos()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("SELECT * FROM PERSONAL a where (a.BORRADO ='F' OR A.BORRADO IS  null) and (a.IOR_CARGO =2 or  a.IOR_CARGO=10577303 or a.oid=0) order by a.cod,a.descripcion", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            List<PERSONAL> oLista = new List<PERSONAL>();

            while (oReader.Read())
            {
                PERSONAL oPersonal = new PERSONAL();
                oPersonal.OID = (int)oReader["OID"];
                oPersonal.BORRADO = oReader["BORRADO"].ToString();
                oPersonal.CANAL = oReader["CANAL"].ToString();
                oPersonal.CID = DataBase.GetIntFromReader(oReader, "CID");
                oPersonal.COD = DataBase.GetStringFromReader(oReader, "COD");
                oPersonal.DESCRIPCION = DataBase.GetStringFromReader(oReader, "DESCRIPCION");
                oPersonal.DNI = DataBase.GetStringFromReader(oReader, "DNI");
                oPersonal.FECHAN = DataBase.GetDateTimeFromReader(oReader, "FECHAN");
                oPersonal.IOR_CARGO = DataBase.GetIntFromReader(oReader, "IOR_CARGO");
                oPersonal.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                oPersonal.LOGIN = DataBase.GetStringFromReader(oReader, "LOGIN");
                oPersonal.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                oPersonal.NIF = DataBase.GetStringFromReader(oReader, "NIF");
                oPersonal.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                oPersonal.NUMERO = DataBase.GetStringFromReader(oReader, "NUMERO");
                oPersonal.TRATA = DataBase.GetStringFromReader(oReader, "TRATA");
                oPersonal.HORAMOD = oReader["HORAMOD"].ToString();
                oLista.Add(oPersonal);

            }
            oCommand.Dispose();
            oConexion.Close();
            return oLista;
        }

        public static List<PERSONAL> ObtenerEstudiantes()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("SELECT * FROM PERSONAL a where (a.BORRADO ='F' OR A.BORRADO IS  null) and a.IOR_CARGO =3 or a.oid=0 order by a.cod", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<PERSONAL> oLista = new List<PERSONAL>();

            while (oReader.Read())
            {
                PERSONAL oPersonal = new PERSONAL();
                oPersonal.OID = (int)oReader["OID"];
                oPersonal.BORRADO = oReader["BORRADO"].ToString();
                oPersonal.CANAL = oReader["CANAL"].ToString();
                oPersonal.CID = DataBase.GetIntFromReader(oReader, "CID");
                oPersonal.COD = DataBase.GetStringFromReader(oReader, "COD");
                oPersonal.DESCRIPCION = DataBase.GetStringFromReader(oReader, "DESCRIPCION");
                oPersonal.DNI = DataBase.GetStringFromReader(oReader, "DNI");
                oPersonal.FECHAN = DataBase.GetDateTimeFromReader(oReader, "FECHAN");
                oPersonal.IOR_CARGO = DataBase.GetIntFromReader(oReader, "IOR_CARGO");
                oPersonal.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                oPersonal.LOGIN = DataBase.GetStringFromReader(oReader, "LOGIN");
                oPersonal.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                oPersonal.NIF = DataBase.GetStringFromReader(oReader, "NIF");
                oPersonal.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                oPersonal.NUMERO = DataBase.GetStringFromReader(oReader, "NUMERO");
                oPersonal.TRATA = DataBase.GetStringFromReader(oReader, "TRATA");
                oPersonal.HORAMOD = oReader["HORAMOD"].ToString();
                oLista.Add(oPersonal);

            }
            oCommand.Dispose();
            oConexion.Close();
            return oLista;
        }




    }
}
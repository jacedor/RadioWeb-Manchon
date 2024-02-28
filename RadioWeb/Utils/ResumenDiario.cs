using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ADPM.Common;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Utils
{
    public class ResumenDiario
    {

        FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

        public ResumenDiario(DateTime fecha, int Mutua = -1, int GAparato = -1, int Aparato = -1, int Centro = -1, int TipoExploracion = -1, int estado=-1, int ior_medico=-1,string informada="", string pagado="",string facturado="", int ior_colegiado = -1 ) 
        {
            if (fecha == DateTime.MinValue) { }
            try
            {
                oConexion.Open();
                string query = "select g.COD_GRUP,d.des_fil,d.cod_fil , Count(e.oid) as total from exploracion e join daparatos d on d.OID=e.IOR_APARATO join gaparatos g on g.OID=e.IOR_GRUPO ";
                query += "where  ( e.IOR_EMPRESA=4) AND (e.fecha ='" + fecha.ToString("yyyy-MM-dd HH:mm:ss") + "') AND (e.ior_grupo+e.ior_aparato>0)  ";

                if (Mutua > 0)
                    query = query + " and e.IOR_ENTIDADPAGADORA='" + Mutua + "'";

                if (GAparato > 0)
                    query = query + " and e.IOR_GRUPO='" + GAparato + "'";

                if (Aparato > 0)
                    query = query + " and e.IOR_APARATO='" + Aparato + "'";

                if (Centro > 0)
                    query = query + " and d.CID=" + Centro;

                if (ior_medico > 0)
                    query = query + " and e.ior_medico=" + ior_medico;

                if (TipoExploracion != -1)
                    query = query + " and e.IOR_GPR=" + TipoExploracion + "";

                if (ior_colegiado >0)
                    query = query + " and e.IOR_COLEGIADO=" + ior_colegiado + "";

                if (estado >= 0)
                {
                    query = query + " and e.ESTADO =" +estado;
                }
                else
                {
                    query = query + " AND (not e.estado in('1','4','5'))";
                }

                if (!String.IsNullOrEmpty(informada) && informada.Trim()!="A")
                {
                    query = query + " and e.INFORMADA=" + informada.Trim().QuotedString();
                }
                if (!String.IsNullOrEmpty(pagado) && pagado.Trim() != "A")
                {
                    query = query + " and e.PAGADO =" + pagado.QuotedString();
                }

                if (!String.IsNullOrEmpty(facturado) && facturado.Trim() != "A")
                {
                    query = query + " and e.FACTURADA =" + facturado.QuotedString();
                }
                query += " group by g.COD_GRUP,d.COD_FIL,d.des_fil  order by g.COD_GRUP,d.COD_FIL";

                FbCommand oCommand = new FbCommand(query, oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();

                this.oGrupoResumen = new List<Grupo>();
                //ITERAMOS POR LOS GRUPOS DE APARATOS Y VAMOS CONTRUYENDO LA ESTRUCTURA DE DATOS
                while (oReader.Read())
                {
                    //BUSCAMOS SI YA HEMOS AGREGADO ESTE APARATO A LA LISTA
                    Grupo oGrupo = oGrupoResumen.Find(delegate (Grupo grupo)
                    {
                        return grupo.Descripcion == DataBase.GetStringFromReader(oReader, "COD_GRUP");
                    });

                    //SINO LA HEMOS AGREGADO LO HACEMOS
                    if (oGrupo == null)
                    {
                        this.oGrupoResumen.Add(new Grupo { CuentaTotal = DataBase.GetIntFromReader(oReader, "TOTAL"), Descripcion = DataBase.GetStringFromReader(oReader, "COD_GRUP") });
                    }
                    else
                    //SI YA ESTA AGREGADA LE SUMAMOS LA COLUMNA DE LA DERECHA (TOTAL DE UN APARATO) PARA TENER EL TOTAL ABSOLUTO
                    {
                        oGrupo.CuentaTotal += DataBase.GetIntFromReader(oReader, "TOTAL");
                    }

                }
                oReader.Close();
                FbCommand oCommand2 = new FbCommand(query, oConexion);
                FbDataReader oReader2 = oCommand.ExecuteReader();


                //UNA VEZ QUE YA TENEMOS EN oGrupoResumen TODOS LOS GRUPOS DE APARATOS CON LOS TOTALES
                //VOLVEMOS A LEER DE LA BASE DE DATOS PERO ESTA VEZ NOS CENTRAREMOS EN LOS APARATOS
                while (oReader2.Read())
                {
                    //BUSCAMOS EL GRUPO
                    Grupo oGrupo = oGrupoResumen.Find(delegate (Grupo grupo)
                    {
                        return grupo.Descripcion == DataBase.GetStringFromReader(oReader2, "COD_GRUP");
                    });


                    //SI LA SUBCOLECCION DE APARATOS DE ESTE GRUPO AUN NO ESTA INSTANCIADA LO HACEMOS
                    if (oGrupo.lAparatos == null)
                    {
                        oGrupo.lAparatos = new List<Aparato>();
                    }
                    //CREAMOS EL APARATO QUE AGREGAMOS A LA COLECCION DE APARATOS DEL GRUPO
                    oGrupo.lAparatos.Add(new Aparato { Cuenta = oReader2["total"].ToString(), Descripcion = oReader2["des_fil"].ToString() });

                }

              
            }
            catch (Exception)
            {

            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)
                    oConexion.Close();
            }


        }
        public List<Grupo> oGrupoResumen;
    }

    public class Grupo {
        public string Descripcion { get; set; }
        public int CuentaTotal { get; set; }
        public List<Aparato> lAparatos;
    }
    public class Aparato
    {
        public string Descripcion { get; set; }
        public string Cuenta { get; set; }
    }
}
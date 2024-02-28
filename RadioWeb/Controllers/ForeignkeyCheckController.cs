using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Repositories;
using RadioWeb.Utils;

namespace RadioWeb.Controllers
{
    public class ForeignkeyCheckController
    {

        public Boolean isBorrable(String tabla, int idRegistro) {

            Boolean borrable = true;

            List<FK_CHECK> fkcheck = Fk_CheckRepositorio.BuscarDependencias(tabla);

            //Recorremos todas las tablas con dependencia
            foreach (var fk in fkcheck)
            {
                String dependencia = fk.DEPENDENCIA;
                String field = fk.FIELD_RELATED;

                //En cada una de las tablas buscamos si hay algun registro asociado al id que intentamos borrar. (FK)
                //Si encontramos algun caso devolvemos false que nos impedirá el borrado.
                if (hayRegistro(dependencia, idRegistro, field)){
                    borrable = false;
                    return borrable;
                }              
            }

            return borrable;        
        }
               

        public Boolean hayRegistro(String dependencia, int idRegistro, String field)
        {
            Boolean registroEncontrado = true;

            
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            
            String queryBuscar = "select count(*) as FOUND from " + dependencia + " where " + field + " = " + idRegistro;
            FbCommand oCommand = new FbCommand(queryBuscar, oConexion);
            try
            {
                FbDataReader oReader = oCommand.ExecuteReader();
                oReader.Read();
                int found = DataBase.GetIntFromReader(oReader, "FOUND");
                if (found == 0) {
                    registroEncontrado = false;
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)
                {

                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }

                }
            }

            return registroEncontrado;
        }



    }

    



}
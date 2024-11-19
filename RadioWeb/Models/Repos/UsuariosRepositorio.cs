using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using System.Security.Principal;
using System.Threading;
using ADPM.Common;

namespace RadioWeb.Models.Repos
{
    public class UsuariosRepositorio
    {
        private static string GetDefaultCartelera(USUARIO oUsuario)
        {

            string carteleraDefault = "Todo";

            if (oUsuario.PERFIL == 76 || oUsuario.PRIVILEGIADO == -1)
            {
                carteleraDefault = "Supe";
            }
            if (oUsuario.ESMEDICO )
            {
                carteleraDefault = "Sani";
            }
            if (oUsuario.PERFIL == 4 || oUsuario.PERFIL == 62)
            {
                carteleraDefault = "ADMI";
            }
            List<int> PerfilesRecepcion = new List<int> { 6, 129, 145, 163, 174 };
            if (PerfilesRecepcion.Contains(oUsuario.PERFIL.Value))
            {
                carteleraDefault = "RECE";
            }

            return carteleraDefault;

        }

        private static int PuedeVerCantidades(USUARIO oUsuario)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);


            string query = "SELECT a.ESTADO FROM UCCADPERM a where a.MODULO='TfrmListaDia' and a.OBJNAME LIKE '%CANTIDAD' and iduser=" + oUsuario.PERFIL;

            int resultado = 1;
            using (var conn = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString))
            {
                using (var cmd = new FbCommand(query, conn))
                {
                    cmd.Connection.Open();
                    using (FbDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            resultado = DataBase.GetIntFromReader(oReader, "ESTADO");
                        }
                    }
                    cmd.Connection.Close();
                }
            }


            return resultado;
        }

        public static bool EsMedico(USUARIO oUsuario)
        {
            string query = "select OID from PERSONAL where IOR_EMPRESA =4 and " +
                "IOR_CARGO=1 and (BORRADO<>'T' or BORRADO is null) and LOGIN=" + oUsuario.LOGIN.QuotedString();
            int resultado = -1;

            using (var conn = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString))
            {
                using (var cmd = new FbCommand(query, conn))
                {
                    cmd.Connection.Open();
                    using (FbDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            resultado = DataBase.GetIntFromReader(oReader, "OID");
                        }
                    }
                    cmd.Connection.Close();
                }
            }

            return resultado > 0;
        }

        public static List<USUARIO> Lista()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);

            string query = "Select U.IDUSER,U.NOME, U.LOGIN,U.EMAIL,U.TIPO, U.PERFIL, U.PRIVILEGIADO,  P.NOME AS PERFILDESCRIPCION from UCCADUSER U LEFT join UCCADUSER  P  on U.PERFIL=P.IDUSER ";
            List<USUARIO> olistaUsuario = new List<USUARIO>();
            using (var conn = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString))
            {
                using (var cmd = new FbCommand(query, conn))
                {
                    cmd.Connection.Open();
                    using (FbDataReader oReader = cmd.ExecuteReader())
                    {

                        while (oReader.Read())
                        {

                            USUARIO UsuarioResult = new USUARIO();
                            UsuarioResult.IDUSER = DataBase.GetIntFromReader(oReader, "IDUSER");
                            UsuarioResult.NOME = DataBase.GetStringFromReader(oReader, "NOME");
                            UsuarioResult.LOGIN = DataBase.GetStringFromReader(oReader, "LOGIN");
                            UsuarioResult.EMAIL = DataBase.GetStringFromReader(oReader, "EMAIL");
                            UsuarioResult.TIPO = DataBase.GetStringFromReader(oReader, "TIPO");
                            UsuarioResult.PERFIL = DataBase.GetIntFromReader(oReader, "PERFIL");
                            UsuarioResult.PRIVILEGIADO = DataBase.GetIntFromReader(oReader, "PRIVILEGIADO");
                            UsuarioResult.DESCPERFILWEB = DataBase.GetStringFromReader(oReader, "PERFILDESCRIPCION");
                            UsuarioResult.CARTELERADEFECTO = GetDefaultCartelera(UsuarioResult);
                            UsuarioResult.VECANTIDADES = PuedeVerCantidades(UsuarioResult);
                            UsuarioResult.ESMEDICO = EsMedico(UsuarioResult);
                            var personal = PersonalRepositorio.Obtener(UsuarioResult.LOGIN);
                            if (personal.OID > 0)
                            {
                                UsuarioResult.IOR_PERSONAL = personal.OID;
                                UsuarioResult.PERSONAL = personal;
                            }
                            olistaUsuario.Add(UsuarioResult);
                        }
                    }
                    cmd.Connection.Close();
                }
            }



            return olistaUsuario;

        }


        public static USUARIO Obtener(string _username)
        {
            FbConnection oConexion2 = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);
            USUARIO UsuarioResult = new USUARIO();

            // Lo Ideal sería que entramos con el password encriptado
            string query = "Select u.*,P.CENTROASOCIADO,  P.NOME AS PERFILDESCRIPCION " +
                "from UCCADUSER U LEFT join UCCADUSER  P  on U.PERFIL=P.IDUSER where U.Login=@p1";

            //FbDataReader oReader = DataBase.EjecutarQuery(oConexion, query);
            using (var conn =
                new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"]
                .ConnectionString))
            {
                using (var cmd = new FbCommand(query, conn))
                {
                    cmd.Connection.Open();
                    cmd.Parameters.Add("p1", FbDbType.VarChar, _username.Length).Value = _username;
                    using (FbDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            UsuarioResult.IDUSER = DataBase.GetIntFromReader(oReader, "IDUSER");
                            UsuarioResult.NOME = DataBase.GetStringFromReader(oReader, "NOME");
                            UsuarioResult.LOGIN = DataBase.GetStringFromReader(oReader, "LOGIN");
                            UsuarioResult.EMAIL = DataBase.GetStringFromReader(oReader, "EMAIL");
                            UsuarioResult.ENCRIPTADO = DataBase.GetStringFromReader(oReader, "ENCRIPTADO");

                            UsuarioResult.TIPO = DataBase.GetStringFromReader(oReader, "TIPO");
                            UsuarioResult.PERFIL = DataBase.GetIntFromReader(oReader, "PERFIL");
                            UsuarioResult.PRIVILEGIADO = DataBase.GetIntFromReader(oReader, "PRIVILEGIADO");
                            UsuarioResult.DESCPERFILWEB = DataBase.GetStringFromReader(oReader, "PERFILDESCRIPCION");
                            UsuarioResult.CARTELERADEFECTO = GetDefaultCartelera(UsuarioResult);
                            UsuarioResult.VECANTIDADES = PuedeVerCantidades(UsuarioResult);
                            UsuarioResult.ESMEDICO = EsMedico(UsuarioResult);
                            UsuarioResult.IOR_COLEGIADO = DataBase.GetIntFromReader(oReader, "IOR_COLEGIADO");
                            UsuarioResult.IOR_ENTIDADPAGADORA = DataBase.GetIntFromReader(oReader, "IOR_ENTIDADPAGADORA");
                            UsuarioResult.CREARPETICIONES = DataBase.GetStringFromReader(oReader, "CREARPETICIONES");
                            UsuarioResult.PERSONAL = PersonalRepositorio.Obtener(UsuarioResult.LOGIN);
                            UsuarioResult.CITACIONONLINE = DataBase.GetStringFromReader(oReader, "CITACIONONLINE");
                            UsuarioResult.IOR_CENTROEXTERNO = DataBase.GetIntFromReader(oReader, "IOR_CENTROEXTERNO");

                            if (UsuarioResult.IOR_COLEGIADO > 0)
                            {
                                COLEGIADOS oColegiado = ColegiadoRepositorio.Obtener(UsuarioResult.IOR_COLEGIADO.Value);
                                UsuarioResult.PERSONAL = new PERSONAL { OID = oColegiado.OID, NOMBRE = oColegiado.NOMBRE };
                            }
                            if (UsuarioResult.IOR_ENTIDADPAGADORA > 0)
                            {
                                MUTUAS oMutua = MutuasRepositorio.Obtener(UsuarioResult.IOR_ENTIDADPAGADORA.Value);
                                UsuarioResult.MUTUA = new MUTUAS { OID = oMutua.OID, NOMBRE = oMutua.NOMBRE };
                            }
                            if (UsuarioResult.IOR_CENTROEXTERNO > 0)
                            {
                                CENTROSEXTERNOS oCentro = CentrosExternosRepositorio.Obtener(UsuarioResult.IOR_CENTROEXTERNO.Value);
                                UsuarioResult.CENTROEXTERNO = new CENTROSEXTERNOS { OID = oCentro.OID, NOMBRE = oCentro.NOMBRE };
                            }
                            UsuarioResult.CENTROASOCIADO = DataBase.GetIntFromReader(oReader, "CENTROASOCIADO");
                            UsuarioResult.IOR_ENTIDADPAGADORA = DataBase.GetIntFromReader(oReader, "IOR_ENTIDADPAGADORA");
                           
                            var principal = new GenericPrincipal(new GenericIdentity(UsuarioResult.LOGIN),
                                new string[] { UsuarioResult.DESCPERFILWEB });
                            Thread.CurrentPrincipal = principal;

                        }

                    }
                    cmd.Connection.Close();
                }
            }
            return UsuarioResult;


        }

        /// <summary>
        /// Checks if user with given password exists in the database
        /// </summary>
        /// <param name="_username">User name</param>
        /// <param name="_password">User password</param>
        /// <returns>True if user exist and password is correct</returns>
        public static USUARIO IsValid(string _username, string _password)
        {
            //     FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);
            FbConnection oConexion2 = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);
            USUARIO UsuarioResult = new USUARIO();
            // Lo Ideal sería que entramos con el password encriptado
            using (UsersDBContext context = new UsersDBContext())
            {
                UsuarioResult = context.UCCADUSER.SingleOrDefault(u => u.LOGIN.ToUpper() == _username.ToUpper());
                if (UsuarioResult != null)
                {

                    //Si el usuario está bloqueado salimos.
                    if (UsuarioResult.BLOQUEADO == "T")
                    {
                        return null;
                    }

                    string passwordWeb = "sin definir@@";
                    if (UsuarioResult.ENCRIPTADO == "T")
                    {
                         passwordWeb= SHA1.Encode(_password);
                         
                    }
                    if (UsuarioResult.PASSWORDWEB != passwordWeb)
                    {
                        //si no hemos conseguido entrar con el passwordWeb , 
                        //intentamos entrar con el campo password que no esta encriptado
                        UsuarioResult = context.UCCADUSER
                            .SingleOrDefault(u => u.LOGIN == _username && u.PASSWORD==_password);
                    }

                    if (UsuarioResult!=null)
                    {
                        //Reiniciamos el contador de bloqueos.
                        UsuarioResult.LOGIN_ATTEMPTS = 0;
                        context.SaveChanges();

                        var perfil = context.UCCADUSER.SingleOrDefault(u => u.IDUSER == UsuarioResult.PERFIL);
                        if (perfil != null)
                        {
                            UsuarioResult.DESCPERFILWEB = perfil.NOME;
                        }
                        UsuarioResult.CARTELERADEFECTO = GetDefaultCartelera(UsuarioResult);
                        UsuarioResult.VECANTIDADES = PuedeVerCantidades(UsuarioResult);
                        UsuarioResult.ESMEDICO = EsMedico(UsuarioResult);
                    
                        UsuarioResult.PERSONAL = PersonalRepositorio.Obtener(UsuarioResult.LOGIN);

                        if (UsuarioResult.IOR_COLEGIADO > 0)
                        {
                            COLEGIADOS oColegiado = ColegiadoRepositorio.Obtener(UsuarioResult.IOR_COLEGIADO.Value);
                            UsuarioResult.PERSONAL = new PERSONAL { OID = oColegiado.OID, NOMBRE = oColegiado.NOMBRE };
                        }
                        var mainmenus = (from menuper in context.UCCADMENUPERM
                                         join mnu in context.UCCADMENU on menuper.IDMENU equals mnu.ID
                                         where menuper.IDUSER == UsuarioResult.PERFIL && mnu.IDPARENT == -1
                                         orderby mnu.ORDEN ascending
                                         select new
                                         {
                                             ID = mnu.ID,
                                             DESCRIPCION = mnu.DESCRIPCION,
                                             IDPARENT = mnu.IDPARENT,
                                             IDUSER = mnu.IDUSER,
                                             ORDEN = mnu.ORDEN,
                                             URL = mnu.URL,
                                             ICONO = mnu.ICONO
                                         }).ToList();

                        foreach (var item in mainmenus)
                        {
                            UsuarioResult.MainMenus.Add(new UCCADMENU
                            {
                                ID = item.ID,
                                DESCRIPCION = item.DESCRIPCION,
                                ICONO = item.ICONO,
                                ORDEN = item.ORDEN
                            });
                        }
                        var SubMenus = (from menuper in context.UCCADMENUPERM
                                        join mnu in context.UCCADMENU on menuper.IDMENU equals mnu.ID
                                        where menuper.IDUSER == UsuarioResult.PERFIL && mnu.IDPARENT != -1
                                        orderby mnu.ORDEN ascending
                                        select new
                                        {
                                            ID = mnu.ID,
                                            DESCRIPCION = mnu.DESCRIPCION,
                                            URL = mnu.URL,
                                            ICONO = mnu.ICONO,
                                            IDPARENT = mnu.IDPARENT,
                                            ORDEN = mnu.ORDEN,
                                            IDUSER = mnu.IDUSER
                                        }).ToList();

                        foreach (var item in SubMenus)
                        {
                            UsuarioResult.SubMenus.Add(new UCCADMENU
                            {
                                URL = item.URL,
                                DESCRIPCION = item.DESCRIPCION,
                                ICONO = item.ICONO,
                                ORDEN = item.ORDEN,
                                IDPARENT = item.IDPARENT
                            });
                        }
                        var principal = new GenericPrincipal(new GenericIdentity(UsuarioResult.LOGIN),
                            new string[] { UsuarioResult.DESCPERFILWEB });
                        Thread.CurrentPrincipal = principal;
                    }
                }
            }
           




            //if (UsuarioResult.IDUSER == 0)
            //{
            //    string query2 = "Select U.ENCRIPTADO, U.IDUSER,U.NOME, U.LOGIN,U.EMAIL,U.TIPO, U.PERFIL,U.PRIVILEGIADO,P.CENTROASOCIADO,  P.NOME AS PERFILDESCRIPCION from UCCADUSER "
            //        + "U LEFT join UCCADUSER  P  on U.PERFIL=P.IDUSER  where U.Login=@p1  AND U.PASSWORD=@p2";

            //    oConexion2.Open();
            //    using (var cmd2 = new FbCommand(query2, oConexion2))
            //    {
            //        cmd2.Parameters.Add("@p1", FbDbType.VarChar, _username.Length).Value = _username;
            //        cmd2.Parameters.Add("@p2", FbDbType.VarChar, _password.Length).Value = _password;
            //        using (FbDataReader oReader2 = cmd2.ExecuteReader())
            //        {
            //            while (oReader2.Read())
            //            {
            //                UsuarioResult.IDUSER = DataBase.GetIntFromReader(oReader2, "IDUSER");
            //                UsuarioResult.NOME = DataBase.GetStringFromReader(oReader2, "NOME");
            //                UsuarioResult.LOGIN = DataBase.GetStringFromReader(oReader2, "LOGIN");
            //                UsuarioResult.EMAIL = DataBase.GetStringFromReader(oReader2, "EMAIL");
            //                UsuarioResult.TIPO = DataBase.GetStringFromReader(oReader2, "TIPO");
            //                UsuarioResult.ENCRIPTADO = DataBase.GetStringFromReader(oReader2, "ENCRIPTADO");
            //                UsuarioResult.PERFIL = DataBase.GetIntFromReader(oReader2, "PERFIL");
            //                UsuarioResult.PRIVILEGIADO = DataBase.GetIntFromReader(oReader2, "PRIVILEGIADO");
            //                UsuarioResult.DESCPERFILWEB = DataBase.GetStringFromReader(oReader2, "PERFILDESCRIPCION");
            //                UsuarioResult.CARTELERADEFECTO = GetDefaultCartelera(UsuarioResult);
            //                UsuarioResult.VECANTIDADES = PuedeVerCantidades(UsuarioResult);
            //                UsuarioResult.ESMEDICO = EsMedico(UsuarioResult);
            //                UsuarioResult.PERSONAL = PersonalRepositorio.Obtener(UsuarioResult.LOGIN);
            //                UsuarioResult.CENTROASOCIADO = DataBase.GetIntFromReader(oReader2, "CENTROASOCIADO");
            //                var principal = new GenericPrincipal(new GenericIdentity(UsuarioResult.LOGIN),
            //                                    new string[] { UsuarioResult.DESCPERFILWEB });
            //                Thread.CurrentPrincipal = principal;
            //            }
            //        }
            //    }

            //}
            //oConexion2.Close();
            //oConexion2.Dispose();
            return UsuarioResult;

        }
    }
}
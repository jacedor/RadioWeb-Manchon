using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Repos
{
    public class AuthorizeRepositorio
    {
        public static bool Autorizar(USUARIO currentUser, string Action)
        {
            bool result = false;

            switch (Action)
            {
                case "/Informe/Create":
                    result= UsuariosRepositorio.EsMedico(currentUser);
                    //Supervisión, Medico Informante y medico
                    if (currentUser.PERFIL == 129 || currentUser.PERFIL == 76 || currentUser.PERFIL==2 || currentUser.PERFIL == 10 || currentUser.PRIVILEGIADO == -1)
                    {
                        result = true;
                    }
                    break;
                case "/Informe/Edit":
                    result = UsuariosRepositorio.EsMedico(currentUser);
                    //Supervisión, Medico Informante y medico
                    if (currentUser.PERFIL == 129 || currentUser.PERFIL==76 || currentUser.PERFIL == 2 || currentUser.PERFIL == 10 || currentUser.PRIVILEGIADO == -1)
                    {
                        result = true;
                    }
                    break;
                case "/P_Informe/Edit":
                    result = UsuariosRepositorio.EsMedico(currentUser);
                    //Medico Informante y medico
                    if (currentUser.PERFIL == 2 || currentUser.PERFIL == 10 || currentUser.PRIVILEGIADO ==-1)
                    {
                        result = true;
                    }
                    break;
                default:
                    return false;
                    break;
            }

            return result;

        }
    }
}
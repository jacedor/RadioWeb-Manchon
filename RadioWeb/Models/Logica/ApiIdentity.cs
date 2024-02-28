using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace RadioWeb.Models.Logica
{
    public class ApiIdentity : IIdentity
    {
        public USUARIO User
        {
            get;
            private set;
        }
        public ApiIdentity(USUARIO user)
        {
            if (user == null) throw new ArgumentNullException("user");
            this.User = user;
        }

        public string Name
        {
            get
            {
                return this.User.NOME;
            }
        }

        public string AuthenticationType
        {
            get
            {
                return "Basic";
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return true;
            }
        }
    }
}
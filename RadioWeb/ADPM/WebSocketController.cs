using Microsoft.Web.WebSockets;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace RadioWeb.ADPM
{
   
    public class WebSocketController : ApiController
    {
        private static WebSocketCollection connections = new WebSocketCollection();

        public HttpResponseMessage Get(string login)
        {
            if (HttpContext.Current.IsWebSocketRequest)
            {
                var noteHandler = new ListaDiaSocketHandler(login);
                HttpContext.Current.AcceptWebSocketRequest(noteHandler);
            }

            return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
        }


        internal class ListaDiaSocketHandler : WebSocketHandler
        {
            public string _login { get; set; }
            public ListaDiaSocketHandler(string login)
            {
                _login = login;
            }

            public override void OnClose()
            {
               
                foreach (USUARIO item in MvcApplication.UsuariosConectados.ToList())
                {
                    if (item.LOGIN==this._login)
                    {
                        MvcApplication.UsuariosConectados.Remove(item);
                    }
                }
                connections.Remove(this);
            }

            public override void OnOpen()
            {
                connections.Add(this);
                MvcApplication.UsuariosConectados.Add(UsuariosRepositorio.Obtener(this._login));
            }

            public override void OnError()
            {
                connections.Remove(this);

            }

            public override void OnMessage(byte[] message)
            {
                base.OnMessage(message);
            }

            public override void OnMessage(string message)
            {
                ListaDiaSocketAction socketAction = new JavaScriptSerializer().Deserialize<ListaDiaSocketAction>(message);
     
                              
                string returnAction = new JavaScriptSerializer().Serialize(socketAction);

                foreach (var connection in connections)
                {
                    if (((ListaDiaSocketHandler)connection)._login != _login)
                        connection.Send(returnAction);
                    
                }


            }
        }
    }

   
}

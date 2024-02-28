using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Generic;


namespace RadioWeb.Models.Chat
{

    [HubName("chat")]
    public class ChatHub : Hub
    {
        private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();

        public void SendChatMessage(string who, string message)
        {
            string name = Context.User.Identity.Name;

            foreach (var connectionId in _connections.GetConnections(who))
            {
                Clients.Client(connectionId).addChatMessage(name + ": " + message);
            }
        }


        //public List< string> GetConnectedUsers(string who, string message)
        //{
        //    List<string> result = new List<string>();

        //    foreach (var connectionId in _connections.GetConnections))
        //    {
        //        Clients.Client(connectionId).addChatMessage(name + ": " + message);
        //    }
        //}

        public void AddConnectedUser(string name)
        {
            _connections.Add(name, Context.ConnectionId);
            Clients.All.updateLabelCount(_connections.Count);
        }

        public void ActualizarEstadoExploracion(int oid)        
        {
            Clients.Others.updateExploracionStatus(oid);
          
        }

        public void actualizarExploracionAnadida(int oid, string hhora)
        {
            Clients.Others.updateExploracionAdded(oid, hhora);

        }



        public override Task OnDisconnected(bool stopCalled)
        {
            string userName = Context.User.Identity.Name;
            _connections.Remove(userName, Context.ConnectionId);
            Clients.All.updateLabelCount(_connections.Count);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {


            string name = Context.User.Identity.Name;


            if (!_connections.GetConnections(name).ToString().Contains(Context.ConnectionId))
            {
                _connections.Add(name, Context.ConnectionId);
            }

            return base.OnReconnected();
        }
    }
}

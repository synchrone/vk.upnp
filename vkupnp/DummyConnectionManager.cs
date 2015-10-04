// 
// DummyConnectionManager.cs
//  
using System;
using Mono.Upnp.Dcp.MediaServer1.ConnectionManager1;

namespace Vk.Music.Upnp.ConsoleServer
{
    public class DummyConnectionManager : ConnectionManager
    {
        protected override string CurrentConnectionIDs {
            get {
                throw new NotImplementedException ();
            }
        }
        
        protected override void GetProtocolInfoCore (out string source, out string sink)
        {
			source = MediaTypes.Mp3.ToString();
			sink = String.Empty;
        }


        protected override void GetCurrentConnectionInfoCore (int connectionId, out int resId, out int avTransportId, out string protocolInfo, out string peerConnectionManager, out int peerConnectionId, out Mono.Upnp.Dcp.MediaServer1.ConnectionManager1.Direction direction, out Mono.Upnp.Dcp.MediaServer1.ConnectionManager1.ConnectionStatus status)
        {
            throw new NotImplementedException ();
        }
    }
}

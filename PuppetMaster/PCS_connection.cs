using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels.Tcp;
using PCS_service;

namespace PuppetMaster
{
    internal class PCS_connection
    {
        public string url;
        public PCS pcs;

        public PCS_connection(string url)
        {
            this.url = url;
            pcs = Activator.GetObject(typeof(PCS), url) as PCS;
        }
    }
}
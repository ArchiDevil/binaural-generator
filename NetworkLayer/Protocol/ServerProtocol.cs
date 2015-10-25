using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLayer
{
    public class ServerProtocol
    {
        IClientConnectionInterface connectionInterface = null;

        ServerProtocol(IClientConnectionInterface connectionInterface)
        {
            this.connectionInterface = connectionInterface;
        }
    }
}

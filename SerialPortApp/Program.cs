using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SensorsLayer;

namespace SerialPortApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SensorsCollector collector = new SensorsCollector();
            List<string> ports = collector.GetAllPorts();
            foreach(var port in ports)
                Console.WriteLine(port);


        }
    }
}

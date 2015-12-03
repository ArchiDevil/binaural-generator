using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorsLayer
{
    public class SensorsCollector
    {
        public SensorsCollector()
        {
        }

        public List<string> GetAllPorts()
        {
            List<string> allPorts = new List<string>();
            foreach (string portName in SerialPort.GetPortNames())
                allPorts.Add(portName);

            return allPorts;
        }

        private void CollectCurrentValues()
        {

        }
    }
}

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

        public bool FindDevice()
        {
            return false;
        }

        private List<string> GetAllPorts()
        {
            List<string> allPorts = new List<string>();
            foreach (string portName in SerialPort.GetPortNames())
                allPorts.Add(portName);

            return allPorts;
        }

        private void SelectPort(string portName)
        {
            SerialPort myPort = new SerialPort(portName);
            if (!myPort.IsOpen)
                myPort.Open();

            myPort.Close();
        }

        private void CollectCurrentValues()
        {

        }
    }
}

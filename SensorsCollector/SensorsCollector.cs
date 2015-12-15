using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensorsLayer
{
    public class SensorsDataEventArgs
    {
        public double temperatureValue = 0.0;
        public double skinResistanceValue = 0.0;
        public double motionValue = 0.0;
        public double pulseValue = 0.0;
    }

    public class SensorsCollector
    {
        SerialPort connectedDevice = null;
        ManualResetEvent stopEvent = new ManualResetEvent(false);

        public SensorsCollector()
        {
        }

        public bool ConnectToDevice()
        {
            // check all ports
            List<string> allPorts = GetAllPorts();

            foreach (string portName in allPorts)
            {
                SerialPort port = new SerialPort(portName);
                if (!port.IsOpen)
                    port.Open();

                port.BaudRate = 9600;

                const int bufferSize = 6;
                byte[] buffer = new byte[bufferSize];
                buffer[0] = 4;
                buffer[1] = 8;
                buffer[2] = 15;
                buffer[3] = 16;
                buffer[4] = 23;
                buffer[5] = 42;

                port.Write(buffer, 0, bufferSize);

                // waiting 100 ms for each device to make sure it request processed correctly
                Thread.Sleep(100);

                if (port.BytesToRead == 0)
                    continue;

                byte[] readBuffer = new byte[port.BytesToRead];
                // read ALL data
                int count = port.Read(readBuffer, 0, port.BytesToRead);
                if (count != port.BytesToRead)
                    throw new SystemException("Internal error");

                if (buffer[0] != 4 ||
                    buffer[1] != 8 ||
                    buffer[2] != 15 ||
                    buffer[3] != 16 ||
                    buffer[4] != 23 ||
                    buffer[5] != 42)
                    continue;

                connectedDevice = port;
                break;
            }

            if (connectedDevice == null)
                return false;

            // select responded device
            byte[] commandBuffer = Encoding.ASCII.GetBytes("START");
            connectedDevice.Write(commandBuffer, 0, commandBuffer.Length);

            //start collector
            Thread worker = new Thread(CollectHandler);
            worker.Start();

            return true;
        }

        public event SensorsDataReceiveHandler SensorsDataReceived = delegate
        { };

        public delegate void SensorsDataReceiveHandler(SensorsDataEventArgs e);

        private void CollectHandler()
        {
            while (true)
            {
                lock (stopEvent)
                {
                    if (stopEvent.WaitOne(0))
                        break;
                }
                // collect data from device
                SensorsDataEventArgs e = new SensorsDataEventArgs();
                if (!CollectCurrentValues(ref e))
                    break;

                // call the event
                SensorsDataReceived(e);
            }

            lock (connectedDevice)
            {
                byte[] commandBuffer = Encoding.ASCII.GetBytes("STOP");
                connectedDevice.Write(commandBuffer, 0, commandBuffer.Length);
                connectedDevice.Close();
                connectedDevice = null;
            }
        }

        private List<string> GetAllPorts()
        {
            List<string> allPorts = new List<string>();
            foreach (string portName in SerialPort.GetPortNames())
                allPorts.Add(portName);

            return allPorts;
        }

        private bool CollectCurrentValues(ref SensorsDataEventArgs e)
        {
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensorsLayer
{
    public class ConnectedEventArgs
    {
        public string portName = null;
        public ConnectedEventArgs(string portName) { this.portName = portName; }
    }

    public class DisconnectedEventArgs
    {
        public string portName = null;
        public DisconnectedEventArgs(string portName) { this.portName = portName; }
    }

    public class SensorsDataEventArgs
    {
        public double temperatureValue = 0.0;
        public double skinResistanceValue = 0.0;
        public double motionValue = 0.0;
        public double pulseValue = 0.0;
    }

    internal enum BaudRates
    {
        Rate_4800 = 4800,
        Rate_9600 = 9600,
        Rate_14400 = 14400,
        Rate_19200 = 19200,
        Rate_28800 = 28800,
        Rate_38400 = 38400,
        Rate_57600 = 57600,
        Rate_115200 = 115200
    }

    public class SensorsCollector
    {
        private SerialPort _devicePort = null;
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private byte[] _receivingBuffer = new byte[4096];
        private int _offset = 0;

        public delegate void DeviceConnectedHandler(object sender, ConnectedEventArgs e);
        public delegate void DeviceDisconnectedHandler(object sender, DisconnectedEventArgs e);

        public event DeviceConnectedHandler DeviceConnected = delegate { };
        public event DeviceDisconnectedHandler DeviceDisconnected = delegate { };

        public async void StartDeviceExploring()
        {
            Task t = new Task(() =>
            {
                // check all ports
                if (!FindDevice() && _devicePort == null)
                    return;

                // select responded device
                byte[] commandBuffer = Encoding.ASCII.GetBytes("START");
                _devicePort.Write(commandBuffer, 0, commandBuffer.Length);

                DeviceConnected(this, new ConnectedEventArgs(_devicePort.PortName));

                //start collector
                Thread worker = new Thread(CollectHandler);
                worker.Start();
            });
            await t;
        }

        private bool FindDevice()
        {
            List<string> allPorts = GetAllPortsList();

            foreach (string portName in allPorts)
                if (CheckDevice(portName))
                    return true;

            return false;
        }

        private bool CheckDevice(string portName)
        {
            SerialPort port = new SerialPort(portName);
            if (!port.IsOpen)
                port.Open();

            port.BaudRate = (int)BaudRates.Rate_9600;

            byte[] buffer = { 4, 8, 15, 16, 23, 42 };
            port.Write(buffer, 0, buffer.Length);

            // waiting 100 ms for each device to make sure it request processed correctly
            Thread.Sleep(100);

            if (port.BytesToRead == 0)
                return false;

            byte[] readBuffer = new byte[port.BytesToRead];
            // read ALL data
            int count = port.Read(readBuffer, 0, port.BytesToRead);
            if (count != port.BytesToRead)
                return false;

            if (readBuffer.SequenceEqual(buffer))
                return false;

            _devicePort = port;
            return true;
        }

        public event SensorsDataReceiveHandler SensorsDataReceived = delegate { };
        public delegate void SensorsDataReceiveHandler(SensorsDataEventArgs e);

        private void CollectHandler()
        {
            while (true)
            {
                lock (_stopEvent)
                {
                    if (_stopEvent.WaitOne(0))
                        break;
                }

                SensorsDataEventArgs e = new SensorsDataEventArgs();
                if (!CollectCurrentValues(ref e))
                    break;

                SensorsDataReceived(e);
            }

            lock (_devicePort)
            {
                DeviceDisconnected(this, new DisconnectedEventArgs(_devicePort.PortName));

                byte[] commandBuffer = Encoding.ASCII.GetBytes("STOP");
                _devicePort.Write(commandBuffer, 0, commandBuffer.Length);
                _devicePort.Close();
                _devicePort = null;
            }
        }

        private List<string> GetAllPortsList()
        {
            List<string> allPorts = new List<string>();
            foreach (string portName in SerialPort.GetPortNames())
                allPorts.Add(portName);

            return allPorts;
        }

        private bool CollectCurrentValues(ref SensorsDataEventArgs e)
        {
            int bytesToRead = _devicePort.BytesToRead;
            int dataSize = 32;
            if (_offset > dataSize)
            {
                e.motionValue = BitConverter.ToDouble(_receivingBuffer, 0);
                e.pulseValue = BitConverter.ToDouble(_receivingBuffer, 8);
                e.skinResistanceValue = BitConverter.ToDouble(_receivingBuffer, 16);
                e.temperatureValue = BitConverter.ToDouble(_receivingBuffer, 24);

                _offset -= dataSize;
                _receivingBuffer.Skip(dataSize).Take(_receivingBuffer.Length - dataSize);
                return true;
            }

            int readCount = _devicePort.Read(_receivingBuffer, _offset, bytesToRead);
            if (readCount != bytesToRead)
                return false;
            return true;
        }
    }
}

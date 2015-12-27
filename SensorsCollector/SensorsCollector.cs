using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace SensorsLayer
{
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
        SerialPort connectedDevice = null;
        ManualResetEvent stopEvent = new ManualResetEvent(false);
        byte[] receivingBuffer = new byte[4096];
        int offset = 0;

        public bool ConnectToDevice()
        {
            // check all ports
            if (!FindDevice() && connectedDevice == null)
                return false;

            // select responded device
            byte[] commandBuffer = Encoding.ASCII.GetBytes("START");
            connectedDevice.Write(commandBuffer, 0, commandBuffer.Length);

            //start collector
            Thread worker = new Thread(CollectHandler);
            worker.Start();

            return true;
        }

        private bool FindDevice()
        {
            List<string> allPorts = GetAllPortsList();

            foreach (string portName in allPorts)
            {
                SerialPort port = new SerialPort(portName);
                if (!port.IsOpen)
                    port.Open();

                port.BaudRate = (int)BaudRates.Rate_9600;

                const int bufferSize = 6;
                byte[] buffer = { 4, 8, 15, 16, 23, 42 };
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

                if (readBuffer.SequenceEqual(buffer))
                    continue;

                connectedDevice = port;
                return true;
            }

            return false;
        }

        public event SensorsDataReceiveHandler SensorsDataReceived = delegate { };
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

                SensorsDataEventArgs e = new SensorsDataEventArgs();
                if (!CollectCurrentValues(ref e))
                    break;

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

        private List<string> GetAllPortsList()
        {
            List<string> allPorts = new List<string>();
            foreach (string portName in SerialPort.GetPortNames())
                allPorts.Add(portName);

            return allPorts;
        }

        private bool CollectCurrentValues(ref SensorsDataEventArgs e)
        {
            int bytesToRead = connectedDevice.BytesToRead;
            int dataSize = 32;
            if (offset > dataSize)
            {
                e.motionValue = BitConverter.ToDouble(receivingBuffer, 0);
                e.pulseValue = BitConverter.ToDouble(receivingBuffer, 8);
                e.skinResistanceValue = BitConverter.ToDouble(receivingBuffer, 16);
                e.temperatureValue = BitConverter.ToDouble(receivingBuffer, 24);

                offset -= dataSize;
                receivingBuffer.Skip(dataSize).Take(receivingBuffer.Length - dataSize);
                return true;
            }
            try
            {
                int readCount = connectedDevice.Read(receivingBuffer, offset, bytesToRead);
                if (readCount != bytesToRead)
                    throw new Exception();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

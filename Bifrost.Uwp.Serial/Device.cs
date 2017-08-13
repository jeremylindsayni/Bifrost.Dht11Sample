using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace Bifrost.Uwp.Serial
{
    public class Device : IDevice
    {
        public Device(
            int writeTimeOut = 1000, 
            int readTimeout = 1000, 
            uint baudRate = 9600, 
            SerialParity parity = SerialParity.None, 
            SerialStopBitCount stopBits = SerialStopBitCount.One,
            ushort dataBits = 8, 
            SerialHandshake handshake = SerialHandshake.None)
        {
            this.writeTimeOutTimeSpan = TimeSpan.FromMilliseconds(writeTimeOut);
            this.readTimeOutTimeSpan = TimeSpan.FromMilliseconds(readTimeout);
            this.baudRate = baudRate;
            this.parity = parity;
            this.stopBits = stopBits;
            this.dataBits = dataBits;
            this.handshake = handshake;
        }

        public DeviceInformation SerialDeviceInformation { get; set; }

        public CancellationTokenSource ReadCancellationTokenSource { get; set; }

        private SerialDevice serialPort;
        private TimeSpan writeTimeOutTimeSpan;
        private TimeSpan readTimeOutTimeSpan;
        private uint baudRate;
        private SerialParity parity;
        private SerialStopBitCount stopBits;
        private ushort dataBits;
        private SerialHandshake handshake;

        public async Task ReadAndProcessSerialDataAsync(Func<string, Task> processSerialData)
        {
            await ConfigureSerialDeviceAsync();

            // Create cancellation token object to close I/O operations when closing the device
            ReadCancellationTokenSource = new CancellationTokenSource();

            DataReader dataReaderObject = null;

            try
            {
                dataReaderObject = new DataReader(serialPort.InputStream);

                // keep reading the serial input
                while (true)
                {
                    UInt32 ReadBufferLength = 1024;

                    // If task cancellation was requested, comply
                    ReadCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
                    dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

                    using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ReadCancellationTokenSource.Token))
                    {
                        // Create a task object to wait for data on the serialPort.InputStream
                        // Launch the task and wait
                        var bytesRead = await dataReaderObject.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);

                        if (bytesRead > 0)
                        {
                            // Process output
                            await processSerialData(dataReaderObject.ReadString(bytesRead));
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                CloseDevice();
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        public void CancelReading()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        private async Task ConfigureSerialDeviceAsync()
        {
            serialPort = await SerialDevice.FromIdAsync(SerialDeviceInformation.Id);
            if (serialPort == null)
            {
                throw new Exception($"The device '{SerialDeviceInformation.Id}' was not found. Please check that the UWP manifest has the serial communication capability enabled.");
            }

            // Configure serial settings
            serialPort.WriteTimeout = this.writeTimeOutTimeSpan;
            serialPort.ReadTimeout = this.readTimeOutTimeSpan;
            serialPort.BaudRate = this.baudRate;
            serialPort.Parity = this.parity;
            serialPort.StopBits = this.stopBits;
            serialPort.DataBits = this.dataBits;
            serialPort.Handshake = this.handshake;
        }

        private void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }

            serialPort = null;
        }
    }
}

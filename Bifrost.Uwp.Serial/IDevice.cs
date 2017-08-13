using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace Bifrost.Uwp.Serial
{
    public interface IDevice
    {
        CancellationTokenSource ReadCancellationTokenSource { get; set; }

        DeviceInformation SerialDeviceInformation { get; set; }

        void CancelReading();

        Task ReadAndProcessSerialDataAsync(Func<string, Task> processSerialData);
    }
}
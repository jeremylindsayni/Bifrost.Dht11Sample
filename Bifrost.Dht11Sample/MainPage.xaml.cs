using Bifrost.Sensors;
using Bifrost.Uwp.Serial;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Bifrost.Dht11Sample
{
    public sealed partial class MainPage : Page
    {
        private IDevice serialDevice = new Device();

        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var aqs = SerialDevice.GetDeviceSelector();
            var arduinoWatcher = DeviceInformation.CreateWatcher(aqs);

            // Subscribe to device events
            arduinoWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(OnDeviceAdded);
            arduinoWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(OnDeviceRemoved);

            arduinoWatcher.Start();
        }

        private async void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            serialDevice.CancelReading();

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => pageTitle.Text = "Device removed");
        }

        private async void OnDeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            if (args.IsUsbDevice())
            {
                serialDevice.SerialDeviceInformation = args;
                await serialDevice.ReadAndProcessSerialDataAsync(ProcessSerialOutput);
            }
        }

        private async Task ProcessSerialOutput(string serialOutput)
        {
            try
            {
                var dht11 = JsonConvert.DeserializeObject<Dht11>(serialOutput);

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => pageTitle.Text = dht11.Temperature.ToString());
            }
            catch (JsonReaderException jsonReaderException)
            {
                Debug.WriteLine($"Invalid JSON - this may be among first serial messages. {jsonReaderException.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}

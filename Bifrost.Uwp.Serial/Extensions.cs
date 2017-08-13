using Windows.Devices.Enumeration;
using System;

namespace Bifrost.Uwp.Serial
{
    public static class Extensions
    {
        public static DeviceMetaData ParseIdentifier(this DeviceInformation deviceInformation)
        {
            var deviceIdentifier = deviceInformation.Id;

            if (!string.IsNullOrEmpty(deviceIdentifier))
            {
                var deviceIdParts = deviceIdentifier.Split('#');

                if (deviceIdParts.Length == 4)
                {
                    return new DeviceMetaData
                    {
                        DeviceType = deviceIdParts[0],
                        VendorAndProductId = deviceIdParts[1],
                        Name = deviceInformation.Name
                    };
                }

                throw new IndexOutOfRangeException("The device has an unexpected number of descriptors.");
            }

            throw new NullReferenceException("The device ID was null.");
        }

        public static bool IsUsbDevice(this DeviceInformation deviceInformation)
        {
            var deviceMetaData = deviceInformation.ParseIdentifier();

            if (deviceMetaData.Name.Contains("USB"))
            {
                return true;
            }

            return false;
        }
    }
}

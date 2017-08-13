using System.Collections.Generic;

namespace Bifrost.Sensors
{
    public interface ISensor
    {
        string Protocol { get; set; }

        string Device { get; set; }

        IDictionary<string, int> Properties { get; set; }
    }
}
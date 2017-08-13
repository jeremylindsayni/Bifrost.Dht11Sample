using System.Collections.Generic;

namespace Bifrost.Sensors
{
    public abstract class AbstractSensor
    {
        public string Protocol { get; set; }

        public string Device { get; set; }

        public IDictionary<string, int> Properties { get; set; }
    }
}
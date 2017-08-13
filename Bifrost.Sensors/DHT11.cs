namespace Bifrost.Sensors
{
    public class Dht11 : AbstractSensor
    {
        public int Temperature
        {
            get
            {
                return Properties[nameof(Temperature)];
            }
        }

        public int Humidity
        {
            get
            {
                return Properties[nameof(Humidity)];
            }
        }
    }
}

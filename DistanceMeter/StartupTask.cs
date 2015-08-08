using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace DistanceMeter
{
    public sealed class StartupTask : IBackgroundTask
    {
        private  HC_SR04 hcSr04 = new HC_SR04(24,23,25,12);

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            hcSr04.Init();
            hcSr04.CountOfMeasure = 3;
            Petla();
        }

        public void Petla()
        {
                while (true)
                {
                    //Debug.WriteLine(hcSr04.Distance);
                    if (hcSr04.Distance < 80)
                    {
                        hcSr04.LedPin1.Write(GpioPinValue.High);
                        hcSr04.LedPin2.Write(GpioPinValue.Low);
                        Task.Delay(500).Wait();
                    }
                    else
                    {
                        hcSr04.LedPin1.Write(GpioPinValue.Low);
                        hcSr04.LedPin2.Write(GpioPinValue.High);
                    }
                }
        }

    }


}

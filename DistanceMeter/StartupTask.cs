using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace DistanceMeter
{
    public sealed class StartupTask : IBackgroundTask
    {
        private readonly HC_SR04 _hcSr04 = new HC_SR04(24,23,25,12);

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _hcSr04.Init();
            _hcSr04.CountOfMeasure = 20;
            Petla();
        }

        public void Petla()
        {
                while (true)
                {
                    double pomiar = _hcSr04.Distance;
                    if (pomiar < 80)
                    {
                        _hcSr04.LedPin1.Write(GpioPinValue.High);
                        _hcSr04.LedPin2.Write(GpioPinValue.Low);
                    }
                    else
                    {
                        _hcSr04.LedPin1.Write(GpioPinValue.Low);
                        _hcSr04.LedPin2.Write(GpioPinValue.High);
                    }
                    Debug.WriteLine(pomiar);
            }
        }

    }


}

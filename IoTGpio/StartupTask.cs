using System;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using System.Diagnostics;
using Windows.System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409
namespace IoTGpio
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const int FirstPinNumber = 35;
        private const int SecondPinNumber = 47;
        private GpioController _gpio;
        public GpioPin FirstPin { get; private set; }
        public GpioPin SecondPin { get; private set; }
        public ThreadPoolTimer Timer { get; private set; }
        private int[] give = new int[1000];

        public bool WhichPort { get; private set; }

        BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            GpioInit();

            Timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(500));
        }


        private void Timer_Tick(ThreadPoolTimer timer)
        {
            WhichPort ^= true;
            if(WhichPort==true) TogglePin(FirstPin);
            else TogglePin(SecondPin);
            StateCheckPin(FirstPin);
            StateCheckPin(SecondPin);
        }

        private void GpioInit()
        {
            _gpio = GpioController.GetDefault();

            //jesli gpio zwroci zero znaczy ze wystapil blad
            if(_gpio == null)
            {
                return;
            }

            FirstPin = _gpio.OpenPin(FirstPinNumber);
            FirstPin.Write(GpioPinValue.High);
            FirstPin.SetDriveMode(GpioPinDriveMode.Output);

            SecondPin = _gpio.OpenPin(SecondPinNumber);
            SecondPin.Write(GpioPinValue.High);
            SecondPin.SetDriveMode(GpioPinDriveMode.Output);
        }

        private void StateCheckPin(GpioPin pin)
        {
            var stated = pin.Read();
            if (stated == GpioPinValue.High)
            {
                Debug.WriteLine("HIGH " + pin.PinNumber);
            }
            if (stated == GpioPinValue.Low)
            {
                Debug.WriteLine("LOW " + pin.PinNumber); 
            }
        }

        private void TogglePin(GpioPin pin)
        {
            pin.Write(pin.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
        }
    }
}

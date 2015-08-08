using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using System.Diagnostics;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace I2C
{
    public sealed class StartupTask : IBackgroundTask
    {
        private readonly DS1307 _i2CTimer = new DS1307();
        private readonly I2C _i2c = new I2C();
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Start();
            while (true)
            {
            }

        }
        private async void Start()
        {
            List<byte> siema = await _i2c.GetAvailableDevicesAddress();
            Debug.WriteLine($"Available devices:{siema.Count}");
            foreach (byte t in siema)
            {
                string hex = t.ToString("X");
                Debug.WriteLine($"Address:{t} Hex:0x{hex}");
            }
            await _i2CTimer.Ds1307Init();
            _i2CTimer.WriteHours(23, TimeMode.Make24, ModeOf12.Pm);
            _i2CTimer.WriteMinutes(59);
            _i2CTimer.WriteSeconds(45);
            _i2CTimer.WriteDay(DayOfWeek.Saturday);
            _i2CTimer.WriteDayOfMonth(15);
            _i2CTimer.WriteMonth(10);
            _i2CTimer.WriteYear(22);
            while (true)
            {
                Debug.WriteLine(_i2CTimer.ReadFullDate());
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }

            
        }
    }
}

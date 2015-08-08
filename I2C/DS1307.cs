using System;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace I2C
{
    class DS1307
    {
        private const byte DS1307_I2C_ADDRESS = 0x68;
        private const byte DS1307_REG_SEC = 0x00;
        private const byte DS1307_REG_MIN = 0x01;
        private const byte DS1307_REG_HOUR = 0x02;
        private const byte DS1307_REG_DAY = 0x03;
        private const byte DS1307_REG_DATE = 0x04;
        private const byte DS1307_REG_MONTH = 0x05;
        private const byte DS1307_REG_YEAR = 0x06;
        private const byte DS1307_REG_CONTROL = 0x07;

        private readonly I2C _i2CDriver = new I2C();
        private readonly BitOperate _bitOperator = new BitOperate();

        public void Ds1307()
        {
        }
        public async Task<bool> Ds1307Init()
        {
            await _i2CDriver.I2CInit(DS1307_I2C_ADDRESS, I2cBusSpeed.StandardMode, I2cSharingMode.Exclusive);
            return true;
        }
        public uint ReadSeconds()
        {
            byte[] get = _i2CDriver.DataReader(1, DS1307_REG_SEC);
            return _bitOperator.Bcd2Int(Convert.ToUInt32(get[0]));
        }
        public void WriteSeconds(uint seconds)
        {
            if (seconds > 59) return;
            _i2CDriver.DataWriter(new[] { Convert.ToByte(_bitOperator.Uint2Bcd(seconds)) }, DS1307_REG_SEC);
        }

        public uint ReadMinutes()
        {
            byte[] get = _i2CDriver.DataReader(1, DS1307_REG_MIN);
            return _bitOperator.Bcd2Int(Convert.ToUInt32(get[0]));
        }
        public void WriteMinutes(uint minutes)
        {
            if (minutes > 59) return;
            _i2CDriver.DataWriter(new[] { Convert.ToByte(_bitOperator.Uint2Bcd(minutes)) }, DS1307_REG_MIN);
        }
        public string ReadHours()
        {
            byte[] get = _i2CDriver.DataReader(1, DS1307_REG_HOUR);
            uint hourData = Convert.ToUInt32(get[0]);

            //IS 24h MODE
            if (_bitOperator.AndAction(hourData, 64) != 64) return _bitOperator.Bcd2Int(hourData).ToString();
            //IS 12h MODE
            return _bitOperator.AndAction(hourData, 32) == 32
                    ? _bitOperator.Bcd2Int(_bitOperator.XorAction(hourData, 64, 32)).ToString() + " PM"
                    : _bitOperator.Bcd2Int(_bitOperator.XorAction(hourData, 64)).ToString() + " AM";
        }
        /// <summary>
        /// mode - date format AmPm or standard 
        /// amPm - when TimeMode.Make12 is selected use this value for write hours as PM or AM.
        /// Otherwise it can be null.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="amPm"></param>
        public void WriteHours(uint hours, TimeMode mode, ModeOf12 amPm)
        {
            _i2CDriver.DataWriter(new[] { Convert.ToByte(_bitOperator.Int2Bcd(mode.GetHashCode())) },
                                          DS1307_REG_HOUR);
            if (mode == TimeMode.Make24)
            {
                if (hours < 24)
                    _i2CDriver.DataWriter(new[] { Convert.ToByte(_bitOperator.Uint2Bcd(hours)) },
                                                  DS1307_REG_HOUR);
            }
            else if (mode != TimeMode.Make24)
            {
                if (hours >= 12) return;
                //to BCD
                hours = _bitOperator.Uint2Bcd(hours);
                ////////
                _i2CDriver.DataWriter(
                    new byte[] { _bitOperator.OrAction(hours, 64, Convert.ToUInt32(amPm.GetHashCode())) },
                    DS1307_REG_HOUR);
            }
        }
        public DayOfWeek ReadDay()
        {
            byte[] get = _i2CDriver.DataReader(1, DS1307_REG_DAY);
            return (DayOfWeek)get[0];
        }
        public void WriteDay(DayOfWeek day)
        {
            _i2CDriver.DataWriter(new[] { Convert.ToByte(_bitOperator.Int2Bcd(day.GetHashCode())) },
                                          DS1307_REG_DAY);
        }
        public uint ReadDayOfMonth()
        {
            byte[] get = _i2CDriver.DataReader(1, DS1307_REG_DATE);
            return _bitOperator.Bcd2Int(Convert.ToUInt32(get[0]));
        }
        public void WriteDayOfMonth(uint day)
        {
            if (day > 0 && day < 32)
                _i2CDriver.DataWriter(new[]{Convert.ToByte(_bitOperator.Int2Bcd(day.GetHashCode())) }, 
                                            DS1307_REG_DATE);
        }
        public uint ReadMonth()
        {
            byte[] get = _i2CDriver.DataReader(1, DS1307_REG_MONTH);
            return _bitOperator.Bcd2Int(Convert.ToUInt32(get[0]));
        }
        public void WriteMonth(uint month)
        {
            if (month < 13) _i2CDriver.DataWriter(new[] { Convert.ToByte(_bitOperator.Uint2Bcd(month)) }, 
                                                          DS1307_REG_MONTH);
        }
        public uint ReadYear()
        {
            byte[] get = _i2CDriver.DataReader(1, DS1307_REG_YEAR);
            return _bitOperator.Bcd2Int(Convert.ToUInt32(get[0]));
        }
        public void WriteYear(uint year)
        {
            if (year < 100)
                _i2CDriver.DataWriter(new[] { Convert.ToByte(_bitOperator.Uint2Bcd(year)) },
                                              DS1307_REG_YEAR);
        }
        public string ReadFullDate() => ReadDay().ToString() + " " + ReadDayOfMonth().ToString() + "."
                                        + ReadMonth().ToString() + "." + ReadYear().ToString() + " " + ReadHours().ToString() + ":"
                                        + ReadMinutes().ToString() + ":" + ReadSeconds().ToString();
        /// <summary>
        /// The fequency can only have this values: 1, 4096, 8192, 32768 kHz
        /// sqweState = 1 -> Enable, sqweState = 0 -> Disable
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="sqweState"></param>
        public void SetControlData(FreqMode frequency, int sqweState)
        {
            if (sqweState == 1)
            {
                const uint sqweMask = 16;
                _i2CDriver.DataWriter(new byte[] { _bitOperator.OrAction(Convert.ToUInt32(frequency.GetHashCode()), sqweMask) },
                DS1307_REG_CONTROL);
            }
            else _i2CDriver.DataWriter(new byte[] { Convert.ToByte(frequency.GetHashCode()) }, 
                                                    DS1307_REG_CONTROL);
        }
    }
}

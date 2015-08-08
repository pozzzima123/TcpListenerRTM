using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace I2C
{
    class I2C
    {
        private I2cDevice _i2CDevice;
        private bool _isConnected;

        public I2C()
        {
            
        }
        public void Dispose()
        {
            _i2CDevice.Dispose();
            _isConnected = false;
        }
        public async Task I2CInit(byte slaveAddress, I2cBusSpeed busSpeed, I2cSharingMode sharingMode)
        {
            // get aqs filter  to find i2c device
            string aqs = I2cDevice.GetDeviceSelector("I2C1");

            // Find the I2C bus controller with our selector string
            var dis = await DeviceInformation.FindAllAsync(aqs);
            if (dis.Count == 0)
                throw new Exception("There is no I2C device"); // bus not found

            I2cConnectionSettings settings = new I2cConnectionSettings(slaveAddress);
            // Create an I2cDevice with our selected bus controller and I2C settings
            _i2CDevice = await I2cDevice.FromIdAsync(dis[0].Id, settings);
            if (_i2CDevice == null) return;
            var status = _i2CDevice.WritePartial(new byte[] { 0x00 });
            if (status.Status == I2cTransferStatus.FullTransfer) _isConnected = true;
            else if (status.Status == I2cTransferStatus.PartialTransfer)
                throw new Exception("There was an error during sending test data.");
            else if (status.Status == I2cTransferStatus.SlaveAddressNotAcknowledged)
              throw new Exception("I2C Device is not connected.");
        
        }
        public async Task< List<byte> >  GetAvailableDevicesAddress()
        {
            // get aqs filter  to find i2c device
            string aqs = I2cDevice.GetDeviceSelector("I2C1");

            // Find the I2C bus controller with our selector string
            var dis = await DeviceInformation.FindAllAsync(aqs);
            if (dis.Count == 0)
                throw new Exception("There is no I2C device"); // bus not found

            List<byte> devicesList = new List<byte>();

            for (byte i = 0; i < 127; i++)
            {
                I2cConnectionSettings settings = new I2cConnectionSettings(i);
                // Create an I2cDevice with our selected bus controller and I2C settings
                var device = await I2cDevice.FromIdAsync(dis[0].Id, settings);
                var result = device.WritePartial(new byte[] { 0x00 });
                if (result.Status == I2cTransferStatus.SlaveAddressNotAcknowledged) continue;
                devicesList.Add(i);
                device.Dispose();
            }

            return devicesList;
        }
        /// <summary>
        /// Return true - data send complete, false - there was an error.
        /// </summary>
        /// <param name="dataDec"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public bool DataWriter(byte[] dataDec, byte registry)
        {
            if (!_isConnected) return false;
            //doklejenie registry do reszty danych
            List<byte> byteArrayix = new List<byte> {registry}; //index==0 -> registry 
            byteArrayix.AddRange(dataDec); //index >0
            var getRaport = _i2CDevice.WritePartial(byteArrayix.ToArray());
            return getRaport.Status == I2cTransferStatus.FullTransfer;
        }
        /// <summary>
        /// Return bytes - data read complete, 0x00 - there was an error.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public byte[] DataReader(int buffer, byte registry)
        {
            if (!_isConnected) return new byte[] {0x00};
            byte[] writeBuffer = { registry };
            byte[] readBuffer = new byte[buffer];
            var result = _i2CDevice.WriteReadPartial(writeBuffer, readBuffer);
            return result.Status == I2cTransferStatus.FullTransfer ? readBuffer : 
                                                                     new byte[]{0x00};
        }
    }
}

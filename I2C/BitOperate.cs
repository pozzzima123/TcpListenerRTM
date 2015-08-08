using System;

namespace I2C
{
    public sealed class BitOperate
    {
        public uint AndAction(uint value1, uint value2) => (value1 & value2);
        public uint XorAction(uint value1, uint value2) => (value1 ^ value2);
        public uint XorAction(uint value1, uint value2, uint value3) => 
            (value1 ^ value2 ^ value3);
        public byte OrAction(uint value1, uint value2) => Convert.ToByte(value1 | value2);
        public byte OrAction(uint value1, uint value2, uint value3) => 
            Convert.ToByte(value1 | value2 | value3);
        public uint Bcd2Int(uint bcd) => uint.Parse(bcd.ToString("X"));
        public uint Uint2Bcd(uint iValue) => Convert.ToUInt32(iValue.ToString(), 16);
        public uint Int2Bcd(int iValue) => Convert.ToUInt32(iValue.ToString(), 16);
    }
}

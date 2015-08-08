namespace I2C
{
    public enum DayOfWeek:uint
    {
        ToPropertyIndex,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }
    public enum ModeOf12
    {
        Am = 0,
        Pm = 32
    }
    public enum FreqMode
    {
        Get1 = 0,
        Get4096 = 1,
        Get8192 = 2,
        Get32768 = 3
    }
    public enum TimeMode
    {
        Make24 = 0,
        Make12 = 64,
    }
}

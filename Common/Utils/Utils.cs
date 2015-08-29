using System;

namespace Common.Utils
{
    public static class Utils
    {
        public static T ParseEnum<T>(string name)
        {
            return (T) Enum.Parse(typeof (T), name);
        }
    }
}
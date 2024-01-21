namespace GoogleMapExport2KML.Extensions;
internal static class DictionaryExtensions
{
    internal static bool TryGetValue<T>(this Dictionary<string, object> dictionary, string key, out T? convertedValue)
    {
        convertedValue = default;
        if (dictionary == null)
        {
            return false;
        }
        if (dictionary.TryGetValue(key, out var value))
        {
            if(value == null)
            {
                return false;
            }
            convertedValue = (T)Convert.ChangeType(value, typeof(T));
            return true;
        }
        return false;
    }

    internal static long ConvertFromHex(this string hexNumber)
    {
        // Convert hexadecimal to decimal
        return Convert.ToInt64(hexNumber);
    }

    internal static double ConvertToCoordinate(this long floatingPointNumber)
    {
        // Convert the 64-bit integer to a byte array
        byte[] byteArray = BitConverter.GetBytes(floatingPointNumber);

        // Reverse the byte array if needed (depends on endianness)
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(byteArray);
        }

        // Interpret the byte array as a double-precision float
        return BitConverter.ToDouble(byteArray, 0);
    }
}

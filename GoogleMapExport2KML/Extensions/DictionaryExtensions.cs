using System.Globalization;

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

    public static double GPSCoordinateConversion(string GPSCoordinate)
    {
        double coordinate = 0;
        byte[] _byte = new byte[GPSCoordinate.Length / 2];
        double value1 = 0;
        double value2 = 0;

        for (int i = 0; i < _byte.Length; i++)
        {
            string byteValue = GPSCoordinate.Substring(i * 2, 2);
            _byte[i] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        value1 = Convert.ToDouble(_byte[2]) / 100;
        value2 = Convert.ToDouble(_byte[3]) / 10000;

        coordinate = Convert.ToDouble(_byte[0] + ((_byte[1] + value1 + value2) / 60));

        return coordinate;
    }

    public static double GetLatitude(this string GPSHexCoordinate)
    {
        double latitude = 0;

        if (GPSHexCoordinate.StartsWith("0x"))
        {
            GPSHexCoordinate = GPSHexCoordinate.Substring(2);
        }
        latitude = GPSCoordinateConversion(GPSHexCoordinate.Substring(0, 8));

        return latitude;
    }
    public static double GetLongitude(this string GPSHexCoordinate)
    {
        double longitude = 0;

        if (GPSHexCoordinate.StartsWith("0x"))
        {
            GPSHexCoordinate = GPSHexCoordinate.Substring(2);
        }
        longitude = GPSCoordinateConversion(GPSHexCoordinate.Substring(8, 8));

        return longitude;
    }

    //internal static long ConvertFromHex(this string hexNumber)
    //{
    //    // Convert hexadecimal to decimal
    //    return Convert.ToInt32(hexNumber);
    //}

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

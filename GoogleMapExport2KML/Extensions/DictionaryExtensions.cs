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
}

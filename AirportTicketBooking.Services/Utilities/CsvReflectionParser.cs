using System.Reflection;
using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Common;
using AirportTicketBooking.Domain.Enums;

namespace AirportTicketBooking.Services.Utilities;

public static class CsvReflectionParser
{
    private static readonly List<StandardPropertyMapping> _cachedProperties = new();
    private static readonly List<PriceDictionaryMapping> _cachedPrices = new();
    private static readonly PropertyInfo? _pricesProperty;

    private record StandardPropertyMapping(PropertyInfo Property, int ColumnIndex, Type TargetType);
    private record PriceDictionaryMapping(FlightClass Class, int ColumnIndex);

    static CsvReflectionParser()
    {
        var flightType = typeof(Flight);
        var properties = flightType.GetProperties();

        foreach (var prop in properties)
        {
            var columnAttr = prop.GetCustomAttribute<CsvColumnAttribute>();
            if (columnAttr != null)
            {
                _cachedProperties.Add(new StandardPropertyMapping(prop, columnAttr.Index, prop.PropertyType));
            }

            var priceMappings = prop.GetCustomAttributes<CsvPriceMappingAttribute>();
            if (priceMappings.Any())
            {
                _pricesProperty = prop; 
                foreach (var mapping in priceMappings)
                {
                    _cachedPrices.Add(new PriceDictionaryMapping(mapping.Class, mapping.Index));
                }
            }
        }
    }


    public static Flight ParseRow(string[] columns)
    {

        var flight = (Flight)Activator.CreateInstance(typeof(Flight))!;

        foreach (var mapping in _cachedProperties)
        {
            if (mapping.ColumnIndex >= columns.Length) continue; 

            string rawValue = columns[mapping.ColumnIndex];
            object parsedValue = ConvertChangeType(rawValue, mapping.TargetType);

            mapping.Property.SetValue(flight, parsedValue);
        }

        if (_pricesProperty != null && _cachedPrices.Any())
        {
            var pricesDictionary = new Dictionary<FlightClass, decimal>();

            foreach (var priceMapping in _cachedPrices)
            {
                if (priceMapping.ColumnIndex >= columns.Length) continue;

                string rawPrice = columns[priceMapping.ColumnIndex];
                if (decimal.TryParse(rawPrice, out decimal parsedPrice))
                {
                    pricesDictionary.Add(priceMapping.Class, parsedPrice);
                }
            }

            _pricesProperty.SetValue(flight, pricesDictionary);
        }

        return flight;
    }   

    private static object ConvertChangeType(string value, Type targetType)
    {
        if (targetType == typeof(DateTime))
            return DateTime.Parse(value);

        return Convert.ChangeType(value, targetType);
    }
}
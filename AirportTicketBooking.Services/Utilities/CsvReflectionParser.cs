using System.Reflection;
using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Common;
using AirportTicketBooking.Domain.Enums;
using AirportTicketBooking.Services.Enums;  
namespace AirportTicketBooking.Services.Utilities;

public static class CsvReflectionParser
{
    private static readonly List<StandardPropertyMapping> _cachedProperties = new();
    private static readonly List<PriceDictionaryMapping> _cachedPrices = new();
    private static readonly List<PropertyValidationInfo> _cachedValidationInfo = new();
    private static readonly PropertyInfo? _pricesProperty;

    private record StandardPropertyMapping(PropertyInfo Property, int ColumnIndex, Type TargetType);
    private record PriceDictionaryMapping(FlightClass Class, int ColumnIndex);

    public  record PropertyValidationInfo(
        PropertyInfo Property,
        bool IsRequired,
        DisplayFieldType FieldType,
        bool HasFutureDateConstraint,
        bool IsNonNegative
    );

    static CsvReflectionParser()
    {
        var flightType = typeof(Flight);
        var properties = flightType.GetProperties();

        foreach (var prop in properties)
        {
            var columnAttr = prop.GetCustomAttribute<CsvColumnAttribute>();
            var priceMappings = prop.GetCustomAttributes<CsvPriceMappingAttribute>();
            bool hasPrice = priceMappings.Any();

            if (columnAttr != null)
            {
                _cachedProperties.Add(new StandardPropertyMapping(prop, columnAttr.Index, prop.PropertyType));
            }

            if (hasPrice)
            {
                _pricesProperty = prop;
                foreach (var mapping in priceMappings)
                {
                    _cachedPrices.Add(new PriceDictionaryMapping(mapping.Class, mapping.Index));
                }
            }

            if (columnAttr != null || hasPrice)
            {
                bool isRequired = prop.CustomAttributes.Any(a => a.AttributeType.Name == "RequiredMemberAttribute");
                bool hasFutureDate = prop.GetCustomAttribute<FutureDateAttribute>() != null;
                bool isNonNegative = prop.PropertyType == typeof(int) || hasPrice;
                DisplayFieldType fieldType = MapToDisplayFieldType(prop.PropertyType, hasPrice);

                _cachedValidationInfo.Add(new PropertyValidationInfo(prop, isRequired, fieldType, hasFutureDate, isNonNegative));
            }
        }
    }

    private static DisplayFieldType MapToDisplayFieldType(Type propertyType, bool hasPrice)
    {
        if (propertyType == typeof(string)) return DisplayFieldType.FreeText;
        if (propertyType == typeof(DateTime)) return DisplayFieldType.DateTime;
        if (propertyType == typeof(int)) return DisplayFieldType.Integer;
        if (hasPrice) return DisplayFieldType.PriceDictionary;

        return DisplayFieldType.FreeText;
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
                if (decimal.TryParse(rawPrice, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedPrice))
                {
                    pricesDictionary.Add(priceMapping.Class, parsedPrice);
                }
            }

            _pricesProperty.SetValue(flight, pricesDictionary);
        }

        return flight;
    }

    public static IReadOnlyList<PropertyValidationInfo> GetValidationInfo() => _cachedValidationInfo;

    private static object ConvertChangeType(string value, Type targetType)
    {
        if (targetType == typeof(DateTime))
            return DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        return Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
    }
}
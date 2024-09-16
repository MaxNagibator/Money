using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Money.Common;

public abstract class Enumeration(int value, string name) : IComparable
{
    public string Name { get; } = name;

    public int Value { get; } = value;

    public static implicit operator int(Enumeration enumeration)
    {
        return enumeration.Value;
    }

    public static implicit operator string(Enumeration enumeration)
    {
        return enumeration.Name;
    }

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        return fields.Select(f => f.GetValue(null)).Cast<T>();
    }

    public static T FromValue<T>(int value) where T : Enumeration
    {
        T matchingItem = Parse<T, int>(value, nameof(value), item => item.Value == value);
        return matchingItem;
    }

    public static T FromName<T>(string name) where T : Enumeration
    {
        T matchingItem = Parse<T, string>(name, nameof(name), item => item.Name == name);
        return matchingItem;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        bool typeMatches = GetType() == obj.GetType();
        bool valueMatches = Value.Equals(otherValue.Value);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode()
    {
        return Value;
    }

    public int CompareTo(Enumeration? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return 1;
        }

        return Value.CompareTo(other.Value);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        if (ReferenceEquals(this, obj))
        {
            return 0;
        }

        return obj is Enumeration other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Enumeration)}");
    }

    private static T Parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration
    {
        T? matchingItem = GetAll<T>().FirstOrDefault(predicate);

        if (matchingItem == null)
        {
            throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");
        }

        return matchingItem;
    }
}

public class EnumerationConverter<T> : JsonConverter<T> where T : Enumeration
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            int value = reader.GetInt32();
            return Enumeration.FromValue<T>(value);
        }

        throw new JsonException("Invalid JSON token for Enumeration");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}

public class EnumerationConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Enumeration).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(EnumerationConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

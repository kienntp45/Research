using System;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;

namespace Lib.MessageBus.Kafka.Extensions;

internal static class IConfigurationExtensions
{
	internal static T GetValue<T>(this IConfiguration configuration, string key, T defaultValue = default(T))
	{
		IConfigurationSection section = configuration.GetSection(key);
		string value = section.Value;
		if (!string.IsNullOrWhiteSpace(value))
		{
			object obj = ConvertValue(typeof(T), value);
			if (obj != null)
			{
				return (T)obj;
			}
		}
		return defaultValue;
	}

	private static object ConvertValue(Type type, string value)
	{
		TryConvertValue(type, value, out var result);
		return result;
	}

	private static bool TryConvertValue(Type type, string value, out object result)
	{
		result = null;
		if (string.IsNullOrWhiteSpace(value))
		{
			return true;
		}
		Type type2 = type;
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			type2 = Nullable.GetUnderlyingType(type);
		}
		TypeConverter converter = TypeDescriptor.GetConverter(type2);
		if (converter.CanConvertFrom(typeof(string)))
		{
			try
			{
				result = converter.ConvertFromInvariantString(value);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
		if (type2 == typeof(byte[]))
		{
			try
			{
				result = Convert.FromBase64String(value);
				return true;
			}
			catch (FormatException)
			{
				return false;
			}
		}
		return false;
	}
}

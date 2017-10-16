using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Faithlife.Json.Converters;
using Faithlife.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Faithlife.Json
{
	/// <summary>
	/// Helper methods for working with Json.NET.
	/// </summary>
	public static class JsonUtility
	{
		/// <summary>
		/// An equality comparer for JToken.
		/// </summary>
		/// <remarks>This comparer ignores the order of object properties.</remarks>
		public static readonly IEqualityComparer<JToken> JTokenEqualityComparer = new OurJTokenEqualityComparer();

		/// <summary>
		/// Converts the object to JSON.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The JSON.</returns>
		public static string ToJson(object value)
		{
			return ToJson(value, null);
		}

		/// <summary>
		/// Converts the object to JSON.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The JSON.</returns>
		public static string ToJson(object value, JsonOutputSettings settings)
		{
			using (StringWriter stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture))
			{
				ToJsonTextWriter(value, settings, stringWriter);
				return stringWriter.ToString();
			}
		}

		/// <summary>
		/// Converts the object to a JSON writer.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="jsonWriter">The JSON writer to write JSON to.</param>
		public static void ToJsonWriter(object value, JsonWriter jsonWriter)
		{
			ToJsonWriter(value, null, jsonWriter);
		}

		/// <summary>
		/// Converts the object to a JSON writer.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <param name="jsonWriter">The JSON writer to write JSON to.</param>
		public static void ToJsonWriter(object value, JsonOutputSettings settings, JsonWriter jsonWriter)
		{
			JsonSerializer serializer = JsonSerializer.Create(CreateDefaultJsonSerializerSettings(settings));
			serializer.Serialize(jsonWriter, value);
		}

		/// <summary>
		/// Converts the object to a JSON text writer.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="textWriter">The text writer to write JSON to.</param>
		public static void ToJsonTextWriter(object value, TextWriter textWriter)
		{
			ToJsonTextWriter(value, null, textWriter);
		}

		/// <summary>
		/// Converts the object to a JSON text writer.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <param name="textWriter">The text writer to write JSON to.</param>
		public static void ToJsonTextWriter(object value, JsonOutputSettings settings, TextWriter textWriter)
		{
			Formatting formatting = GetJsonFormatting(settings);
			using (JsonTextWriter jsonTextWriter = new JsonTextWriter(textWriter) { Formatting = formatting, CloseOutput = false })
				ToJsonWriter(value, settings, jsonTextWriter);
		}

		/// <summary>
		/// Converts the object to a JSON stream.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="outputStream">The stream to write JSON to, using UTF-8 encoding.</param>
		public static void ToJsonStream(object value, Stream outputStream)
		{
			ToJsonStream(value, null, outputStream);
		}

		/// <summary>
		/// Converts the object to a JSON stream.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <param name="outputStream">The stream to write JSON to, using UTF-8 encoding.</param>
		public static void ToJsonStream(object value, JsonOutputSettings settings, Stream outputStream)
		{
			// don't dispose the StreamWriter to avoid closing the stream
			StreamWriter textWriter = new StreamWriter(outputStream);
			ToJsonTextWriter(value, settings, textWriter);
			textWriter.Flush();
		}

		/// <summary>
		/// Converts the object to compressed JSON.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The compressed JSON (as compressed by StringUtility).</returns>
		public static byte[] ToCompressedJson(object value)
		{
			return ToCompressedJson(value, null);
		}

		/// <summary>
		/// Converts the object to JSON.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The JSON.</returns>
		public static byte[] ToCompressedJson(object value, JsonOutputSettings settings)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (TextWriter textWriter = StringUtility.CreateCompressingTextWriter(stream, Ownership.None))
					ToJsonTextWriter(value, textWriter);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Returns the number of bytes used by the JSON of an object.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The number of bytes used by the JSON of an object.</returns>
		public static int ToJsonByteCount(object value)
		{
			return ToJsonByteCount(value, null);
		}

		/// <summary>
		/// Returns the number of bytes used by the JSON of an object.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The number of bytes used by the JSON of an object.</returns>
		public static int ToJsonByteCount(object value, JsonOutputSettings settings)
		{
			using (ZeroStream zeroStream = new ZeroStream())
			{
				ToJsonStream(value, settings, zeroStream);
				return (int) zeroStream.Length;
			}
		}

		/// <summary>
		/// Converts the object to a JToken.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The JToken.</returns>
		public static JToken ToJToken(object value)
		{
			return ToJToken(value, null);
		}

		/// <summary>
		/// Converts the object to a JToken.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The JToken.</returns>
		public static JToken ToJToken(object value, JsonOutputSettings settings)
		{
			using (JTokenWriter jTokenWriter = new JTokenWriter())
			{
				ToJsonWriter(value, settings, jTokenWriter);
				return jTokenWriter.Token;
			}
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <param name="json">The JSON.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static T FromJson<T>(string json)
		{
			return FromJson<T>(json, null);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <param name="json">The JSON.</param>
		/// <param name="type">The type.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static object FromJson(string json, Type type)
		{
			return FromJson(json, type, null);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <param name="json">The JSON.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static T FromJson<T>(string json, JsonInputSettings settings)
		{
			return (T) FromJson(json, typeof(T), settings);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <param name="json">The JSON.</param>
		/// <param name="type">The type.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static object FromJson(string json, Type type, JsonInputSettings settings)
		{
			using (StringReader stringReader = new StringReader(json))
				return FromJsonTextReader(stringReader, type, settings);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <param name="textReader">The JSON.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static T FromJsonTextReader<T>(TextReader textReader)
		{
			return FromJsonTextReader<T>(textReader, null);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <param name="textReader">The JSON.</param>
		/// <param name="type">The type.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static object FromJsonTextReader(TextReader textReader, Type type)
		{
			return FromJsonTextReader(textReader, type, null);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <param name="textReader">The JSON.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static T FromJsonTextReader<T>(TextReader textReader, JsonInputSettings settings)
		{
			return (T) FromJsonTextReader(textReader, typeof(T), settings);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <param name="textReader">The JSON.</param>
		/// <param name="type">The type.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static object FromJsonTextReader(TextReader textReader, Type type, JsonInputSettings settings)
		{
			using (JsonReader reader = new JsonTextReader(textReader))
				return Deserialize(settings, reader, type);
		}

		/// <summary>
		/// Creates an object from compressed JSON.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <param name="json">The compressed JSON.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static T FromCompressedJson<T>(byte[] json)
		{
			return FromCompressedJson<T>(json, null);
		}

		/// <summary>
		/// Creates an object from compressed JSON.
		/// </summary>
		/// <param name="json">The compressed JSON.</param>
		/// <param name="type">The type.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static object FromCompressedJson(byte[] json, Type type)
		{
			return FromCompressedJson(json, type, null);
		}

		/// <summary>
		/// Creates an object from compressed JSON.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <param name="json">The compressed JSON.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static T FromCompressedJson<T>(byte[] json, JsonInputSettings settings)
		{
			return (T) FromCompressedJson(json, typeof(T), settings);
		}

		/// <summary>
		/// Creates an object from compressed JSON.
		/// </summary>
		/// <param name="json">The compressed JSON.</param>
		/// <param name="type">The type.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static object FromCompressedJson(byte[] json, Type type, JsonInputSettings settings)
		{
			using (MemoryStream stream = new MemoryStream(json, false))
			using (TextReader textReader = StringUtility.CreateDecompressingTextReader(stream, Ownership.None))
				return FromJsonTextReader(textReader, type, settings);
		}

		/// <summary>
		/// Creates an object from a JToken.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <param name="json">The JToken.</param>
		/// <returns>The object.</returns>
		public static T FromJToken<T>(JToken json)
		{
			return FromJToken<T>(json, null);
		}

		/// <summary>
		/// Creates an object from a JToken.
		/// </summary>
		/// <param name="json">The JToken.</param>
		/// <param name="type">The type.</param>
		/// <returns>The object.</returns>
		public static object FromJToken(JToken json, Type type)
		{
			return FromJToken(json, type, null);
		}

		/// <summary>
		/// Creates an object from a JToken.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <param name="json">The JToken.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		public static T FromJToken<T>(JToken json, JsonInputSettings settings)
		{
			return (T) FromJToken(json, typeof(T), settings);
		}

		/// <summary>
		/// Creates an object from a JToken.
		/// </summary>
		/// <param name="json">The JToken.</param>
		/// <param name="type">The type.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		public static object FromJToken(JToken json, Type type, JsonInputSettings settings)
		{
			if (json.IsNull())
				return json;

			using (JsonReader reader = new JTokenReader(json))
				return Deserialize(settings, reader, type);
		}

		/// <summary>
		/// Creates default serialization settings.
		/// </summary>
		/// <returns>The serialization settings used by ToJson and FromJson.</returns>
		public static JsonSerializerSettings CreateDefaultJsonSerializerSettings()
		{
			return DoCreateDefaultJsonSerializerSettings(null, null);
		}

		/// <summary>
		/// Creates default serialization settings.
		/// </summary>
		/// <returns>The serialization settings used by ToJson.</returns>
		public static JsonSerializerSettings CreateDefaultJsonSerializerSettings(JsonOutputSettings outputSettings)
		{
			return DoCreateDefaultJsonSerializerSettings(null, outputSettings);
		}

		/// <summary>
		/// Creates default serialization settings.
		/// </summary>
		/// <returns>The serialization settings used by FromJson.</returns>
		public static JsonSerializerSettings CreateDefaultJsonSerializerSettings(JsonInputSettings inputSettings)
		{
			return DoCreateDefaultJsonSerializerSettings(inputSettings, null);
		}

		/// <summary>
		/// Gets a persistent hash code for the token.
		/// </summary>
		/// <param name="token">The token, which must not be <c>null</c>.</param>
		/// <returns>The persistent hash code.</returns>
		public static int GetPersistentHashCode(JToken token)
		{
			// return hard-coded hash code for null
			if (token.IsNull())
				return 10;

			JTokenType tokenType = token.Type;

			// compare arrays
			if (tokenType == JTokenType.Array)
			{
				// combine type hash code with hash codes for elements
				IList<JToken> list = (IList<JToken>) token;
				int[] hashCodes = new int[list.Count + 1];
				hashCodes[0] = 2;
				for (int index = 0; index < list.Count; index++)
					hashCodes[index + 1] = GetPersistentHashCode(list[index]);
				return HashCodeUtility.CombineHashCodes(hashCodes);
			}

			// compare objects
			if (tokenType == JTokenType.Object)
			{
				// use XOR so that order doesn't matter
				IDictionary<string, JToken> properties = (IDictionary<string, JToken>) token;
				int hashCode = 0;
				foreach (KeyValuePair<string, JToken> property in properties)
					hashCode ^= HashCodeUtility.CombineHashCodes(property.Key.GetPersistentHashCode(), GetPersistentHashCode(property.Value));

				// combine type hash code with hash codes for properties
				return HashCodeUtility.CombineHashCodes(1, hashCode);
			}

			// combine type hash code with hash code for string
			if (tokenType == JTokenType.String)
				return HashCodeUtility.CombineHashCodes(8, ((string) token).GetPersistentHashCode());

			// combine type hash code with hash code for bool
			if (tokenType == JTokenType.Boolean)
				return HashCodeUtility.CombineHashCodes(9, HashCodeUtility.GetPersistentHashCode((bool) token));

			// return hash code of string representation of anything else (e.g. numbers)
			string tokenAsString = token.ToString(Formatting.None);
			return tokenAsString.GetPersistentHashCode();
		}

		/// <summary>
		/// Gets the JSON formatting specified by the settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <returns>The JSON formatting.</returns>
		public static Formatting GetJsonFormatting(JsonOutputSettings settings)
		{
			return settings != null && settings.IsIndented ? Formatting.Indented : Formatting.None;
		}

		/// <summary>
		/// Clones the specified Json.NET token.
		/// </summary>
		/// <typeparam name="T">The type of token.</typeparam>
		/// <param name="token">The token.</param>
		/// <returns>The clone.</returns>
		public static T Clone<T>(T token) where T : JToken
		{
			return token == null ? null : (T) token.DeepClone();
		}

		private static JsonSerializerSettings DoCreateDefaultJsonSerializerSettings(JsonInputSettings inputSettings, JsonOutputSettings outputSettings)
		{
			inputSettings = inputSettings ?? new JsonInputSettings();
			outputSettings = outputSettings ?? new JsonOutputSettings();

			JsonSerializerSettings settings =
				new JsonSerializerSettings
				{
					ContractResolver = inputSettings.ContractResolver ?? outputSettings.ContractResolver ?? new CamelCasePropertyNamesContractResolver(),
					DateParseHandling = DateParseHandling.None,
					NullValueHandling = inputSettings.IncludesNullValues ? NullValueHandling.Include : NullValueHandling.Ignore,
					MissingMemberHandling = inputSettings.IgnoresExtraProperties ? MissingMemberHandling.Ignore : MissingMemberHandling.Error
				};

			if (inputSettings.Converters != null)
				settings.Converters.AddRange(inputSettings.Converters);

			if (outputSettings.Converters != null)
				settings.Converters.AddRange(outputSettings.Converters);

			settings.Converters.AddRange(s_defaultConverters);

			return settings;
		}

		private static object Deserialize(JsonInputSettings settings, JsonReader reader, Type type)
		{
			JsonSerializer serializer = JsonSerializer.Create(CreateDefaultJsonSerializerSettings(settings));
			object value = serializer.Deserialize(reader, type);
			if (reader.Read() && reader.TokenType != JsonToken.Comment)
				throw new JsonSerializationException("Additional text found in JSON after deserializing.");
			if (value == null && type == typeof(JToken))
				value = new JValue((object) null);
			return value;
		}

		private sealed class OurJTokenEqualityComparer : IEqualityComparer<JToken>
		{
			public bool Equals(JToken left, JToken right)
			{
				if (object.ReferenceEquals(left, right))
					return true;

				if (left.IsNull())
					return right.IsNull();
				else if (right.IsNull())
					return false;

				JTokenType leftType = left.Type;
				JTokenType rightType = right.Type;

				// compare arrays
				if (leftType == JTokenType.Array)
				{
					if (rightType != JTokenType.Array)
						return false;

					JArray leftArray = (JArray) left;
					JArray rightArray = (JArray) right;

					// check count then items
					return leftArray.Count == rightArray.Count && leftArray.SequenceEqual(rightArray, this);
				}

				// compare objects
				if (leftType == JTokenType.Object)
				{
					if (rightType != JTokenType.Object)
						return false;

					IDictionary<string, JToken> leftProperties = (IDictionary<string, JToken>) left;
					IDictionary<string, JToken> rightProperties = (IDictionary<string, JToken>) right;

					// check count first
					if (leftProperties.Count != rightProperties.Count)
						return false;

					// allow properties to be in any order, but make sure they have the same names and values
					foreach (KeyValuePair<string, JToken> leftProperty in leftProperties)
					{
						JToken rightValue;
						if (!rightProperties.TryGetValue(leftProperty.Key, out rightValue))
							return false;
						if (!Equals(leftProperty.Value, rightValue))
							return false;
					}
					return true;
				}

				// compare strings ordinally
				if (leftType == JTokenType.String)
					return rightType == JTokenType.String && (string) left == (string) right;

				// compare Booleans
				if (leftType == JTokenType.Boolean)
					return rightType == JTokenType.Boolean && (bool) left == (bool) right;

				// compare string representations of anything else (e.g. numbers)
				return left.ToString(Formatting.None) == right.ToString(Formatting.None);
			}

			public int GetHashCode(JToken token)
			{
				return token == null ? 0 : GetPersistentHashCode(token);
			}
		}

		private static readonly IReadOnlyList<JsonConverter> s_defaultConverters =
			new JsonConverter[]
			{
				new CamelCaseEnumJsonConverter(),
				new IsoDateTimeUtcJsonConverter(),
				new IsoDateTimeOffsetJsonConverter(),
				new DictionaryKeysAreNotPropertyNamesJsonConverter(), // NOTE: must be after any other dictionary converters
			};
	}
}
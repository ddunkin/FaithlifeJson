# UnixDateTimeConverter class

Handles conversion an integer representing a unix UTC timestamp and a UTC DateTime.

```csharp
public class UnixDateTimeConverter : JsonConverterBase<DateTime>
```

## Public Members

| name | description |
| --- | --- |
| [UnixDateTimeConverter](UnixDateTimeConverter/UnixDateTimeConverter.md)() | The default constructor. |

## Protected Members

| name | description |
| --- | --- |
| override [ReadCore](UnixDateTimeConverter/ReadCore.md)(…) | Reads the JSON representation of the object. |
| override [WriteCore](UnixDateTimeConverter/WriteCore.md)(…) | Writes the JSON representation of the object. |

## See Also

* class [JsonConverterBase&lt;T&gt;](JsonConverterBase-1.md)
* namespace [Faithlife.Json.Converters](../Faithlife.Json.md)
* [UnixDateTimeConverter.cs](https://github.com/Faithlife/FaithlifeJson/tree/master/src/Faithlife.Json/Converters/UnixDateTimeConverter.cs)

<!-- DO NOT EDIT: generated by xmldocmd for Faithlife.Json.dll -->
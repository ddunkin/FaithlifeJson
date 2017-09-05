# JsonPointer class

Points to a specific node within a JSON document.

```csharp
public sealed class JsonPointer
```

## Public Members

| name | description |
| --- | --- |
| [JsonPointer](JsonPointer/JsonPointer.md)(…) | Initializes a new instance of the [`JsonPointer`](JsonPointer.md) class. |
| static readonly [Root](JsonPointer/Root.md) | The root pointer. |
| static [Parse](JsonPointer/Parse.md)(…) | Parses a JSON pointer. |
| [Names](JsonPointer/Names.md) { get; } | Gets the property names and/or array indices. |
| [Parent](JsonPointer/Parent.md) { get; } | Gets the parent JSON pointer, or null if this pointer is at the root. |
| [Concat](JsonPointer/Concat.md)(…) | Concatenates two JSON pointers. |
| [Evaluate](JsonPointer/Evaluate.md)(…) | Evaluates the pointer against the specified JSON token. |
| override [ToString](JsonPointer/ToString.md)() | Converts the JSON pointer to a string. |
| static [TryParse](JsonPointer/TryParse.md)(…) |  |

## Remarks

See http://tools.ietf.org/html/draft-pbryan-zyp-json-pointer-00 for details.

## See Also

* namespace [Faithlife.Json](../Faithlife.Json.md)
* [JsonPointer.cs](https://github.com/Faithlife/FaithlifeJson/tree/master/src/Faithlife.Json/JsonPointer.cs)

<!-- DO NOT EDIT: generated by xmldocmd for Faithlife.Json.dll -->
# Arguments

## The `Arguments` class

The `Arguments` class is used by Windsor to pass arguments [down the invocation pipeline](how-dependencies-are-resolved.md). The class is a simple implementation of non-generic `IDictionary`, but it has some useful capabilities.

Resolution arguments are key/value pairs, keyed either by name (`string`) or type (`System.Type`). How the arguments are added to the `Arguments` collections determines how they are bound to dependencies during resolution.

### Constructors

The class has several constructors:
```csharp
// Empty collection
new Arguments();

// Initialize from another collection
new Arguments(new Dictionary<object, object>());
```

Other constructors form part of the fluent interface of inserting arguments:
```csharp
new Arguments("hostname", "localhost");
new Arguments(typeof(MyClass), new MyClass());
```

### Fluent Interface

:information_source: **AddXXX methods overwrite:** these methods overwrite existing arguments rather than throw an exception.

#### Named Arguments

Arguments will be matched by a string key in a case insensitive manner. For example, `logLevel`, `LogLevel` and even `LOGLEVEL` as property names are all treated as one key.

```csharp
new Arguments()
	.AddNamed("key", 123456)
	.AddNamed(new Dictionary<string, string> { { "string-key", "string-value" } });
```

Named arguments can also be added from a plain old C# object or from properties of an anonymous type:
```csharp
new Arguments()
	.AddNamedProperties(myPOCO) // plain old C# object with public properties
	.AddNamedProperties(new { logLevel = LogLevel.High }); // anonymous type
```

#### Typed Arguments

Arguments can be matched by type as dependencies:

```csharp
new Arguments()
	.AddTyped(LogLevel.High, new AppConfig()) // params array
	.AddTyped(typeof(MyClass), new MyClass())
	.AddTyped<IService>(new MyService());
```

:information_source: **Typed arguments are matched exactly:** When you don't specify the type of the argument, its concrete type will be used. For example, if you pass a `MemoryStream` it will only match to a dependency of type `MemoryStream`, but not of the base type `Stream`. If you want to match it to `Stream` specify the type explicitly.

#### Named and/or Typed Arguments Collection

A collection implementing non-generic `IDictionary` containing named and/or typed arguments can be added to the `Arguments` instance:
```csharp
IDictionary map = new Dictionary<object, object>();
map.Add("string-key", 123456);
map.Add(typeof(TypeKey), 123456);

new Arguments()
 	.Add(map);
```

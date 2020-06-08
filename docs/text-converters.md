### Adding Text Converters

The framework can only bind properties for which it has an **ITextConverter** to
convert a text value (or values) to a target property type (or array/generic list
of such type).

The base framework comes with converters for the following types:
- boolean
- decimal
- double
- float
- int32
- long
- text (a no-brainer)

There's also a class you can use to create an **ITextConverter** for a
specific type of **enum**. It's called **TextToEnum** and it's used like
this:
```
public enum PlainEnum
{
    A,
    B,
    C
}

[Flags]
public enum FlagsEnum
{
    A = 1 << 0,
    B = 1 << 1,
    C = 1 << 3,

    None = 0,
    All = A | B | C
}

public class PlainEnumConverter : TextToEnum<PlainEnum>
{
}

public class FlagsEnumConverter : TextToEnum<FlagsEnum>
{
}
```
Adding your own **ITextConverter** is pretty simple. You do it by 
deriving from **TextConverter**, an abstract base class implementation of
**ITextConverter**. Here's the code for the *int* converter:
```
public class TextToInt : TextConverter<int>
{
    public override bool Convert( string value, out int result )
    {
        if( int.TryParse( value, out var innerResult ) )
        {
            result = innerResult;
            return true;
        }

        result = default;

        return false;
    }
}
```
Once you've defined your **ITextConverter** add-on you just need to register
it with the dependency injection framework. For how to do that using 
the **Autofac** add-on library see [this article](di.md).
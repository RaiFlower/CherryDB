using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CherryDB.Source.MetaCommandSystem;

public abstract class OptionalArgument : Argument
{
    protected OptionalArgument(string name) : base(name) { }
}

public class OptionalArgument<T> : OptionalArgument
{
    public override Type Type => typeof(T);

    public override string Signature => $"{App.OptionalArgumentPrefix}{Name}={Default}";

    private readonly T? _defaultValue;
    private readonly Func<T>? _defaultExpression;

    public T Default => _defaultValue ?? _defaultExpression!.Invoke();


    public OptionalArgument(string name, T defaultValue) : base(name)
    {
        _defaultValue = defaultValue;
    }

    public OptionalArgument(string name, Func<T> defaultExpression) : base(name)
    {
        _defaultExpression = defaultExpression;
    }
}
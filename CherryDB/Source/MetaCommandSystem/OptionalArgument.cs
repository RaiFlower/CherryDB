using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CherryDB.Source.MetaCommandSystem;

public class OptionalArgument
{
    public readonly string Name;

    public OptionalArgument(string name)
    {
        Name = name;
    }
}

public class OptionalArgument<T> : OptionalArgument
{
    public readonly Type Type;

    private readonly T? _defaultValue;
    private readonly Func<T>? _defaultExpression;

    public T Default => _defaultValue ?? _defaultExpression!.Invoke();

    public OptionalArgument(string name, T defaultValue) : base(name)
    {
        Type = typeof(T);
        _defaultValue = defaultValue;
    }

    public OptionalArgument(string name, Func<T> defaultExpression) : base(name)
    {
        Type = typeof(T);
        _defaultExpression = defaultExpression;
    }
}
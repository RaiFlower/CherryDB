using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CherryDB.Source.MetaCommandSystem;

public class RequiredArgument
{
    public readonly string Name;

    public RequiredArgument(string name)
    {
        Name = name;
    }
}

public class RequiredArgument<T> : RequiredArgument
{
    public readonly Type Type;

    public RequiredArgument(string name) : base(name)
    {
        Type = typeof(T);
    }
}
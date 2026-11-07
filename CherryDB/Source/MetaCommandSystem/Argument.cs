using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CherryDB.Source.MetaCommandSystem;

public abstract class Argument
{
    public readonly string Name;

    public abstract Type Type { get; }

    public abstract string Signature { get; }

    protected Argument(string name)
    {
        Name = name;
    }
}
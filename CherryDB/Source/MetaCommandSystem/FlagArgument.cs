using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CherryDB.Source.MetaCommandSystem;

public class FlagArgument
{
    public readonly string Name;

    public FlagArgument(string name)
    {
        Name = name;
    }
}
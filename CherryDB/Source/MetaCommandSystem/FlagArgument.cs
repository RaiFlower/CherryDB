using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CherryDB.Source.MetaCommandSystem;

public class FlagArgument : Argument
{
    public override Type Type => typeof(bool);

    public override string Signature => $"{App.FlagArgumentPrefix}{Name}";

    public FlagArgument(string name) : base(name)
    {
        
    }
}
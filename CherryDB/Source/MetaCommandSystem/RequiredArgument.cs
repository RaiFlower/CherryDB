using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CherryDB.Source.MetaCommandSystem;

public abstract class RequiredArgument : Argument
{
    protected RequiredArgument(string name) : base(name) { }
}

public class RequiredArgument<T> : RequiredArgument
{
    public override Type Type => typeof(T);

    public override string Signature => $"<{Name}>";

    public RequiredArgument(string name) : base(name) { }
}
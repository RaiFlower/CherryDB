using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CherryDB.Source.MetaCommandSystem;

public class MetaCommand
{
    public string Name = null!;

    public List<RequiredArgument> RequiredArguments = new();
    public List<FlagArgument> FlagArguments = new();
    public List<OptionalArgument> OptionalArguments = new();

    public Action<MetaCommandArguments> Action = null!;

    public string Signature = null!;
    public string Description = null!;

    public MetaCommand SetName(string name)
    {
        Name = name;
        return this;
    }

    public MetaCommand SetRequiredArguments(List<RequiredArgument> requiredArguments)
    {
        RequiredArguments = requiredArguments;
        return this;
    }

    public MetaCommand SetFlagArguments(List<FlagArgument> flagArguments)
    {
        FlagArguments = flagArguments;
        return this;
    }

    public MetaCommand SetOptionalArguments(List<OptionalArgument> optionalArguments)
    {
        OptionalArguments = optionalArguments;
        return this;
    }

    public MetaCommand SetAction(Action<MetaCommandArguments> action)
    {
        Action = action;
        return this;
    }

    public MetaCommand SetDescription(string description)
    {
        Description = description;
        return this;
    }

    public MetaCommand Build()
    {
        StringBuilder builder = new();

        builder.Append($"{Name} ");

        foreach (RequiredArgument argument in RequiredArguments)
        {
            builder.Append($"<{argument.Name}> ");
        }

        foreach (FlagArgument argument in FlagArguments)
        {
            builder.Append($"{argument.Signature} ");
        }

        foreach (OptionalArgument argument in OptionalArguments)
        {
            builder.Append(argument.Signature );
        }

        Signature = builder.ToString();

        return this;
    }
}
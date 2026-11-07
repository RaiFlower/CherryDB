using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CherryDB.Source.MetaCommandSystem;

public class MetaCommandArguments
{
    private readonly MetaCommand _metaCommand;

    private readonly Dictionary<string, object> _requiredArguments = new();
    private readonly List<string> _flagArguments = new();
    private readonly Dictionary<string, object> _optionalArguments = new();

    public MetaCommandArguments(MetaCommand metaCommand)
    {
        _metaCommand = metaCommand;
    }

    public void Parse(string[] argumentWords)
    {
        for (int i = 0; i < argumentWords.Length; i++)
        {
            string word = argumentWords[i];

            if (!word.StartsWith(App.FlagArgumentPrefix) && !word.StartsWith(App.OptionalArgumentPrefix))
            {
                if (_flagArguments.Count > 0)
                {
                    throw new Exception("Error parsing flag arguments.");
                }

                if (_optionalArguments.Count > 0)
                {
                    throw new Exception("Error parsing optional arguments.");
                }

                if (i >= _metaCommand.RequiredArguments.Count)
                {
                    throw new Exception("Error parsing required arguments.");
                }

                _requiredArguments.Add(_metaCommand.RequiredArguments[i].Name, word);
            }
            else if (word.StartsWith(App.FlagArgumentPrefix) && !word.StartsWith(App.OptionalArgumentPrefix))
            {
                string name = word[App.FlagArgumentPrefix.Length..];

                if (!_metaCommand.FlagArguments.Any(a => a.Name == name))
                {
                    throw new Exception("Error parsing flag arguments.");
                }

                _flagArguments.Add(word[App.FlagArgumentPrefix.Length..]);
            }
            else if (word.StartsWith(App.OptionalArgumentPrefix))
            {
                string name = word[App.OptionalArgumentPrefix.Length..];

                if (!_metaCommand.OptionalArguments.Any(a => a.Name == name))
                {
                    throw new Exception("Error parsing optional arguments.");
                }

                if (i + 1 >= argumentWords.Length)
                {
                    throw new Exception("Error parsing optional arguments.");
                }
                
                if (argumentWords[i + 1].StartsWith(App.FlagArgumentPrefix) || argumentWords[i + 1].StartsWith(App.OptionalArgumentPrefix))
                {
                    throw new Exception("Error parsing optional arguments.");
                }
               
                _optionalArguments.Add(name, argumentWords[i + 1]);
                i += 1;
            }
        }

        if (_requiredArguments.Count() != _metaCommand.RequiredArguments.Count())
        {
            throw new Exception("Error parsing required arguments.");
        }
    }

    public T GetRequired<T>(string name)
    {
        return (T)Convert.ChangeType(_requiredArguments[name], typeof(T), CultureInfo.InvariantCulture);
    }

    public bool GetFlag(string name)
    {
        return _flagArguments.Contains(name);
    }

    public T GetOptional<T>(string name)
    {
        if (_optionalArguments.ContainsKey(name))
        {
            return (T)Convert.ChangeType(_optionalArguments[name], typeof(T), CultureInfo.InvariantCulture);
        }
        else
        {
            return (_metaCommand.OptionalArguments.First(a => a.Name == name) as OptionalArgument<T>)!.Default;
        }
    }
}
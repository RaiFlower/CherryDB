using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CherryDB.Source.MetaCommandSystem;

namespace CherryDB.Source;

public class App : IDisposable
{
    private bool _isRunning;

    private readonly string _dbFolderPath = Path.Join(AppContext.BaseDirectory, "db");
    private string _savedQueriesFolderPath = null!;

    public const string MetaCommandPrefix = "!";
    public const string SavedQueryPrefix = "?";

    public const string FlagArgumentPrefix = "-";
    public const string OptionalArgumentPrefix = "--";

    private readonly Connection _connection = new();

    private readonly ANSIFixer _ANSIFixer = new();
    private readonly Printer _printer = new();

    private readonly Dictionary<string, MetaCommand> _metaCommands = new();

    public void Start()
    {
        _ANSIFixer.EnableANSI();

        Connect();

        _isRunning = true;
        Run();
    }

    private void Run()
    {
        while (_isRunning)
        {
            string? input = ReadInput();
            Console.WriteLine();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine();
                continue;
            }

            if (input.StartsWith(MetaCommandPrefix))
            {
                ExecuteMetaCommand(input[1..]);
            }
            else if (input.StartsWith(SavedQueryPrefix))
            {
                ExecuteSavedQuery(input[1..]);
            }
            else
            {
                ExecuteRawSQL(input);
            }
            Console.WriteLine();
        }

        End();
    }

    private void End()
    {
        _connection.Dispose();
    }

    public void Dispose()
    {
        End();
    }

    private string? ReadInput()
    {
        Console.Write("> ");
        return Console.ReadLine();
    }

    private void Connect()
    {
        while (true)
        {
            Console.WriteLine("Enter connection string: \n");
            string? input = ReadInput();
            Console.WriteLine();

            if (string.IsNullOrEmpty(input))
            {
                continue;
            }

            try
            {
                _connection.Open(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                continue;
            }

            break;
        }

        Console.Clear();

        Console.WriteLine($"Connected to \"{_connection.Name}\".");
        Console.WriteLine();

        _savedQueriesFolderPath = Path.Join(_dbFolderPath, _connection.Name);
        Directory.CreateDirectory(_savedQueriesFolderPath);

        InitMetaCommands();
    }

    private void ExecuteMetaCommand(string command)
    {
        string[] commandWords = command.Split();

        string commandName = commandWords[0];

        if (_metaCommands.TryGetValue(commandName, out MetaCommand? metaCommand))
        {
            try
            {
                MetaCommandArguments metaCommandArguments = new(metaCommand);
                metaCommandArguments.Parse(commandWords[1..]);
                metaCommand.Action.Invoke(metaCommandArguments);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        else
        {
            Console.WriteLine($"No meta-command named \"{commandName}\" found.");
        }
    }

    private void ExecuteSavedQuery(string command)
    {
        try
        {
            string query = File.ReadAllText(Path.Join(_savedQueriesFolderPath, $"{command}.sql"));
            ExecuteRawSQL(query);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void ExecuteRawSQL(string command)
    {
        try
        {
            _connection.ExecuteSQL(command, _printer.PrintTable);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void InitMetaCommands()
    {
        // RegisterMetaCommand(new MetaCommand()
        //     .SetName("print")
        //     .SetRequiredArguments([
        //         new RequiredArgument<string>("text")
        //     ])
        //     .SetFlagArguments([
        //         new FlagArgument("mark")
        //     ])
        //     .SetOptionalArguments([
        //         new OptionalArgument<string>("prefix", "")
        //     ])
        //     .SetAction((args) => Console.WriteLine(args.GetOptional<string>("prefix") + args.GetRequired<string>("text") + (args.GetFlag("mark") ? "!" : "")))
        // );

        RegisterMetaCommand(new MetaCommand()
            .SetName("exit")
            .SetDescription("Exits application.")
            .SetAction((args) => _isRunning = false)
            .Build()
        );

        RegisterMetaCommand(new MetaCommand()
            .SetName("clear")
            .SetDescription("Clears console.")
            .SetAction((args) =>
            {
                Console.Clear();
                Console.Write("\x1b[3J");
                Console.WriteLine("Console cleared.");
            })
            .Build()
        );

        RegisterMetaCommand(new MetaCommand()
            .SetName("commands")
            .SetDescription("Prints all meta-commands.")
            .SetAction((args) =>
            {
                foreach (string name in _metaCommands.Keys)
                {
                    Console.WriteLine(name);
                }
            })
            .Build()
        );

        RegisterMetaCommand(new MetaCommand()
            .SetName("command")
            .SetDescription("Prints meta-command's signature and description.")
            .SetRequiredArguments([
                new RequiredArgument<string>("command")
            ])
            .SetAction((args) =>
            {
                string commandName = args.GetRequired<string>("command");

                if (_metaCommands.TryGetValue(commandName, out MetaCommand? command))
                {
                    Console.WriteLine($"""
                    Signature: {command.Signature}
                    Description: {command.Description}
                    """);
                }
                else
                {
                    Console.WriteLine($"No meta-command named \"{commandName}\" found.");
                }
            })
            .Build()
        );

        RegisterMetaCommand(new MetaCommand()
            .SetName("tables")
            .SetDescription("Prints all tables with columns names.")
            .SetAction((args) => PrintTablesInfo())
            .Build()
        );

        RegisterMetaCommand(new MetaCommand()
            .SetName("views")
            .SetDescription("Prints all views with columns names.")
            .SetAction((args) => PrintViewsInfo())
            .Build()
        );

        RegisterMetaCommand(new MetaCommand()
            .SetName("table")
            .SetDescription("Prints columns of a table.")
            .SetRequiredArguments([
                new RequiredArgument<string>("table")
            ])
            .SetAction((args) => PrintTableInfo(args.GetRequired<string>("table")))
            .Build()
        );

        RegisterMetaCommand(new MetaCommand()
            .SetName("view")
            .SetDescription("Prints columns of a view.")
            .SetRequiredArguments([
                new RequiredArgument<string>("view")
            ])
            .SetAction((args) => PrintViewInfo(args.GetRequired<string>("view")))
            .Build()
        );

        RegisterMetaCommand(new MetaCommand()
            .SetName("print")
            .SetDescription("Prints rows of a table or a view.")
            .SetRequiredArguments([
                new RequiredArgument<string>("table|view")
            ])
            .SetFlagArguments([
                new FlagArgument("all"),
                new FlagArgument("desc")
            ])
            .SetOptionalArguments([
                new OptionalArgument<int>("limit", 20)
            ])
            .SetAction((args) =>
            {
                if (!args.GetFlag("all"))
                {
                    Console.WriteLine($"{(args.GetFlag("desc") ? "Bottom" : "Top")} {args.GetOptional<int>("limit")} rows: ");
                    Console.WriteLine();
                }

                ExecuteRawSQL($"""
                    SELECT * FROM {args.GetRequired<string>("table|view")} 
                    {(args.GetFlag("desc") ? "ORDER BY 1 DESC" : "")}
                    {(args.GetFlag("all") ? "" : $"LIMIT {args.GetOptional<int>("limit")}")}
                """);
            })
            .Build()
        );
    }

    private void RegisterMetaCommand(MetaCommand metaCommand)
    {
        _metaCommands.Add(metaCommand.Name, metaCommand);
    }

    private void PrintTablesInfo()
    {
        ExecuteRawSQL("""
            SELECT
                table_name,
                string_agg(column_name, ', ' ORDER BY ordinal_position) AS table_columns
            FROM
                information_schema.columns
            WHERE
                table_schema NOT IN ('pg_catalog', 'information_schema')
            GROUP BY
                table_schema,
                table_name
            ORDER BY
                table_schema,
                table_name;
        """);
    }

    private void PrintViewsInfo()
    {
        ExecuteRawSQL("""
            SELECT
                v.table_name AS view_name,
                string_agg(c.column_name, ', ' ORDER BY c.ordinal_position) AS view_columns
            FROM
                information_schema.views v
                JOIN information_schema.columns c
                    ON c.table_schema = v.table_schema
                AND c.table_name = v.table_name
            WHERE
                v.table_schema NOT IN ('pg_catalog', 'information_schema')
            GROUP BY
                v.table_schema,
                v.table_name
            ORDER BY
                v.table_schema,
                v.table_name;
        """);
    }

    private void PrintTableInfo(string name)
    {
        ExecuteRawSQL($"""
            SELECT
                table_name,
                string_agg(column_name, ', ' ORDER BY ordinal_position) AS table_columns
            FROM
                information_schema.columns
            WHERE
                table_schema NOT IN ('pg_catalog', 'information_schema')
                AND table_name = '{name}'
            GROUP BY
                table_schema,
                table_name
            ORDER BY
                table_schema,
                table_name;
        """);
    }

    private void PrintViewInfo(string name)
    {
        ExecuteRawSQL($"""
            SELECT
                v.table_name AS view_name,
                string_agg(c.column_name, ', ' ORDER BY c.ordinal_position) AS view_columns
            FROM
                information_schema.views v
                JOIN information_schema.columns c
                    ON c.table_schema = v.table_schema
                AND c.table_name = v.table_name
            WHERE
                v.table_schema NOT IN ('pg_catalog', 'information_schema')
                AND v.table_name = '{name}'
            GROUP BY
                v.table_schema,
                v.table_name
            ORDER BY
                v.table_schema,
                v.table_name;
        """);
    }
}
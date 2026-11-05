using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CherryDB.Source;

public class App : IDisposable
{
    private bool _isRunning;

    private readonly string _dbFolderPath = Path.Join(AppContext.BaseDirectory, "db");
    private string _savedQueriesFolderPath = null!;

    private readonly string _metaCommandPrefix = "!";
    private readonly string _savedQueryPrefix = "?";

    private readonly Connection _connection = new();

    private readonly ANSIFixer _ANSIFixer = new();
    private readonly Printer _printer = new();

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

            if (input.StartsWith(_metaCommandPrefix))
            {
                ExecuteMetaCommand(input[1..]);
            }
            else if (input.StartsWith(_savedQueryPrefix))
            {
                ExecuteSavedQuery(input[1..]);
            }
            else
            {
                ExecuteRawSQL(input);
            }
            Console.WriteLine();
        }
    }

    private void End()
    {
        _isRunning = false;

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
    }

    private void ExecuteMetaCommand(string command)
    {
        
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
            Console.WriteLine();
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
            Console.WriteLine();
        }
    }
}
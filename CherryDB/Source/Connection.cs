using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace CherryDB.Source;

public class Connection : IDisposable
{
    private NpgsqlConnection _connection = null!;

    public string Name => _connection.Database;

    public void Open(string connectionString)
    {
        _connection = new(connectionString);
        _connection.Open();
    }

    public void Close()
    {
        _connection.Close();
    }

    public void Dispose()
    {
        Close();
    }

    public void ExecuteSQL(string sql, Action<DbDataReader> printAction)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        using var command = new NpgsqlCommand(sql, _connection);
        using var reader = command.ExecuteReader();

        do
        {
            if (reader.FieldCount > 0)
            {
                printAction(reader);
                Console.WriteLine();
            }
        }
        while (reader.NextResult());

        bool needsLine = false;

        if (reader.RecordsAffected > 0)
        {
            needsLine = true;
            Console.WriteLine($"Rows affected: {reader.RecordsAffected}.");
        }

        if (needsLine)
        {
            Console.WriteLine();
        }
        Console.WriteLine($"Query performed in {stopwatch.ElapsedMilliseconds} ms.");
    }
}
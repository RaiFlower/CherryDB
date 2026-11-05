using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CherryDB.Source;

public class Printer
{
    public void PrintTable(DbDataReader reader)
    {
        int padding = 5;
        int cols = reader.FieldCount;

        var headers = new string[cols];
        var maxWidths = new int[cols];
        for (int i = 0; i < cols; i++)
        {
            headers[i] = reader.GetName(i);
            maxWidths[i] = headers[i].Length;
        }

        // Pass 1: read everything into strings and track max width per column
        var rows = new List<string[]>();
        while (reader.Read())
        {
            var row = new string[cols];
            for (int i = 0; i < cols; i++)
            {
                row[i] = ReadValue(reader, i);
                if (row[i].Length > maxWidths[i])
                {
                    maxWidths[i] = row[i].Length;
                }
            }
            rows.Add(row);
        }

        // Pass 2: draw (single write via StringBuilder)
        var sb = new StringBuilder();

        for (int i = 0; i < cols; i++)
        {
            sb.Append("\u001b[4m")
              .Append(headers[i].PadRight(maxWidths[i] + padding))
              .Append("\u001b[0m");
        }
        sb.AppendLine();

        foreach (var row in rows)
        {
            for (int i = 0; i < cols; i++)
            {
                sb.Append(row[i].PadRight(maxWidths[i] + padding));
            }
            sb.AppendLine();
        }
        
        Console.Write(sb.ToString());
    }

    private static string ReadValue(DbDataReader reader, int i)
    {
        if (reader.IsDBNull(i))
            return Format(null);

        object value = reader.GetValue(i);
        return Format(value);
    }

    private static string Format(object? value) => value switch
    {
        null => "null",
        bool flag => flag ? "true" : "false",
        float f => f.ToString("0.00", CultureInfo.InvariantCulture),
        double d => d.ToString("0.00", CultureInfo.InvariantCulture),
        decimal m => m.ToString(CultureInfo.InvariantCulture),
        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        byte[] bytes => $"<{bytes.Length} bytes>",
        Array array => "{" + string.Join(",", array.Cast<object?>().Select(Format)) + "}",
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? ""
    };
}
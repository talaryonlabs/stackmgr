using System.CommandLine;

namespace Talaryon.StackManager;

public static class HelperMethods
{
    public static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void LogSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void LogInfo(string message)
    {
        Console.WriteLine(message);
    }

    public static bool ConfirmWarning(string question)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{question} [y/N]: ");
        var answer = Console.ReadLine()?.ToLower() == "y";
        Console.ResetColor();
        return answer;
    }
    
    public static void PrintTable(List<string[]> rows)
    {
        if (rows == null || rows.Count == 0)
        {
            Console.WriteLine("No data to display.");
            return;
        }

        // Calculate maximum width for each column
        int[] columnWidths = new int[rows[0].Length];
        for (int col = 0; col < columnWidths.Length; col++)
        {
            columnWidths[col] = rows.Max(row => row[col]?.Length ?? 0);
        }

        // Helper to print a separator line
        void PrintSeparator()
        {
            Console.WriteLine("+" + string.Join("+", columnWidths.Select(w => new string('-', w + 2))) + "+");
        }

        // Print table
        PrintSeparator();
        for (int i = 0; i < rows.Count; i++)
        {
            Console.WriteLine("| " + string.Join(" | ", rows[i].Select((cell, col) =>
                (cell ?? "").PadRight(columnWidths[col]))) + " |");

            // Print separator after header row
            if (i == 0) PrintSeparator();
        }
        PrintSeparator();
    }

    public static string GetSymbolName<T>() where T : Symbol => Activator.CreateInstance<T>().Name;
    
    public static string HostToName(string host) => host.Replace(".", "-");
}
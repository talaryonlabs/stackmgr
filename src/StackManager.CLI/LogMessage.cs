using System.Collections.Immutable;

namespace Talaryon.StackManager;

public static class LogMessage
{
    public static void AsError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void AsSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void AsWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void AsInfo(string message)
    {
        Console.WriteLine(message);
    }

    public static bool AsConfirmInfo(string message)
    {
        return LogBuilder.Question(message)
            .AsYesNo()
            .Run();
    }
    
    public static bool AsConfirmWarning(string message)
    {
        return LogBuilder.Question(message)
            .AsYesNo()
            .AsWarning()
            .Run();
    }
}
namespace Talaryon.StackManager;

public static class LogMessage
{
    // Basic color methods (now using LogBuilder internally)
    public static void AsError(string message)
    {
        LogBuilder.Message(message).AsError().Run();
    }
    
    public static void AsSuccess(string message)
    {
        LogBuilder.Message(message).AsSuccess().Run();
    }
    
    public static void AsWarning(string message)
    {
        LogBuilder.Message(message).AsWarning().Run();
    }
    
    public static void AsInfo(string message)
    {
        LogBuilder.Message(message).Run();
    }

    // Enhanced confirmation methods
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
    
    public static bool AsConfirmError(string message)
    {
        return LogBuilder.Question(message)
            .AsYesNo()
            .AsError()
            .Run();
    }

    // New meaningful methods
    public static void AsCustom(string message, ConsoleColor color)
    {
        LogBuilder.Message(message).AsColored(color).Run();
    }
    
    public static void AsBoxed(string message)
    {
        LogBuilder.Message(message).InBox().Run();
    }
    
    public static void AsBoxed(string message, ConsoleColor color)
    {
        LogBuilder.Message(message).InBox().AsColored(color).Run();
    }
    
    public static void AsTimestamped(string message)
    {
        LogBuilder.Message(message).WithTimestamp().Run();
    }
    
    public static void AsIndented(string message, int level)
    {
        LogBuilder.Message(message).Indented(level).Run();
    }
    
    // Separator methods
    public static void Separator(char c = '-', int length = 40)
    {
        Console.WriteLine(new string(c, length));
    }
    
    public static void Separator(string title, char c = '=', int length = 40)
    {
        int padding = (length - title.Length - 2) / 2;
        string left = new string(c, padding);
        string right = new string(c, length - title.Length - 2 - padding);
        Console.WriteLine($"{left} {title} {right}");
    }
}
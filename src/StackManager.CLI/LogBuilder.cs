using Talaryon.Toolbox;
using Talaryon.Toolbox.Extensions;

namespace Talaryon.StackManager;

public interface ILogBuilder<out T>
{
    T AsError();
    T AsWarning();
    T AsSuccess();
    T AsColored(ConsoleColor color);
    T ResetFormatting();
    T WithPrefix(string prefix);
    T WithSuffix(string suffix);
    T WithTimestamp();
    T Indented(int level);
    T InBox();
    T NoNewLineAfter();
    T NewLineBefore();
}

public interface ILogBuilderMessage : ITalaryonRunner, ILogBuilder<ILogBuilderMessage>
{
    ILogBuilderMessage WaitFor(Func<ILogBuilderMessage> predicate);
    ILogBuilderMessage WaitFor(Func<Task<ILogBuilderMessage>> predicate);
}

public interface ILogBuilderQuestion : ITalaryonRunner<bool>, ILogBuilder<ILogBuilderQuestion>
{
    ILogBuilderQuestion AsYesNo();
    ILogBuilderQuestion WaitFor(Func<bool, ILogBuilderMessage> predicate);
    ILogBuilderQuestion WaitFor(Func<bool, Task<ILogBuilderMessage>> predicate);
}

public class LogBuilder(string content) : ILogBuilderMessage, ILogBuilderQuestion
{
    public static ILogBuilderMessage Message(string message) => new LogBuilder(message);
    public static ILogBuilderQuestion Question(string question) => new LogBuilder(question);

    private bool _noNewLineAfter, _newLineBefore, _asError, _asWarning, _asSuccess, _asYesNo, _answer, _useCustomColor, _useTimestamp, _inBox;
    private ConsoleColor _customColor;
    private int _indentLevel;
    private string? _prefix, _suffix;
    private Func<ILogBuilderMessage>? _messageFunction;
    private Func<Task<ILogBuilderMessage>>? _messageAsyncFunction;
    private Func<bool, ILogBuilderMessage>? _questionFunction;
    private Func<bool, Task<ILogBuilderMessage>>? _questionAsyncFunction;
    
    private string _content = content;

    void ITalaryonRunner.Run()
    {
        string finalContent = _content;
        
        // Apply timestamp
        if (_useTimestamp)
        {
            finalContent = $"[{DateTime.Now:HH:mm:ss}] {finalContent}";
        }
        
        // Apply indentation
        if (_indentLevel > 0)
        {
            finalContent = new string(' ', _indentLevel * 2) + finalContent;
        }
        
        // Apply prefix/suffix
        if (_prefix != null) finalContent = _prefix + finalContent;
        if (_suffix != null) finalContent = finalContent + _suffix;
        
        // Apply box if requested
        if (_inBox)
        {
            int boxWidth = finalContent.Length + 4;
            string border = new string('─', boxWidth);
            finalContent = $"┌{border}┐\n│ {finalContent.PadRight(boxWidth - 3)} │\n└{border}┘";
        }

        // Set color
        if (_useCustomColor)
        {
            Console.ForegroundColor = _customColor;
        }
        else
        {
            if (_asError) Console.ForegroundColor = ConsoleColor.Red;
            if (_asWarning) Console.ForegroundColor = ConsoleColor.Yellow;
            if (_asSuccess) Console.ForegroundColor = ConsoleColor.Green;
        }

        if (_newLineBefore) Console.WriteLine();
        if (_noNewLineAfter)
        {
            Console.Write(finalContent);
        }
        else
        {
            Console.WriteLine(finalContent);
        }
        Console.ResetColor();

        _messageFunction?
            .Invoke()
            .Run();
        
        _messageAsyncFunction?
            .Invoke()
            .RunSynchronouslyWithResult()
            .Run();
    }
    
    bool ITalaryonRunner<bool>.Run()
    {
        string finalContent = _content;
        
        // Apply timestamp
        if (_useTimestamp)
        {
            finalContent = $"[{DateTime.Now:HH:mm:ss}] {finalContent}";
        }
        
        // Apply indentation
        if (_indentLevel > 0)
        {
            finalContent = new string(' ', _indentLevel * 2) + finalContent;
        }
        
        // Apply prefix/suffix
        if (_prefix != null) finalContent = _prefix + finalContent;
        if (_suffix != null) finalContent = finalContent + _suffix;
        
        // Apply box if requested
        if (_inBox)
        {
            int boxWidth = finalContent.Length + 4;
            string border = new string('─', boxWidth);
            finalContent = $"┌{border}┐\n│ {finalContent.PadRight(boxWidth - 3)} │\n└{border}┘";
        }

        // Set color
        if (_useCustomColor)
        {
            Console.ForegroundColor = _customColor;
        }
        else
        {
            if (_asError) Console.ForegroundColor = ConsoleColor.Red;
            if (_asWarning) Console.ForegroundColor = ConsoleColor.Yellow;
            if (_asSuccess) Console.ForegroundColor = ConsoleColor.Green;
        }

        if (_asYesNo)
        {
            finalContent += " [y/N]: ";
        }
        
        if (_newLineBefore) Console.WriteLine();
        if (_noNewLineAfter)
        {
            Console.Write(finalContent);
        }
        else
        {
            Console.WriteLine(finalContent);
        }
        Console.ResetColor();

        if (_asYesNo)
        {
            _answer = Console.ReadLine()?.ToLower() == "y";
        }

        _questionFunction?
            .Invoke(_answer)
            .Run();
        
        _questionAsyncFunction?
            .Invoke(_answer)
            .RunSynchronouslyWithResult()
            .Run();

        return _answer;
    }

    Task<bool> ITalaryonRunner<bool>.RunAsync(CancellationToken cancellationToken) =>
        Task.Run(() => (this as ITalaryonRunner<bool>).Run(), cancellationToken);

    Task ITalaryonRunner.RunAsync(CancellationToken cancellationToken) =>
        Task.Run(() => (this as ITalaryonRunner).Run(), cancellationToken);

    ILogBuilderMessage ILogBuilder<ILogBuilderMessage>.AsError()
    {
        _asError = true;
        return this;
    }

    ILogBuilderMessage ILogBuilder<ILogBuilderMessage>.AsWarning()
    {
        _asWarning = true;
        return this;
    }

    ILogBuilderMessage ILogBuilder<ILogBuilderMessage>.AsSuccess()
    {
        _asSuccess = true;
        return this;
    }

    ILogBuilderMessage ILogBuilder<ILogBuilderMessage>.AsColored(ConsoleColor color)
    {
        _customColor = color;
        _useCustomColor = true;
        return this;
    }

    ILogBuilderMessage ILogBuilder<ILogBuilderMessage>.NoNewLineAfter()
    {
        _noNewLineAfter = true;
        return this;
    }

    ILogBuilderMessage ILogBuilder<ILogBuilderMessage>.NewLineBefore()
    {
        _newLineBefore = true;
        return this;
    }

    ILogBuilderMessage ILogBuilder<ILogBuilderMessage>.ResetFormatting()
    {
        _asError = _asWarning = _asSuccess = _useCustomColor = false;
        _useTimestamp = _inBox = false;
        _indentLevel = 0;
        _prefix = _suffix = null;
        _customColor = ConsoleColor.Gray;
        return this;
    }

    ILogBuilderMessage ILogBuilder<ILogBuilderMessage>.WithPrefix(string prefix)
    {
        _prefix = prefix;
        return this;
    }

    ILogBuilderMessage ILogBuilder<ILogBuilderMessage>.WithSuffix(string suffix)
    {
        _suffix = suffix;
        return this;
    }

    ILogBuilderMessage ILogBuilder<ILogBuilderMessage>.WithTimestamp()
    {
        _useTimestamp = true;
        return this;
    }

    ILogBuilderMessage ILogBuilder<ILogBuilderMessage>.Indented(int level)
    {
        _indentLevel = Math.Max(0, level);
        return this;
    }

    ILogBuilderMessage ILogBuilder<ILogBuilderMessage>.InBox()
    {
        _inBox = true;
        return this;
    }

    ILogBuilderMessage ILogBuilderMessage.WaitFor(Func<ILogBuilderMessage> predicate)
    {
        _messageFunction = predicate;
        return this;
    }

    ILogBuilderMessage ILogBuilderMessage.WaitFor(Func<Task<ILogBuilderMessage>> predicate)
    {
        _messageAsyncFunction = predicate;
        return this;
    }

    ILogBuilderQuestion ILogBuilderQuestion.AsYesNo()
    {
        _asYesNo = true;
        return this;  
    }

    ILogBuilderQuestion ILogBuilderQuestion.WaitFor(Func<bool, ILogBuilderMessage> predicate)
    {
        _questionFunction = predicate;
        return this;   
    }

    ILogBuilderQuestion ILogBuilderQuestion.WaitFor(Func<bool, Task<ILogBuilderMessage>> predicate)
    {
        _questionAsyncFunction = predicate;
        return this;
    }

    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.AsError() => (ILogBuilderQuestion)(this as ILogBuilderMessage).AsError();
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.AsWarning() => (ILogBuilderQuestion)(this as ILogBuilderMessage).AsWarning();
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.AsSuccess() => (ILogBuilderQuestion)(this as ILogBuilderMessage).AsSuccess();
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.AsColored(ConsoleColor color) => (ILogBuilderQuestion)(this as ILogBuilderMessage).AsColored(color);
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.ResetFormatting() => (ILogBuilderQuestion)(this as ILogBuilderMessage).ResetFormatting();
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.WithPrefix(string prefix) => (ILogBuilderQuestion)(this as ILogBuilderMessage).WithPrefix(prefix);
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.WithSuffix(string suffix) => (ILogBuilderQuestion)(this as ILogBuilderMessage).WithSuffix(suffix);
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.WithTimestamp() => (ILogBuilderQuestion)(this as ILogBuilderMessage).WithTimestamp();
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.Indented(int level) => (ILogBuilderQuestion)(this as ILogBuilderMessage).Indented(level);
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.InBox() => (ILogBuilderQuestion)(this as ILogBuilderMessage).InBox();
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.NoNewLineAfter() => (ILogBuilderQuestion)(this as ILogBuilderMessage).NoNewLineAfter();
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.NewLineBefore() => (ILogBuilderQuestion)(this as ILogBuilderMessage).NewLineBefore();
}
using Talaryon.Toolbox;
using Talaryon.Toolbox.Extensions;

namespace Talaryon.StackManager;

public interface ILogBuilder<out T>
{
    T AsError();
    T AsWarning();
    T AsSuccess();
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

    private bool _noNewLineAfter, _newLineBefore, _asError, _asWarning, _asSuccess, _asYesNo, _answer;
    private Func<ILogBuilderMessage>? _messageFunction;
    private Func<Task<ILogBuilderMessage>>? _messageAsyncFunction;
    private Func<bool, ILogBuilderMessage>? _questionFunction;
    private Func<bool, Task<ILogBuilderMessage>>? _questionAsyncFunction;
    
    private string _content = content;

    void ITalaryonRunner.Run()
    {
        if (_asError)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        
        if(_asWarning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }

        if (_newLineBefore) Console.WriteLine();
        if (_noNewLineAfter)
        {
            Console.Write(_content);
        }
        else
        {
            Console.WriteLine(_content);
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
        if (_asError) Console.ForegroundColor = ConsoleColor.Red;
        if (_asWarning) Console.ForegroundColor = ConsoleColor.Yellow;
        if (_asSuccess) Console.ForegroundColor = ConsoleColor.Green;

        if (_asYesNo)
        {
            _content += " [y/N]: ";
        }
        
        if (_newLineBefore) Console.WriteLine();
        if (_noNewLineAfter)
        {
            Console.Write(_content);
        }
        else
        {
            Console.WriteLine(_content);
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
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.NoNewLineAfter() => (ILogBuilderQuestion)(this as ILogBuilderMessage).NoNewLineAfter();
    ILogBuilderQuestion ILogBuilder<ILogBuilderQuestion>.NewLineBefore() => (ILogBuilderQuestion)(this as ILogBuilderMessage).NewLineBefore();
}
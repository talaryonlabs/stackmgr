using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Services;
using Talaryon.Toolbox.Api;

namespace Talaryon.StackManager.Commands;

public abstract class BaseCommand : Command
{
    protected ParseResult ParseResult => _parseResult;
    
    private IServiceProvider? _serviceProvider;
    private ParseResult? _parseResult;

    protected BaseCommand(string name, string description) : base(name, description)
    {
        SetAction(async parseResult =>
        {
            _parseResult = parseResult;

            var errorService = GetRequiredService<IErrorService>();
            
            try
            {
                await ExecuteAsync();
            }
            catch (CliException cliEx)
            {
                errorService.LogError(cliEx);
                errorService.SetExitCode(cliEx.ExitCode);
            }
            catch (ApiError apiError)
            {
                errorService.LogError(apiError);
                errorService.SetExitCode(2);
            }
            catch (Exception ex)
            {
                errorService.LogError(ex);
                errorService.SetExitCode(2);
            }
        });
    }

    protected virtual void Execute()
    {
    }

    protected virtual Task ExecuteAsync()
    {
        Execute();
        return Task.CompletedTask;
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        Children.OfType<BaseCommand>()
            .ToList()
            .ForEach(v => v.SetServiceProvider(serviceProvider));
        
        _serviceProvider = serviceProvider;
    }

    protected void Add(IEnumerable<Symbol> symbols)
    {
        foreach(var symbol in symbols)
        {
            switch (symbol)
            {
                case Option option:
                    Add(option);
                    break;
                case Command command:
                    Add(command);
                    break;
                case Argument argument:
                    Add(argument);
                    break;
                default:
                    throw new ArgumentException($"Unsupported symbol type: {symbol.GetType()}");
            }
        }
    }

    protected void UseAutodiscoverCommands(Type type)
    {
        // Auto-discover and add all type implementations
        var commands = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(v => v.BaseType?.IsGenericType == true 
                        && v.BaseType.GetGenericTypeDefinition() == type
                        && !v.IsAbstract)
            .Select(v => (BaseCommand?)Activator.CreateInstance(v))
            .OfType<BaseCommand>()
            .ToList();

        foreach (var instance in commands)
        {
            Add(instance);
        }
    }
    
    protected T GetRequiredService<T>() where T : class
    {
        return _serviceProvider == null
            ? throw new InvalidOperationException("Service provider not configured. Call SetServiceProvider first.")
            : _serviceProvider.GetRequiredService<T>();
    }
    
    protected T? GetService<T>() where T : class
    {
        return _serviceProvider?.GetService<T>();
    }
    
    protected TValue GetRequiredValue<TValue, TSymbol>() where TSymbol : Symbol
    {
        return _parseResult is null ? default : _parseResult.GetRequiredValue<TValue, TSymbol>();
    }
    
    protected TValue? GetValue<TValue, TSymbol>() where TSymbol : Symbol
    {
        return _parseResult is null ? default : _parseResult.GetValue<TValue, TSymbol>();
    }

    protected string GetName<T>() where T : Symbol
    {
        return _parseResult.GetRequiredValue<string, T>().ToLower();
    }
    
    protected StackEnvironment GetEnvironment<T>() where T : Symbol
    {
        var name = GetName<T>();
        var path = Path.Combine(Directory.GetCurrentDirectory(), name, StackEnvironment.FileName);
        var file = new FileInfo(path);
        
        return StackResource.Load<StackEnvironment>(file);
    }
    
    protected Stack GetStack<T>(StackEnvironment env) 
        where T : Symbol
    {
        var name = GetName<T>();
        return env.GetStack(name);
    }
    
    protected StackApp GetApp<T>(Stack stack) 
        where T : Symbol
    {
        var name = GetName<T>();
        return stack.Get<StackApp>(name);
    }
}
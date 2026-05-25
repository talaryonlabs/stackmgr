using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Builder;

public interface IStackEnvironmentBuilder
{
    IStackEnvironmentBuilder WithName(string name);
    IStackEnvironmentBuilder Configure(Action<StackEnvironment> configure);
    StackEnvironment Build();
}

public class StackEnvironmentBuilder : IStackEnvironmentBuilder
{
    private readonly StackEnvironment _environment = new();
    
    public IStackEnvironmentBuilder WithName(string name)
    {
        var directory = new DirectoryInfo(name);

        if (directory.Exists)
            throw new EnvironmentAlreadyExistsException(name);
        
        _environment.Name = name;
        return this;
    }

    public IStackEnvironmentBuilder Configure(Action<StackEnvironment> configure)
    {
        configure(_environment);
        return this;
    }

    public StackEnvironment Build() => string.IsNullOrWhiteSpace(_environment.Name)
        ? throw new ArgumentNullException(nameof(_environment.Name), "Environment name cannot be null or empty.")
        : _environment;
}
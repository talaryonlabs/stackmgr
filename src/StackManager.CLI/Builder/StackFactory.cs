using System.Reflection;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Builder;

public interface IStackFactory<out T>
{
    IStackFactory<T> WithName(string name);
    IStackFactory<T> Configure(Action<T> configure);
    T Save();
}

public class StackFactory<T>(Stack stack) : IStackFactory<T>
    where T : class, IStackObject
{
    private readonly T _object = Activator.CreateInstance<T>();
    private string? _name;
    
    public IStackFactory<T> WithName(string name)
    {
        var list = GetList();
        if (list.Any(v => v.Name == name))
            throw new ResourceAlreadyExistsException<T>(stack, name);
        
        _name = name;
        return this;
    }

    public IStackFactory<T> Configure(Action<T> configure)
    {
        configure(_object);
        return this;
    }

    public T Save()
    {
        if(_name is null)
            throw new InvalidOperationException("Resource name cannot be null");
        
        _object.Name = _name;
        _object.Stack = stack;
        
        var list = GetList();
        list.Add(_object);
        stack.SaveConfig();
        
        return _object;
    }

    private List<T> GetList()
    {
        var list = typeof(Stack)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(v => v.PropertyType == typeof(List<T>))
            .Select(v => v.GetValue(stack))
            .OfType<List<T>>()
            .FirstOrDefault();

        return list ?? throw new InvalidOperationException();
    }
}
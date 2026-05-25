namespace Talaryon.StackManager.Types;

public interface IStackObject
{
    Stack Stack { get; set; }
    string Name { get; set; }
}
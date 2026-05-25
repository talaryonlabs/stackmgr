namespace Talaryon.StackManager.Models;

public interface IStackObject
{
    Stack Stack { get; set; }
    string Name { get; set; }
}
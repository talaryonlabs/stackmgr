using System.CommandLine;

namespace stackmgr;

public class StackManager
{
    public void RegisterListStacks(Command command)
    {
        command.SetAction(async (v, c) =>
        {
            return await Task.Run(() =>
            {
                Console.WriteLine("Listing stacks async");
                return 0;
            }, c);
        });
    }
}
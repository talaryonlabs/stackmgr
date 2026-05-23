using System.CommandLine;

namespace Talaryon.StackManager.Arguments;

public class RemoteArgument : Argument<Uri>
{
    public RemoteArgument() : base("remote-uri")
    {
        Description = "remote url (e.g. https://example.com)";
    }  
}
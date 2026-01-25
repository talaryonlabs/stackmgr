using System.CommandLine;
using System.Net.Http.Json;
using System.Text.Json;
using Talaryon.Toolbox.Services.ArgoCD;

namespace stackmgr.Commands;

public enum ConnectService
{
    RKE2,
    ArgoCD,
    GitHub
}

public class ConnectCommand : Command
{
    public ConnectCommand() : base("connect", "Connect to a service")
    {
        var config = StackMgrConfig.Load();
        
        SetAction(async v =>
        {
            await RKE2.CreateNamespace("stackmgr-test");

        });
    }
}
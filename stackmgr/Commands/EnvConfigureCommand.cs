using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class EnvConfigureCommand : Command
{
    public EnvConfigureCommand() : base("configure", "Configure environment variables")
    {
        Add(new EnvironmentArgument());
        Add(new RKE2AccessTokenOption());
        Add(new RKE2UrlOption());
        Add(new RKE2ProjectIdOption());
        SetAction(v =>
        {
            var config = StackMgrConfig.Load();
            var name = v.GetRequiredValue<string, EnvironmentArgument>().ToLower();
            var env = config.Environments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            
            if (env is null)
            {
                Console.WriteLine($"Environment '{name}' does not exist.");
                return;
            }
            
            var rke2AccessToken = v.GetValue<string, RKE2AccessTokenOption>();
            if (rke2AccessToken is not null)
            {
                env.RKE2.AccessToken = rke2AccessToken;
                Console.WriteLine("RKE2 access token updated.");
            }
            
            var rke2Url = v.GetValue<string, RKE2UrlOption>();
            if (rke2Url is not null)
            {
                env.RKE2.Url = rke2Url;
                Console.WriteLine("RKE2 URL updated.");
            }
            
            var rke2ProjectId = v.GetValue<string, RKE2ProjectIdOption>();
            if (rke2ProjectId is not null)
            {
                env.RKE2.ProjectId = rke2ProjectId;
                Console.WriteLine("RKE2 project ID updated.");
            }
            
            config.Save();
        });
    }
}
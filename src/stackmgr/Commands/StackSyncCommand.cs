using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;
using Talaryon.Toolbox.Services.ArgoCD.Models;

namespace stackmgr.Commands;

public class StackSyncCommand : StackManagerCommand
{
    private StackEnvironment _env;
    private string _stack;
    private StackConfig _config;
    private string? _namespace;
    private V1alpha1Application? _application;
    

    public StackSyncCommand() : base("sync", "Sync a stack")
    {
        SetAction(async v =>
        {
            var name = v.GetRequiredValue<string, EnvironmentOption>().ToLower();
            _env = Config.Environments.FirstOrDefault(x =>
                x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));

            if (_env is null)
            {
                Console.WriteLine($"Environment '{name}' does not exist.");
                return;
            }

            _stack = v.GetRequiredValue<string, StackArgument>();
            if (!_env.HasLocalStack(_stack))
            {
                Console.WriteLine($"Stack '{_stack}' does not exist in environment '{_env.Name}'");
                return;
            }
            
            _config = StackConfig.Load(_env, _stack);
            if (_config is null)
            {
                Console.WriteLine("Failed to get stack configuration. (Unknown error)");
                return;
            }

            
            if ((_namespace = await GetOrCreateNamespaceAsync()) is not null && (_application = await GetOrCreateApplicationAsync()) is not null)
            {
                await SyncAutoSyncSettingAsync();
            }
            
        });
    }

    private async Task<string> GetOrCreateNamespaceAsync()
    {
        var ns = _env.GetStackNamespace(_stack);
        if (!await RKE2.NamespaceExists(_env, ns))
        {
            Console.Write($".. Creating RKE2 namespace '{ns}' .. ");
            await RKE2.CreateNamespace(_env, ns);
            Console.WriteLine("Done.");
        }
        else
        {
            Console.WriteLine(".. RKE2 namespace already exists. (Nothing to do)");
        }
        return ns;
    }

    private async Task<V1alpha1Application?> GetOrCreateApplicationAsync()
    {
        if (_namespace is null) return null;
        
        var application = await ArgoCD.GetApplication(_env, _namespace);
        if (application is not null)
        {
            Console.Write(".. Creating ArgoCD application .. ");
            if (await ArgoCD.CreateApplication(_env, _namespace))
            {
                Console.WriteLine("Done.");
            }
        }
        else
        {
            Console.WriteLine(".. ArgoCD application already exists. (Nothing to do)");
        }
        return application;
    }

    private async Task SyncAutoSyncSettingAsync()
    {
        if (_namespace is null) return;
        if(_application is null) return;
        
        if (_config.AutoSync)
        {
            await ArgoCD.EnableAutoSync(_env, _namespace);
        }
        else
        {
            await ArgoCD.DisableAutoSync(_env, _namespace);
        }
    }
}
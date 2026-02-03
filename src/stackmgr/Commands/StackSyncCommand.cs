using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;
using Talaryon.Toolbox.Services.ArgoCD.Models;

namespace stackmgr.Commands;

public class StackSyncCommand : StackManagerCommand
{
    private StackEnvironment _env;
    private Stack _stack;
    private string? _namespace;
    private V1alpha1Application? _application;
    

    public StackSyncCommand() : base("sync", "Sync a stack")
    {
        SetAction(async v =>
        {
            _env = GetEnvironment<EnvironmentOption>(v);
            _stack = GetStack<StackArgument>(v, _env);
            
            if ((_namespace = await GetOrCreateNamespaceAsync()) is not null && (_application = await GetOrCreateApplicationAsync()) is not null)
            {
                await SyncAutoSyncSettingAsync();
            }
            
        });
    }

    private async Task<string> GetOrCreateNamespaceAsync()
    {
        if (!await RKE2.NamespaceExists(_env, _stack.Namespace))
        {
            Console.Write($".. Creating RKE2 namespace '{_stack.Namespace}' .. ");
            await RKE2.CreateNamespace(_env, _stack.Namespace);
            Console.WriteLine("Done.");
        }
        else
        {
            Console.WriteLine(".. RKE2 namespace already exists. (Nothing to do)");
        }
        return _stack.Namespace;
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
        
        if (_stack.Application.Spec.SyncPolicy is not null)
        {
            await ArgoCD.EnableAutoSync(_env, _namespace);
        }
        else
        {
            await ArgoCD.DisableAutoSync(_env, _namespace);
        }
    }
}
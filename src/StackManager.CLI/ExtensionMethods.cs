using System.CommandLine;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager;

public static class ExtensionMethods
{
    extension(ParseResult parseResult)
    {
        public TValue GetRequiredValue<TValue, TSymbol>() where TSymbol : Symbol 
        {
            var item = Activator.CreateInstance<TSymbol>();
            return parseResult.GetRequiredValue<TValue>(item.Name);
        }

        public TValue? GetValue<TValue, TSymbol>() where TSymbol : Symbol 
        {
            var item = Activator.CreateInstance<TSymbol>();
            return parseResult.GetValue<TValue>(item.Name);
        }
    }

    extension(StackEnvironmentRancher rancher)
    {
        public string GetAccessToken(StackEnvironment env) => LocalConfig
            .Get()
            .Environments
            .FirstOrDefault(x => x.Name == env.Name)?
            .RancherAccessToken ?? "";

        public void SetAccessToken(StackEnvironment env, string accessToken)
        {
            var config = LocalConfig.Get();
            var localEnvironment = config.Environments.FirstOrDefault(v => v.Name == env.Name);
            if (localEnvironment is null)
            {
                localEnvironment = new() { Name = env.Name };
                config.Environments.Add(localEnvironment);
            }
            localEnvironment.RancherAccessToken = accessToken;
            config.Save();
        }
    }

    extension(StackEnvironmentArgo argo)
    {
        public string GetAccessToken(StackEnvironment env) => LocalConfig
            .Get()
            .Environments
            .FirstOrDefault(x => x.Name == env.Name)?
            .ArgoAccessToken ?? "";
        
        public void SetAccessToken(StackEnvironment env, string accessToken)
        {
            var config = LocalConfig.Get();
            var localEnvironment = config.Environments.FirstOrDefault(v => v.Name == env.Name);
            if (localEnvironment is null)
            {
                localEnvironment = new() { Name = env.Name };
                config.Environments.Add(localEnvironment);
            }
            localEnvironment.ArgoAccessToken = accessToken;
            config.Save();
        }
    }
    
}
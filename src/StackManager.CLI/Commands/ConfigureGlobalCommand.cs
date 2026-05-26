using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Validation;

namespace Talaryon.StackManager.Commands;

/// <summary>
/// Command for configuring global settings.
/// </summary>
public class ConfigureGlobalCommand : ResourceConfigureCommand<NameArgument>
{
    public ConfigureGlobalCommand()
        : base("global", "Configure the app repository")
    {
        Add(new AppRepositoryOption());
    }

    protected override void Configure()
    {
        var localConfig = GetRequiredService<LocalConfig>();
        
        var appRepository = GetValue<string, AppRepositoryOption>();
        if (appRepository is not null)
        {
            ValidationHelper.ValidateUrl(appRepository);
            localConfig.AppRepository = appRepository;
            LogMessage.AsSuccess("App repository updated.");
        }
        
        localConfig.Save();
    }
}

using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Commands.Remotes;

public class TestRemoteCommand : BaseCommand
{
    public TestRemoteCommand()
        : base("test", "Test a remote proxy")
    {
        Add(new NameArgument());
    }

    protected override async Task ExecuteAsync()
    {
        var config = GetRequiredService<LocalConfig>();
        var name = GetRequiredValue<string, NameArgument>();
        var remote = config.Remotes.FirstOrDefault(r => r.Name == name);
        if (remote == null)
        {
            LogMessage.AsError($"Remote not found: {name}");
            return;
        }

        var proxy = GetRequiredService<ProxyService>().Remote(remote);
        await LogBuilder.Message($"Testing Connection '{remote.Name}' ...")
            .WaitFor(async () =>
            {
                try
                {
                    if (await proxy.TestConnectionAsync())
                    {
                        return LogBuilder.Message("Done.").AsSuccess();
                    }
                    return LogBuilder.Message("Failed.").AsError();
                }
                catch (Exception ex)
                {
                    return LogBuilder.Message($"Failed: {ex}").AsError();
                }
            })
            .NoNewLineAfter()
            .RunAsync();
    }
}
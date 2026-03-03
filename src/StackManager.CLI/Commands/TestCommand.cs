using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Commands;

public class TestCommand : StackManagerCommand
{
    public TestCommand() : base("test", "Test an environment connection (RKE2, ArgoCD)")
    {
        Add(new EnvironmentArgument());
        SetAction(async parseResult =>
        {
            var env = GetEnvironment<EnvironmentArgument>(parseResult);
            var proxy = new ProxyService(env);

            await LogBuilder.Message($"Testing Connection '{env.Name}' ...")
                .WaitFor(async () =>
                {
                    if (await proxy.TestConnectionAsync())
                    {
                        return LogBuilder.Message("Done.").AsSuccess();
                    }
                    return LogBuilder.Message("Failed.").AsError();
                })
                .NoNewLineAfter()
                .RunAsync();
        });
    }
}
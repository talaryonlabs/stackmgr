using Talaryon.StackManager.Builder;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Commands;

public class BuildCommand : BaseCommand
{
    public BuildCommand() : base("build", "Build a stack")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
    }

    protected override async Task ExecuteAsync()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        var builder = new StackBuilder(stack);
        var kustomizeService = GetRequiredService<IKustomizeService>()
            .Directory(stack.LocalDirectory);

        await LogBuilder.Message("- [Registry Credentials] ... ")
            .NoNewLineAfter()
            .WaitFor(() =>
            {
                builder.BuildRegistryCredentials();
                return Task.FromResult(LogBuilder.Message("Done.").AsSuccess());

            })
            .RunAsync();

        if (stack.Ingresses.Any(v => v.IsSecured))
        {
            await LogBuilder.Message("- [Outpost] ... ")
                .NoNewLineAfter()
                .WaitFor(() =>
                {
                    builder.BuildOutpost();
                    return Task.FromResult(LogBuilder.Message("Done.").AsSuccess());

                })
                .RunAsync();
        }

        await LogBuilder.Message("- [Ingresses] ... ")
            .NoNewLineAfter()
            .WaitFor(() =>
            {
                builder.BuildIngresses();
                return LogBuilder.Message("Done.").AsSuccess();

            })
            .RunAsync();

        await LogBuilder.Message("- [Kustomization] ... ")
            .NoNewLineAfter()
            .WaitFor(() =>
            {
                builder.BuildKustomization();
                return LogBuilder.Message("Done.").AsSuccess();

            })
            .RunAsync();

        var errors = default(List<string>);
        await LogBuilder.Message("- [Validation] ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                if ((errors = await kustomizeService.ValidateAsync()).Count > 0)
                {
                    return LogBuilder.Message("Failed.").AsError();
                }

                return LogBuilder.Message("Done.").AsSuccess();

            })
            .RunAsync();

        if (errors is { Count: > 0 })
        {
            foreach (var error in errors)
            {
                LogMessage.AsError($"> {error}");
            }
        }
    }
}
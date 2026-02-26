namespace Talaryon.StackManager.Proxy;

public static class HostingExtensions2
{
    extension(IApplicationBuilder applicationBuilder)
    {
        public static WebApplication AsApi()
        {
            return null;
        }
    }

    extension(IServiceCollection services)
    {
        public static IServiceCollection AddApiComponents()
        {
            return null;
        }
    }
}
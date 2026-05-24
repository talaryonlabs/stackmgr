namespace Talaryon.StackManager;

public static class HelperMethods
{
    public static string HostToName(string host) => host.Replace(".", "-");

    public static string GenerateRandomHostname() => Guid.NewGuid().ToString("N")[..6];
}
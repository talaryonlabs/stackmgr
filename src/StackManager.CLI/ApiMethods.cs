namespace Talaryon.StackManager;

public class ApiMethods
{
    [ApiVersion("stack.talaryon.io/v2beta")]
    private static string GetVolumeName(Stack stack, string volume) => $"{stack.Environment.Name}-{stack.Name}-{volume}";

    [ApiVersion]
    private static string GetVolumeNameLegacy(Stack stack, string volume) => volume;
}
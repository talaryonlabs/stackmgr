namespace Talaryon.StackManager;

/// <summary>
/// Default API version strings for StackManager configuration files.
/// </summary>
public static class ApiDefaults
{
    /// <summary>
    /// Default API version for environment files (.env.yaml)
    /// </summary>
    public const string EnvironmentVersion = "environment.talaryon.io/v1beta";

    /// <summary>
    /// Default API version for stack files (.stack.yaml)
    /// </summary>
    public const string StackVersion = "stack.talaryon.io/v2beta";

    /// <summary>
    /// Default API version for stack templates
    /// </summary>
    public const string TemplateVersion = "template.talaryon.io/v1beta";
}

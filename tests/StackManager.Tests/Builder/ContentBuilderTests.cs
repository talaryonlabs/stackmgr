using Talaryon.StackManager.Builder;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Models;
using Xunit;

namespace Talaryon.StackManager.Tests.Builder;

public class ContentBuilderTests
{
    private readonly StackEnvironment _environment = new()
    {
        Name = "test-env",
        Vault = "/vault"
    };

    private readonly Stack _stack = new()
    {
        Name = "test-stack",
        Environment = null!
    };

    public ContentBuilderTests()
    {
        _stack.Environment = _environment;
    }

    [Fact]
    public void Build_SingleParam_ShouldReplaceCorrectly()
    {
        // Arrange
        var app = new StackApp
        {
            Name = "test-app",
            Stack = _stack,
            Params = new Dictionary<string, string>
            {
                ["database"] = "mydb"
            }
        };

        var builder = new ContentBuilder(app);
        var content = "database: {{app-param.database}}";

        // Act
        var result = builder.With(content).Build();

        // Assert
        Assert.Equal("database: mydb", result);
    }

    [Fact]
    public void Build_MultipleParamsOnSameLine_ShouldReplaceAllCorrectly()
    {
        // Arrange
        var app = new StackApp
        {
            Name = "test-app",
            Stack = _stack,
            Params = new Dictionary<string, string>
            {
                ["database"] = "mydb",
                ["user"] = "admin"
            }
        };

        var builder = new ContentBuilder(app);
        var content = "databases: {{app-param.database}}: {{app-param.user}}";

        // Act
        var result = builder.With(content).Build();

        // Assert
        Assert.Equal("databases: mydb: admin", result);
    }

    [Fact]
    public void Build_MultipleParamsMultipleLines_ShouldReplaceAllCorrectly()
    {
        // Arrange
        var app = new StackApp
        {
            Name = "test-app",
            Stack = _stack,
            Params = new Dictionary<string, string>
            {
                ["database"] = "mydb",
                ["user"] = "admin",
                ["password"] = "secret"
            }
        };

        var builder = new ContentBuilder(app);
        var content = "{{app-param.database}}\n{{app-param.user}}\n{{app-param.password}}";

        // Act
        var result = builder.With(content).Build();

        // Assert
        Assert.Equal("mydb\nadmin\nsecret", result);
    }

    [Fact]
    public void Build_MultipleVolumesOnSameLine_ShouldReplaceAllCorrectly()
    {
        // Arrange
        var app = new StackApp
        {
            Name = "test-app",
            Stack = _stack,
            Volumes = new Dictionary<string, string>
            {
                ["data"] = "data-volume",
                ["config"] = "config-volume"
            }
        };

        var builder = new ContentBuilder(app);
        var content = "volumes: {{app-volume.data}} and {{app-volume.config}}";

        // Act
        var result = builder.With(content).Build();

        // Assert
        Assert.Equal("volumes: data-volume and config-volume", result);
    }

    [Fact]
    public void Build_MixedTemplatesOnSameLine_ShouldReplaceAllCorrectly()
    {
        // Arrange
        var app = new StackApp
        {
            Name = "my-app",
            Stack = _stack,
            Params = new Dictionary<string, string>
            {
                ["replicas"] = "3"
            },
            Volumes = new Dictionary<string, string>
            {
                ["storage"] = "my-storage"
            }
        };

        var builder = new ContentBuilder(app);
        var content = "{{app-name}} with {{app-param.replicas}} replicas and {{app-volume.storage}}";

        // Act
        var result = builder.With(content).Build();

        // Assert
        Assert.Equal("my-app with 3 replicas and my-storage", result);
    }

    [Fact]
    public void Build_MultipleRequirementsOnSameLine_ShouldReplaceAllCorrectly()
    {
        // Arrange
        var app = new StackApp
        {
            Name = "test-app",
            Stack = _stack,
            Requirements = new Dictionary<string, string>
            {
                ["cpu"] = "500m",
                ["memory"] = "1Gi"
            }
        };

        var builder = new ContentBuilder(app);
        var content = "resources: cpu={{app-requirement.cpu}}, memory={{app-requirement.memory}}";

        // Act
        var result = builder.With(content).Build();

        // Assert
        Assert.Equal("resources: cpu=500m, memory=1Gi", result);
    }

    [Fact]
    public void Build_MissingParam_ShouldThrowConfigurationException()
    {
        // Arrange
        var app = new StackApp
        {
            Name = "test-app",
            Stack = _stack,
            Params = new Dictionary<string, string>()
        };

        var builder = new ContentBuilder(app);
        var content = "database: {{app-param.database}}";

        // Act & Assert
        var ex = Assert.Throws<ConfigurationException>(() => builder.With(content).Build());
        Assert.Contains("database", ex.Message);
        Assert.Contains("test-app", ex.Message);
    }

    [Fact]
    public void Build_AppNameAndStackName_ShouldReplaceCorrectly()
    {
        // Arrange
        var app = new StackApp
        {
            Name = "my-app",
            Stack = _stack
        };

        var builder = new ContentBuilder(app);
        var content = "App: {{app-name}}, Stack: {{stack-name}}, Env: {{env-name}}";

        // Act
        var result = builder.With(content).Build();

        // Assert
        Assert.Equal("App: my-app, Stack: test-stack, Env: test-env", result);
    }

    [Fact]
    public void Build_VaultPath_ShouldReplaceCorrectly()
    {
        // Arrange
        var app = new StackApp
        {
            Name = "my-app",
            Stack = _stack
        };

        var builder = new ContentBuilder(app);
        var content = "vault: {{vault-path}}";

        // Act
        var result = builder.With(content).Build();

        // Assert
        Assert.Equal("vault: /vault/test-stack/my-app", result);
    }

    [Fact]
    public void Build_VaultPathNotConfigured_ShouldThrowConfigurationException()
    {
        // Arrange
        var envWithoutVault = new StackEnvironment
        {
            Name = "test-env-no-vault",
            Vault = null
        };

        var stackWithoutVault = new Stack
        {
            Name = "test-stack-no-vault",
            Environment = envWithoutVault
        };

        var app = new StackApp
        {
            Name = "test-app",
            Stack = stackWithoutVault
        };

        var builder = new ContentBuilder(app);
        var content = "vault: {{vault-path}}";

        // Act & Assert
        var ex = Assert.Throws<ConfigurationException>(() => builder.With(content).Build());
        Assert.Contains("Vault-Path is not configured", ex.Message);
    }
}

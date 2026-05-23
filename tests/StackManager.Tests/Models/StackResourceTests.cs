using System.IO;
using Talaryon.StackManager.Types;
using Xunit;

namespace Talaryon.StackManager.Tests.Models;

[Collection("FileSystemTests")]
public class StackResourceTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _originalDirectory;

    public StackResourceTests()
    {
        _originalDirectory = Environment.CurrentDirectory;
        _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
        Environment.CurrentDirectory = _testDir;
    }

    public void Dispose()
    {
        try
        {
            Environment.CurrentDirectory = _originalDirectory;
        }
        finally
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }
    }

    [Fact]
    public void Save_ShouldCreateFile()
    {
        var env = new StackEnvironment
        {
            Name = "test-env",
            Vault = "vault1",
            Outpost = "outpost1",
            CertIssuer = "cert1",
            RegistryCredentials = "creds1",
            Repository = "repo1",
            Remote = "remote1"
        };

        var file = new FileInfo(Path.Combine(_testDir, ".env.yaml"));
        StackResource.Save(env, file);

        Assert.True(file.Exists);
        Assert.True(file.Length > 0);
    }

    [Fact]
    public void SaveAndLoad_ShouldRoundTrip()
    {
        var env = new StackEnvironment
        {
            Name = "test-env",
            Vault = "vault1",
            Outpost = "outpost1",
            CertIssuer = "cert1",
            RegistryCredentials = "creds1",
            Repository = "https://github.com/test/repo",
            Remote = "remote1"
        };

        var file = new FileInfo(Path.Combine(_testDir, ".env.yaml"));
        StackResource.Save(env, file);

        var loaded = StackResource.Load<StackEnvironment>(file);

        Assert.Equal(env.Name, loaded.Name);
        Assert.Equal(env.Vault, loaded.Vault);
        Assert.Equal(env.Outpost, loaded.Outpost);
        Assert.Equal(env.CertIssuer, loaded.CertIssuer);
        Assert.Equal(env.RegistryCredentials, loaded.RegistryCredentials);
        Assert.Equal(env.Repository, loaded.Repository);
        Assert.Equal(env.Remote, loaded.Remote);
    }

    [Fact]
    public void Load_ShouldThrowIfFileNotFound()
    {
        var file = new FileInfo(Path.Combine(_testDir, "nonexistent.yaml"));

        var ex = Assert.Throws<FileNotFoundException>(() => StackResource.Load<StackEnvironment>(file));
        Assert.Contains("nonexistent.yaml", ex.Message);
    }

    [Fact]
    public void Save_ShouldOverwriteExistingFile()
    {
        var env1 = new StackEnvironment
        {
            Name = "test-env-1",
            Vault = "vault1",
            Outpost = "",
            CertIssuer = "",
            RegistryCredentials = "",
            Remote = ""
        };

        var file = new FileInfo(Path.Combine(_testDir, ".env.yaml"));
        StackResource.Save(env1, file);

        var env2 = new StackEnvironment
        {
            Name = "test-env-2",
            Vault = "vault2",
            Outpost = "",
            CertIssuer = "",
            RegistryCredentials = "",
            Remote = ""
        };

        StackResource.Save(env2, file);

        var loaded = StackResource.Load<StackEnvironment>(file);
        Assert.Equal("test-env-2", loaded.Name);
        Assert.Equal("vault2", loaded.Vault);
    }

    [Fact]
    public void Save_Stack_ShouldCreateValidYaml()
    {
        var env = StackEnvironment.Create("test-env");
        var stack = Stack.Create(env, "test-stack");

        var file = new FileInfo(Path.Combine(_testDir, "test-stack.yaml"));
        StackResource.Save(stack, file);

        Assert.True(file.Exists);
        Assert.True(file.Length > 0);

        // Load it back
        var loaded = StackResource.Load<Stack>(file);
        Assert.Equal(stack.Name, loaded.Name);
        Assert.Equal(stack.Namespace, loaded.Namespace);
    }
}

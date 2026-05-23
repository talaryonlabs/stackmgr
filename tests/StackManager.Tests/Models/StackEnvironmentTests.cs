using System.IO;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Types;
using Xunit;

namespace Talaryon.StackManager.Tests.Models;

[Collection("FileSystemTests")]
public class StackEnvironmentTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _originalDirectory;

    public StackEnvironmentTests()
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
    public void Create_ShouldCreateDirectoryAndFile()
    {
        var env = StackEnvironment.Create("test-env");

        Assert.Equal("test-env", env.Name);
        Assert.True(Directory.Exists(Path.Combine(_testDir, "test-env")));
        Assert.True(File.Exists(Path.Combine(_testDir, "test-env", ".env.yaml")));
    }

    [Fact]
    public void Create_ShouldThrowIfAlreadyExists()
    {
        var env1 = StackEnvironment.Create("test-env");

        var ex = Assert.Throws<EnvironmentAlreadyExistsException>(() => StackEnvironment.Create("test-env"));
        Assert.Contains("test-env", ex.Message);
    }

    [Fact]
    public void Load_ShouldLoadExistingEnvironment()
    {
        var env1 = StackEnvironment.Create("test-env");
        var env2 = StackEnvironment.Load("test-env");

        Assert.Equal(env1.Name, env2.Name);
    }

    [Fact]
    public void Load_ShouldThrowIfNotFound()
    {
        var ex = Assert.Throws<EnvironmentNotFoundException>(() => StackEnvironment.Load("nonexistent"));
        Assert.Contains("nonexistent", ex.Message);
    }

    [Fact]
    public void SaveConfig_ShouldUpdateFile()
    {
        var env = StackEnvironment.Create("test-env");
        env.Vault = "my-vault";
        env.SaveConfig();

        var loaded = StackEnvironment.Load("test-env");
        Assert.Equal("my-vault", loaded.Vault);
    }

    [Fact]
    public void LocalDirectory_ShouldReturnCorrectPath()
    {
        var env = StackEnvironment.Create("test-env");
        var expectedPath = Path.Combine(_testDir, "test-env");

        Assert.Equal(expectedPath, env.LocalDirectory.FullName);
    }

    [Fact]
    public void LocalFile_ShouldReturnCorrectPath()
    {
        var env = StackEnvironment.Create("test-env");
        var expectedPath = Path.Combine(_testDir, "test-env", ".env.yaml");

        Assert.Equal(expectedPath, env.LocalFile.FullName);
    }
}

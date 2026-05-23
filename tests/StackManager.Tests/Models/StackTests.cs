using System.IO;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Types;
using Xunit;

namespace Talaryon.StackManager.Tests.Models;

[Collection("FileSystemTests")]
public class StackTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _originalDirectory;

    public StackTests()
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
        var stack = Stack.Create(env, "test-stack");

        Assert.Equal("test-stack", stack.Name);
        Assert.Equal("test-env-test-stack", stack.Namespace);
        Assert.True(Directory.Exists(Path.Combine(_testDir, "test-env", "test-stack")));
        Assert.True(File.Exists(Path.Combine(_testDir, "test-env", "test-stack", ".stack.yaml")));
    }

    [Fact]
    public void Create_WithDotsInName_ShouldReplaceWithHyphensInNamespace()
    {
        var env = StackEnvironment.Create("test-env");
        var stack = Stack.Create(env, "example.com");

        Assert.Equal("example.com", stack.Name);
        Assert.Equal("test-env-example-com", stack.Namespace);
    }

    [Fact]
    public void Create_ShouldThrowIfAlreadyExists()
    {
        var env = StackEnvironment.Create("test-env");
        var stack1 = Stack.Create(env, "test-stack");

        var ex = Assert.Throws<StackAlreadyExistsException>(() => Stack.Create(env, "test-stack"));
        Assert.Contains("test-stack", ex.Message);
    }

    [Fact]
    public void Load_ShouldLoadExistingStack()
    {
        var env = StackEnvironment.Create("test-env");
        var stack1 = Stack.Create(env, "test-stack");
        var stack2 = Stack.Load(env, "test-stack");

        Assert.Equal(stack1.Name, stack2.Name);
        Assert.Equal(stack1.Namespace, stack2.Namespace);
    }

    [Fact]
    public void Load_ShouldThrowIfNotFound()
    {
        var env = StackEnvironment.Create("test-env");

        var ex = Assert.Throws<StackNotFoundException>(() => Stack.Load(env, "nonexistent"));
        Assert.Contains("nonexistent", ex.Message);
    }

    [Fact]
    public void Delete_ShouldMarkAsDeleted()
    {
        var env = StackEnvironment.Create("test-env");
        var stack = Stack.Create(env, "test-stack");
        
        Assert.False(stack.IsDeleted);
        
        stack.Delete();
        
        Assert.True(stack.IsDeleted);
        Assert.True(File.Exists(Path.Combine(_testDir, "test-env", "test-stack", ".stack.yaml")));
    }

    [Fact]
    public void Delete_WithCompleteTrue_ShouldRemoveDirectory()
    {
        var env = StackEnvironment.Create("test-env");
        var stack = Stack.Create(env, "test-stack");
        var stackDir = Path.Combine(_testDir, "test-env", "test-stack");
        
        stack.Delete(true);
        
        Assert.False(Directory.Exists(stackDir));
    }

    [Fact]
    public void Delete_ShouldThrowIfAlreadyDeleted()
    {
        var env = StackEnvironment.Create("test-env");
        var stack = Stack.Create(env, "test-stack");
        stack.Delete();

        var ex = Assert.Throws<StackAlreadyDeletedException>(() => stack.Delete());
        Assert.Contains("test-stack", ex.Message);
    }

    [Fact]
    public void Namespace_ShouldBeLowercaseWithHyphens()
    {
        var env = StackEnvironment.Create("Prod-Env");
        var stack = Stack.Create(env, "MyStack.Test");

        // Environment name in namespace should be from env.Name (which is "Prod-Env")
        // Stack name dots replaced with hyphens
        Assert.Equal("prod-env-mystack-test", stack.Namespace);
    }

    [Fact]
    public void SaveConfig_ShouldUpdateFile()
    {
        var env = StackEnvironment.Create("test-env");
        var stack = Stack.Create(env, "test-stack");
        stack.EnableAutoSync = true;
        stack.SaveConfig();

        var loaded = Stack.Load(env, "test-stack");
        Assert.True(loaded.EnableAutoSync);
    }
}

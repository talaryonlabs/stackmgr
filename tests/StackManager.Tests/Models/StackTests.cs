using Talaryon.StackManager.Models;
using Xunit;

namespace Talaryon.StackManager.Tests.Models;

public class StackTests
{
    [Fact]
    public void Stack_ShouldHaveDefaultValues()
    {
        var env = new StackEnvironment { Name = "test-env" };
        var stack = new Stack { Name = "test-stack", Environment = env };

        Assert.Equal("test-stack", stack.Name);
        Assert.Equal("stack.talaryon.io/v1beta", stack.Version);
        Assert.Null(stack.Namespace);
        Assert.False(stack.EnableAutoSync);
        Assert.False(stack.IsDeleted);
        Assert.Empty(stack.Images);
        Assert.Empty(stack.Apps);
        Assert.Empty(stack.Ingresses);
        Assert.Empty(stack.Volumes);
    }

    [Fact]
    public void Stack_WithNamespaceSet_ShouldHaveCorrectNamespace()
    {
        var env = new StackEnvironment { Name = "test-env" };
        var stack = new Stack { Name = "test-stack", Environment = env, Namespace = "test-env-test-stack" };

        Assert.Equal("test-env-test-stack", stack.Namespace);
    }

    [Fact]
    public void Stack_NamespaceWithDotsInName_ShouldUseHyphens()
    {
        var env = new StackEnvironment { Name = "prod-env" };
        var stack = new Stack { Name = "MyStack.Test", Environment = env, Namespace = "prod-env-mystack-test" };

        // Stack name dots replaced with hyphens
        Assert.Equal("prod-env-mystack-test", stack.Namespace);
    }

    [Fact]
    public void Stack_AllPropertiesShouldBeSettable()
    {
        var env = new StackEnvironment { Name = "test-env" };
        var stack = new Stack
        {
            Name = "test-stack",
            Environment = env,
            Namespace = "test-ns",
            Version = "stack.talaryon.io/v2",
            EnableAutoSync = true,
            IsDeleted = true,
            Images = [],
            Apps = [],
            Ingresses = [],
            Volumes = []
        };

        Assert.Equal("test-stack", stack.Name);
        Assert.Equal("test-ns", stack.Namespace);
        Assert.Equal("stack.talaryon.io/v2", stack.Version);
        Assert.True(stack.EnableAutoSync);
        Assert.True(stack.IsDeleted);
    }

    [Fact]
    public void Stack_ShouldBeDeletable()
    {
        var env = new StackEnvironment { Name = "test-env" };
        var stack = new Stack { Name = "test-stack", Environment = env, IsDeleted = false };

        Assert.False(stack.IsDeleted);
        
        stack.IsDeleted = true;
        
        Assert.True(stack.IsDeleted);
    }

    [Fact]
    public void Stack_WithEnableAutoSync_ShouldBeTrue()
    {
        var env = new StackEnvironment { Name = "test-env" };
        var stack = new Stack { Name = "test-stack", Environment = env, EnableAutoSync = true };

        Assert.True(stack.EnableAutoSync);
    }

    [Fact]
    public void Stack_ListsShouldBeInitialized()
    {
        var env = new StackEnvironment { Name = "test-env" };
        var stack = new Stack { Name = "test-stack", Environment = env };

        Assert.NotNull(stack.Images);
        Assert.NotNull(stack.Apps);
        Assert.NotNull(stack.Ingresses);
        Assert.NotNull(stack.Volumes);
        Assert.Empty(stack.Images);
        Assert.Empty(stack.Apps);
        Assert.Empty(stack.Ingresses);
        Assert.Empty(stack.Volumes);
    }
}

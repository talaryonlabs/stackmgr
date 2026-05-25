using System;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Validation;
using Xunit;

namespace Talaryon.StackManager.Tests;

public class ExceptionTests
{
    [Fact]
    public void CliException_ShouldHaveExitCode()
    {
        var ex = new CliException("test", 1);
        Assert.Equal(1, ex.ExitCode);
        Assert.Equal("test", ex.Message);
    }

    [Fact]
    public void UserErrorException_ShouldHaveExitCode1()
    {
        var ex = new UserErrorException("user error");
        Assert.Equal(1, ex.ExitCode);
    }

    [Fact]
    public void SystemErrorException_ShouldHaveExitCode2()
    {
        var ex = new SystemErrorException("system error");
        Assert.Equal(2, ex.ExitCode);
    }

    [Fact]
    public void ConfigurationException_ShouldHaveExitCode3()
    {
        var ex = new ConfigurationException("config error");
        Assert.Equal(3, ex.ExitCode);
    }

    [Fact]
    public void ValidationException_ShouldBeCliException()
    {
        var ex = new ValidationException("validation error");
        Assert.IsAssignableFrom<CliException>(ex);
        Assert.Equal(1, ex.ExitCode);
    }

    [Fact]
    public void StackNameValidationException_ShouldBeValidationException()
    {
        var ex = new StackNameValidationException("invalid stack name");
        Assert.IsAssignableFrom<ValidationException>(ex);
        Assert.IsAssignableFrom<CliException>(ex);
    }

    [Fact]
    public void HostnameValidationException_ShouldBeValidationException()
    {
        var ex = new HostnameValidationException("invalid hostname");
        Assert.IsAssignableFrom<ValidationException>(ex);
        Assert.IsAssignableFrom<CliException>(ex);
    }

    [Fact]
    public void PortValidationException_ShouldBeValidationException()
    {
        var ex = new PortValidationException("invalid port");
        Assert.IsAssignableFrom<ValidationException>(ex);
        Assert.IsAssignableFrom<CliException>(ex);
    }

    [Fact]
    public void UrlValidationException_ShouldBeValidationException()
    {
        var ex = new UrlValidationException("invalid URL");
        Assert.IsAssignableFrom<ValidationException>(ex);
        Assert.IsAssignableFrom<CliException>(ex);
    }
}

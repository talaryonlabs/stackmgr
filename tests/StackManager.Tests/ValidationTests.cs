using Talaryon.StackManager.Validation;
using Xunit;

namespace Talaryon.StackManager.Tests;

public class ValidationTests
{
    [Theory]
    [InlineData("example.com")]
    [InlineData("test.at")]
    [InlineData("my-app.dev")]
    [InlineData("a-b-c.d.e-f")]
    [InlineData("simple")]
    [InlineData("test-123")]
    [InlineData("a.b.c.d")]
    public void ValidateStackName_ValidNamesWithDots_ShouldPass(string name)
    {
        // Act & Assert - should not throw
        ValidationHelper.ValidateStackName(name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-")]
    [InlineData("-test")]
    [InlineData("test-")]
    [InlineData(".test")]
    [InlineData("test.")]
    [InlineData("TEST")]
    [InlineData("Test")]
    [InlineData("TEST.COM")]
    public void ValidateStackName_InvalidNames_ShouldThrow(string name)
    {
        var ex = Assert.Throws<StackNameValidationException>(() => ValidationHelper.ValidateStackName(name));
        Assert.Contains("stack name", ex.Message);
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("example.com")]
    [InlineData("test.at")]
    [InlineData("sub.example.com")]
    public void ValidateHostname_ValidHostnames_ShouldPass(string hostname)
    {
        ValidationHelper.ValidateHostname(hostname);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-test")]
    [InlineData("test-")]
    [InlineData(".test")]
    [InlineData("test..com")]
    public void ValidateHostname_InvalidHostnames_ShouldThrow(string hostname)
    {
        var ex = Assert.Throws<HostnameValidationException>(() => ValidationHelper.ValidateHostname(hostname));
        Assert.Contains("hostname", ex.Message);
    }

    [Theory]
    [InlineData("1Gi")]
    [InlineData("10Gi")]
    [InlineData("100Mi")]
    [InlineData("500m")]
    [InlineData("1000")]
    [InlineData("1")]
    public void ValidateAndNormalizeSize_ValidSizes_ShouldPass(string size)
    {
        var result = ValidationHelper.ValidateAndNormalizeSize(size);
        Assert.NotNull(result);
        Assert.StartsWith(size.TrimEnd('i', 'I'), result, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-1Gi")]
    [InlineData("abc")]
    public void ValidateAndNormalizeSize_InvalidSizes_ShouldThrow(string size)
    {
        var ex = Assert.Throws<SizeValidationException>(() => ValidationHelper.ValidateAndNormalizeSize(size));
        Assert.Contains("size", ex.Message);
    }

    [Theory]
    [InlineData(80)]
    [InlineData(443)]
    [InlineData(8080)]
    [InlineData(1)]
    [InlineData(65535)]
    public void ValidatePort_ValidPorts_ShouldPass(int port)
    {
        ValidationHelper.ValidatePort(port);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65536)]
    [InlineData(-1)]
    public void ValidatePort_InvalidPorts_ShouldThrow(int port)
    {
        var ex = Assert.Throws<PortValidationException>(() => ValidationHelper.ValidatePort(port));
        Assert.Contains("port", ex.Message);
    }

    [Theory]
    [InlineData("myapp")]
    [InlineData("app-123")]
    [InlineData("a")]
    public void ValidateAppName_ValidNames_ShouldPass(string name)
    {
        ValidationHelper.ValidateAppName(name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Test")]
    [InlineData("-test")]
    [InlineData("test-")]
    [InlineData("test.name")]
    public void ValidateAppName_InvalidNames_ShouldThrow(string name)
    {
        var ex = Assert.Throws<AppNameValidationException>(() => ValidationHelper.ValidateAppName(name));
        Assert.Contains("app name", ex.Message);
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://localhost:8080")]
    [InlineData("https://github.com/user/repo")]
    public void ValidateUrl_ValidUrls_ShouldPass(string url)
    {
        ValidationHelper.ValidateUrl(url);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    public void ValidateUrl_InvalidUrls_ShouldThrow(string url)
    {
        var ex = Assert.Throws<UrlValidationException>(() => ValidationHelper.ValidateUrl(url));
        Assert.Contains("URL", ex.Message);
    }
}

using System;
using Talaryon.StackManager;
using Xunit;

namespace Talaryon.StackManager.Tests;

public class LocalConfigTests
{
    [Fact]
    public void Get_ShouldReturnNonNullInstance()
    {
        var config = LocalConfig.Get();
        
        Assert.NotNull(config);
        Assert.NotNull(config.Remotes);
        Assert.NotNull(config.Defaults);
    }

    [Fact]
    public void Get_ShouldReturnSingleton()
    {
        var config1 = LocalConfig.Get();
        var config2 = LocalConfig.Get();
        
        Assert.Same(config1, config2);
    }

    [Fact]
    public void EncryptAndDecrypt_ShouldRoundTrip()
    {
        var original = "my-secret-token";
        var encrypted = LocalConfig.Encrypt(original);
        var decrypted = LocalConfig.Decrypt(encrypted);
        
        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void Encrypt_WithNullOrEmpty_ShouldReturnInput()
    {
        Assert.Equal("", LocalConfig.Encrypt(""));
        Assert.Null(LocalConfig.Encrypt(null));
    }

    [Fact]
    public void Decrypt_WithInvalidData_ShouldReturnInput()
    {
        var invalid = "not-base64!!!";
        Assert.Equal(invalid, LocalConfig.Decrypt(invalid));
    }

    [Fact]
    public void Defaults_ShouldHaveNullableProperties()
    {
        var config = LocalConfig.Get();
        
        Assert.NotNull(config.Defaults);
        // These may be null or have values, just check they don't throw
        var env = config.Defaults.Environment;
        var stack = config.Defaults.Stack;
    }

    [Fact]
    public void Remotes_ShouldBeNonNull()
    {
        var config = LocalConfig.Get();
        
        Assert.NotNull(config.Remotes);
    }
}

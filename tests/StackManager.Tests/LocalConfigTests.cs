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
        Assert.Null(LocalConfig.Encrypt(null!));
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

    [Fact]
    public void LocalConfigDefault_ShouldHaveNullableProperties()
    {
        var defaults = new LocalConfigDefault();
        
        Assert.Null(defaults.Environment);
        Assert.Null(defaults.Stack);
    }

    [Fact]
    public void LocalConfigRemote_ShouldEncryptAccessToken()
    {
        var remote = new LocalConfigRemote
        {
            Name = "test-remote",
            Url = "https://api.example.com",
            AccessToken = "my-secret-token"
        };
        
        // The AccessToken setter should encrypt the value
        Assert.NotNull(remote.EncryptedAccessToken);
        Assert.NotEqual("my-secret-token", remote.EncryptedAccessToken);
        
        // The getter should decrypt it back
        Assert.Equal("my-secret-token", remote.AccessToken);
    }

    [Fact]
    public void LocalConfig_ShouldHaveDefaultValues()
    {
        var config = new LocalConfig();
        
        Assert.Equal("", config.AppRepository);
        Assert.NotNull(config.Remotes);
        Assert.Empty(config.Remotes);
        Assert.NotNull(config.Defaults);
        Assert.False(config.DebugMode);
    }

    [Fact]
    public void LocalConfigRemote_AccessTokenGetterSetter_ShouldWork()
    {
        var remote = new LocalConfigRemote
        {
            Name = "test",
            Url = "https://test.com",
            AccessToken = "secret123"
        };
        
        // Verify the token is encrypted
        Assert.NotEqual("secret123", remote.EncryptedAccessToken);
        
        // Verify we can get it back
        Assert.Equal("secret123", remote.AccessToken);
        
        // Set a new token
        remote.AccessToken = "new-secret";
        
        // Verify it's encrypted
        Assert.NotEqual("new-secret", remote.EncryptedAccessToken);
        
        // Verify we can get the new one back
        Assert.Equal("new-secret", remote.AccessToken);
    }
}

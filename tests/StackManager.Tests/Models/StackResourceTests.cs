using System.IO;
using Talaryon.StackManager.Models;
using Xunit;

namespace Talaryon.StackManager.Tests.Models;

public class StackResourceTests
{
    [Fact]
    public void Save_ShouldWriteToFile()
    {
        // Create a mock FileInfo that tracks if Save was called
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

        // Use a real temp file for testing
        var tempPath = Path.GetTempFileName();
        var file = new FileInfo(tempPath);

        try
        {
            StackResource.Save(env, file);

            Assert.True(file.Exists);
            Assert.True(file.Length > 0);
        }
        finally
        {
            if (file.Exists)
                file.Delete();
        }
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

        var tempPath = Path.GetTempFileName();
        var file = new FileInfo(tempPath);

        try
        {
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
        finally
        {
            if (file.Exists)
                file.Delete();
        }
    }

    [Fact]
    public void Load_ShouldThrowIfFileNotFound()
    {
        var file = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".yaml"));

        // For StackEnvironment type, it throws EnvironmentNotFoundException
        var ex = Assert.Throws<Talaryon.StackManager.Exceptions.EnvironmentNotFoundException>(
            () => StackResource.Load<StackEnvironment>(file));
        Assert.Contains(file.Name, ex.Message);
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

        var tempPath = Path.GetTempFileName();
        var file = new FileInfo(tempPath);

        try
        {
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
        finally
        {
            if (file.Exists)
                file.Delete();
        }
    }

    [Fact]
    public void Save_Stack_ShouldCreateValidYaml()
    {
        var env = new StackEnvironment { Name = "test-env" };
        var stack = new Stack { Name = "test-stack", Environment = env, Namespace = "test-ns" };

        var tempPath = Path.GetTempFileName();
        var file = new FileInfo(tempPath);

        try
        {
            StackResource.Save(stack, file);

            Assert.True(file.Exists);
            Assert.True(file.Length > 0);

            // Load it back
            var loaded = StackResource.Load<Stack>(file);
            Assert.Equal(stack.Name, loaded.Name);
            Assert.Equal(stack.Namespace, loaded.Namespace);
        }
        finally
        {
            if (file.Exists)
                file.Delete();
        }
    }

    [Fact]
    public void Load_ShouldThrowStackNotFoundException_ForStackType()
    {
        var file = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".yaml"));

        var ex = Assert.Throws<Talaryon.StackManager.Exceptions.StackNotFoundException>(
            () => StackResource.Load<Stack>(file));
    }

    [Fact]
    public void Load_ShouldThrowEnvironmentNotFoundException_ForEnvironmentType()
    {
        var file = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".yaml"));

        var ex = Assert.Throws<Talaryon.StackManager.Exceptions.EnvironmentNotFoundException>(
            () => StackResource.Load<StackEnvironment>(file));
    }

    [Fact]
    public void Load_ShouldThrowTemplateNotFoundException_ForTemplateType()
    {
        var file = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".yaml"));

        var ex = Assert.Throws<Talaryon.StackManager.Exceptions.TemplateNotFoundException>(
            () => StackResource.Load<StackTemplate>(file));
    }
}

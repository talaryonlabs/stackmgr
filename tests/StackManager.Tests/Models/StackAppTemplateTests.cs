using System.IO;
using Talaryon.StackManager.Models;
using Talaryon.StackManager.Serialization;
using Xunit;

namespace Talaryon.StackManager.Tests.Models;

public class StackAppTemplateTests
{
    [Fact]
    public void StackAppTemplate_FromString_WithNameOnly_ShouldDefaultToMain()
    {
        var template = StackAppTemplate.FromString("my-template");
        
        Assert.Equal("my-template", template.Name);
        Assert.Equal("main", template.Branch);
    }

    [Fact]
    public void StackAppTemplate_FromString_WithNameAndBranch_ShouldParseCorrectly()
    {
        var template = StackAppTemplate.FromString("my-template@dev");
        
        Assert.Equal("my-template", template.Name);
        Assert.Equal("dev", template.Branch);
    }

    [Fact]
    public void StackAppTemplate_ImplicitConversion_FromString()
    {
        StackAppTemplate template = "my-template@main";
        
        Assert.Equal("my-template", template.Name);
        Assert.Equal("main", template.Branch);
    }

    [Fact]
    public void YamlSerialization_WithObjectFormat_ShouldRoundTrip()
    {
        var env = new StackEnvironment { Name = "test-env" };
        var stack = new Stack { Name = "test-stack", Environment = env };
        var app = new StackApp
        {
            Name = "my-app",
            Stack = stack,
            Template = new StackAppTemplate { Name = "my-template", Branch = "main" }
        };

        var yaml = YamlSerializer.Serialize(app);
        
        // Verify the YAML contains the template as object
        Assert.Contains("template:", yaml);
        Assert.Contains("name: my-template", yaml);
        Assert.Contains("branch: main", yaml);
    }

    [Fact]
    public void YamlDeserialization_WithStringFormat_ShouldParse()
    {
        var yaml = @"
name: my-app
template: my-template@dev
";
        
        var app = YamlSerializer.Deserialize<StackApp>(yaml);
        
        Assert.Equal("my-app", app.Name);
        Assert.NotNull(app.Template);
        Assert.Equal("my-template", app.Template.Name);
        Assert.Equal("dev", app.Template.Branch);
    }

    [Fact]
    public void YamlDeserialization_WithObjectFormat_ShouldParse()
    {
        var yaml = @"
name: my-app
template:
  name: my-template
  branch: feature
";
        
        var app = YamlSerializer.Deserialize<StackApp>(yaml);
        
        Assert.Equal("my-app", app.Name);
        Assert.NotNull(app.Template);
        Assert.Equal("my-template", app.Template.Name);
        Assert.Equal("feature", app.Template.Branch);
    }

    [Fact]
    public void YamlRoundTrip_StringToObjectToObject_ShouldPreserveData()
    {
        // Start with string format
        var yaml1 = @"name: my-app
template: my-template@main
";
        
        // Deserialize (string -> object)
        var app = YamlSerializer.Deserialize<StackApp>(yaml1);
        Assert.Equal("my-template", app.Template?.Name);
        Assert.Equal("main", app.Template?.Branch);
        
        // Serialize (object -> YAML with object format)
        var yaml2 = YamlSerializer.Serialize(app);
        
        // Verify it's serialized as object format, not string
        Assert.Contains("template:", yaml2);
        Assert.Contains("name: my-template", yaml2);
        Assert.Contains("branch: main", yaml2);
        
        // Deserialize again
        var app2 = YamlSerializer.Deserialize<StackApp>(yaml2);
        Assert.Equal("my-template", app2.Template?.Name);
        Assert.Equal("main", app2.Template?.Branch);
    }

    [Fact]
    public void YamlRoundTrip_WithColonSeparator_ShouldWork()
    {
        // Test with colon separator
        var yaml1 = @"name: my-app
template: my-template:dev
";
        
        var app = YamlSerializer.Deserialize<StackApp>(yaml1);
        Assert.Equal("my-template", app.Template?.Name);
        Assert.Equal("dev", app.Template?.Branch);
        
        // Serialize and verify it's in object format
        var yaml2 = YamlSerializer.Serialize(app);
        Assert.Contains("name: my-template", yaml2);
        Assert.Contains("branch: dev", yaml2);
    }
}

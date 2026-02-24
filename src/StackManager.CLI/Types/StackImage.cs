using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackImage : IStackObject
{
    public static StackImage Create(Stack stack, string image, string? name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            var parts = image.Split("/");
            name = parts[^1].Contains(':') ? parts[^1].Split(":")[0] : parts[^1];
        }

        if (stack.Images.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new Exception($"Image with name '{name}' already exists in stack '{stack.Name}' (use 'stackmgr migrate image' instead)");
        }

        var img = new StackImage
        {
            Stack = stack,
            Name = name,
            Image = image
        };

        lock (stack.Images)
        {
            stack.Images.Add(img);
        }
        stack.SaveConfig();
        stack.Build();

        return img;
    }

    public void Migrate(string newImage)
    {
        var parts = newImage.Split("/");
        var name = parts[^1].Contains(':') ? parts[^1].Split(":")[0] : parts[^1];
        
        Image = newImage;
        Stack.SaveConfig();
    }
    
    public void Delete()
    {
        lock (Stack.Images)
        {
            Stack.Images.Remove(this);
        }
        Stack.SaveConfig();
    }
    
    [YamlIgnore] public required Stack Stack { get; set; }
    
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "image")] public required string Image { get; set; }
}
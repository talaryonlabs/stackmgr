using System.Reflection;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager;

public static class ExtensionMethods
{
    extension(StackEnvironment env)
    {
        public Stack GetStack(string name)
        {
            var path = Path.Combine(env.LocalDirectory.FullName, name, Stack.FileName); 
            var file = new FileInfo(path);
        
            if (!file.Exists) 
                throw new StackNotFoundException(name);

            var stack = StackResource.Load<Stack>(file);

            stack.Environment = env;
            
            typeof(Stack)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(v => v.PropertyType.GetInterfaces().Any(i => i == typeof(IStackObject)))
                .Select(v => v.GetValue(stack))
                .OfType<IStackObject>()
                .ToList()
                .ForEach(v => v.Stack = stack);

            return stack;
        }

        public IReadOnlyList<Stack> GetStacks()
        {
            return env.LocalDirectory.GetDirectories()
                .Select(v => env.GetStack(v.Name))
                .ToList();
        }
        
        public Stack NewStack(string name)
        {
            var stack = new Stack
            {
                Name = name,
                Environment = env,
                Namespace = $"{env.Name.ToLower()}-{name.ToLower().Replace(".", "-")}",
                Images = [],
                Apps = [],
                Ingresses = [],
                Volumes = [],
            };

            if (stack.LocalFile.Exists)
                throw new StackAlreadyExistsException(stack);
        
            if(!stack.LocalDirectory.Exists)
                stack.LocalDirectory.Create();
        
            stack.Save();
        
            return stack;
        }

        public void Save()
        {
            if (!env.LocalDirectory.Exists)
                env.LocalDirectory.Create();
            
            StackResource.Save(env, env.LocalFile);
        }
    }

    extension(Stack stack)
    {
        public IStackObjectFactory<T> New<T>() where T : class, IStackObject
        {
            return new StackObjectFactory<T>(stack);
        }

        public T Get<T>(string name) where T : class, IStackObject
        {
            var list = typeof(Stack)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(v => v.PropertyType == typeof(List<T>))
                .Select(v => v.GetValue(stack))
                .OfType<List<T>>()
                .FirstOrDefault();
            
            if(list is null)
                throw new InvalidOperationException();

            return list.FirstOrDefault(v => v.Name == name) ?? throw new ResourceNotFoundException<T>(stack, name);
        }

        public void Delete(bool complete = false)
        {
            if (complete)
            {
                stack.LocalDirectory.Delete(true);
                return;
            }

            if (stack.IsDeleted)
                throw new StackAlreadyDeletedException(stack.Name);

            stack.IsDeleted = true;
            stack.Save();
        }

        public void Save()
        {
            StackResource.Save(stack, stack.LocalFile);
        }
    }

    extension(StackApp app)
    {
        public async Task<bool> CheckRequirements(StackTemplate template)
        {
            var errors = new Dictionary<string, string>();
            var files = template.LocalDirectory
                .GetFileSystemInfos("*", SearchOption.AllDirectories);

            foreach (var requirement in app.Requirements.Where(requirement =>
                         !app.Stack.Apps.Exists(v => v.Name == requirement.Value)))
            {
                errors.TryAdd(requirement.Key, $"Required app '{requirement.Value}' not found in stack.");
            }

            foreach (var volume in app.Volumes.Where(volume => !app.Stack.Volumes.Exists(v => v.Name == volume.Value)))
            {
                errors.TryAdd(volume.Key, $"Required volume '{volume.Value}' not found in stack.");
            }

            foreach (var file in files)
            {
                var content = await File.ReadAllTextAsync(file.FullName);
                if (content.Contains("{{vault-path}}") && string.IsNullOrEmpty(app.Stack.Environment.Vault))
                {
                    errors.TryAdd("vault-path",
                        "Vault-Path is not configured. Please run 'stackmgr configure env <environment-name> --vault <vault-path>' first.");
                }
            }

            if (errors.Count == 0) return true;
            foreach (var error in errors)
            {
                LogMessage.AsError($"- {error.Value}");
            }

            return false;
        }
    }

    extension(IStackObject stackObject)
    {
        public void Delete<T>() where T : IStackObject
        {
            typeof(Stack)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(v => v.PropertyType == typeof(List<T>))
                .Select(v => v.GetValue(stackObject.Stack))
                .OfType<List<T>>()
                .ToList()
                .ForEach(v => v.Remove((T)stackObject));
            
            stackObject.Stack.Save();
        }
    }
}
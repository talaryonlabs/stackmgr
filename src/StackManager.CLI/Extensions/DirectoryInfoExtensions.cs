namespace Talaryon.StackManager.Extensions;

public static class DirectoryInfoExtensions
{
    extension(DirectoryInfo directory)
    {
        public FileInfo GetFile(string name) => new(Path.Combine(directory.FullName, name));
        public DirectoryInfo GetDirectory(string name) => new(Path.Combine(directory.FullName, name));
    }
}
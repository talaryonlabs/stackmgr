namespace stackmgr;

public interface IStackManagerEntity
{
    DirectoryInfo LocalDirectory { get; }
    string Name { get; }
}
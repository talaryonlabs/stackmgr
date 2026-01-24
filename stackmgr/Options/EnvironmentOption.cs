using System.CommandLine;

namespace stackmgr.Options;

public enum StackEnvironment { Production, Staging, Development }

public class EnvironmentOption() : Option<StackEnvironment>("--environment", "--env")
{
}
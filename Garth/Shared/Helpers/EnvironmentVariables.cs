using Shared.Exceptions;

namespace Shared.Helpers;

public enum EnvironmentVariableScope
{
    Process,
    User,
    System,
    Global
}

public static class EnvironmentVariables
{
    public static string? Get(string name, bool required = false, EnvironmentVariableScope scope = EnvironmentVariableScope.Global) =>
        ((scope) switch
        {
            EnvironmentVariableScope.Global =>
                Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process) ??
                Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User) ??
                Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine),
            EnvironmentVariableScope.Process =>
                Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process),
            EnvironmentVariableScope.User =>
                Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User),
            EnvironmentVariableScope.System =>
                Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine),
            _ => string.Empty
        }) ?? (required ? throw new EnvironmentVariableMissingException(name) : null);

    public static T? Get<T>(string name, bool required = false, EnvironmentVariableScope scope = EnvironmentVariableScope.Global, T? defaultValue = default)
    {
        string? value = Get(name, required, scope);
        return value is null ? defaultValue : (T?)Convert.ChangeType(value, typeof(T?));
    }
}
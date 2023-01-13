namespace Shared.Exceptions;

internal class EnvironmentVariableMissingException : Exception
{
    public EnvironmentVariableMissingException(string variableName) : base($"Required environment variable `{variableName}` is not defined!")
    {
        
    }
}
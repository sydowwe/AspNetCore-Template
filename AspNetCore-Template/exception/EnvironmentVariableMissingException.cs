namespace AspNetCore_Template.exception;

public class EnvironmentVariableMissingException(string name) : Exception($"Environment variable {name.ToUpper()} missing")
{
}
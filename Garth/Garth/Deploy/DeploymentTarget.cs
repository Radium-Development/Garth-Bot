namespace Garth.Deploy;

public class DeploymentTarget
{
    public TargetType TargetType { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? User { get; set; }
}
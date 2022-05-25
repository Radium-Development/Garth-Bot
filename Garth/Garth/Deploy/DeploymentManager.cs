using Spectre.Console;

namespace Garth.Deploy;

public class DeploymentManager
{
    private readonly DeploymentTarget[] _deploymentTargets = new[]
    {
        new DeploymentTarget()
        {
            Name = "Production",
            Address = "138.197.128.238",
            User = "deploy",
            TargetType = TargetType.Production
        }
    };
    
    public DeploymentManager()
    {
        AnsiConsole.MarkupLine("[bold black on red]Garth Deployment[/]");
        var server = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a [green]deployment target[/]")
                .PageSize(10)
                .AddChoices(_deploymentTargets.Select(t => t.Name!)));
        
        var selectedServer = _deploymentTargets.FirstOrDefault(t => t.Name == server);
    }
}
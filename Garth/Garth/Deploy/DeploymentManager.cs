using Renci.SshNet;
using Shared.Helpers;
using Spectre.Console;

namespace Garth.Deploy;

public class DeploymentManager
{
    
    public DeploymentManager()
    {
        AnsiConsole.MarkupLine("[bold black on red]ENSURE YOU COMMIT CHANGES TO GIT FIRST[/]");

        string TARGET_IP = EnvironmentVariables.Get("GARTH_DEPLOY_IP") ?? AnsiConsole.Ask<string>("Enter Deployment IP");
        string TARGET_USER = EnvironmentVariables.Get("GARTH_DEPLOY_USER") ?? AnsiConsole.Ask<string>("Enter Deployment User");
        string TARGET_PASSWORD = EnvironmentVariables.Get("GARTH_DEPLOY_PASSWORD") ?? AnsiConsole.Ask<string>("Enter Deployment Password");

        var connectionInfo = new ConnectionInfo(TARGET_IP, 22, TARGET_USER,
            new PasswordAuthenticationMethod(TARGET_USER, TARGET_PASSWORD));

        AnsiConsole.Clear();
        AnsiConsole.WriteLine("Starting deployment...");
        
        using (var client = new SshClient(connectionInfo))
        {
            AnsiConsole.WriteLine("Connecting to server...");
            client.Connect();
            AnsiConsole.WriteLine("Connecting...");
            var pull = client.RunCommand("cd /home/garth/Garth-Bot && git pull");
            var chown = client.RunCommand("chown garth:garth -R /home/garth/Garth-Bot");
            var restart = client.RunCommand("sudo systemctl restart Garth && sudo systemctl restart ChatGPT");
            foreach (var sshCommand in new [] {pull, chown, restart}.Where(t => t.ExitStatus != 0))
            {
                AnsiConsole.MarkupLine($"[red]ERR: [/][gray]{sshCommand.Error}[/]");
            }
            AnsiConsole.WriteLine("Deployment Complete!");
        }
    }
}
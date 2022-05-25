using System.Reflection;
using Garth.Deploy;
using MySql.Data.MySqlClient;

namespace Garth;

public class Program
{
    public static void Main(string[] args)
    {
        #if DEPLOY
        _ = new DeploymentManager();
        return;
        #endif
        
        _ = new Garth();
    }
}
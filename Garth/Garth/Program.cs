using System.Reflection;
using Garth.Deploy;
using MySql.Data.MySqlClient;

namespace Garth;

public class Program
{
    
    public static void Main(string[] args) =>
#if DEPLOY
        _ = new DeploymentManager();
#else
        _ = new Garth();
#endif
}
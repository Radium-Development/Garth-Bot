using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garth.IO
{
  public class Configuration : JsonInterfacer<Configuration.Config>
  {
    public class Config
    {
      public string? Token { get; set; }
      public string? TestingToken { get; set; }
      public string[]? Prefixes { get; set; }
    }

    public Configuration() : base("config.json", (loc) => throw new Exception($"Configuration File Created. Please update it.\n\t{loc}"))
    {
      
    }
  }
}

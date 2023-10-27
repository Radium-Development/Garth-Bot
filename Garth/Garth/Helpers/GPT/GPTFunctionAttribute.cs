namespace Garth.Helpers.GPT;

[AttributeUsage(AttributeTargets.Method)]
public class GPTFunctionAttribute : Attribute
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string ResultUsage { get; set; }

    public bool BypassGPT { get; set; } = false;
}
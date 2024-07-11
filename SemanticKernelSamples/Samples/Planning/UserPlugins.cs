using System.ComponentModel;

using Microsoft.SemanticKernel;

namespace SemanticKernelSamples.Samples.Planning;

internal class UserPlugins
{
    [KernelFunction, Description("Get user name by a user ID")]
    public Task<string> GetUserNameById(int id)
    {
        var name = id switch
        {
            7891 => "Alex Johnson",
            7892 => "Sophia Lee",
            _ => throw new Exception("User not found"),
        };
        return Task.FromResult(name);
    }
}
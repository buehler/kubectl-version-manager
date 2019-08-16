using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Kvm
{
    public static class Program
    {
        public static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Commands.Kvm>(args);
    }
}

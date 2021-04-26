// This attribute is for the Source Generator
// to add Components to the Compilation
[assembly: SourceGeneratedBlazor("Component1")]
[assembly: SourceGeneratedBlazor("Component2")]

namespace SampleWASM {
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    public class Program {
        public async static Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            await builder.Build().RunAsync();
        }
    }
}

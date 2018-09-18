using Flimsy.Api;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Flimsy {
    public class FlimsyApp {
        /// <summary>
        /// Loaded config.
        /// </summary>
        public static FlimsyConfig Config { get; set; }

        /// <summary>
        /// Initiate a new instance of the app engine.
        /// </summary>
        public static void Init(string[] args) {
            // Read config from JSON.
            Config = FlimsyConfig.Read(args);

            // Run RegisterRoutes() for each FlimsyModule.
            FlimsyInterfaceHandler.RunInterfaceFunction(
                FlimsyInterfaceHandler.InterfaceFunction.Module_RegisterRoutes,
                new FlimsyRouter());

            // Setup and run the WebHost engine.
            WebHost.CreateDefaultBuilder()
                .UseStartup<FlimsyRouteHandler>()
                .Build()
                .Run();
        }
    }
}
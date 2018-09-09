using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Flimsy {
    public class FlimsyConfig {
        #region Properties

        public bool Debug { get; set; }

        #endregion

        #region Helper functions

        /// <summary>
        /// Read config from file and return.
        /// </summary>
        /// <param name="args">Arguments from command-line.</param>
        /// <returns>Loaded config.</returns>
        public static FlimsyConfig Read(string[] args) {
            var file = args != null &&
                       args.Any()
                ? string.Join(" ", args)
                : Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "config.json");

            if (!File.Exists(file)) {
                return new FlimsyConfig();
            }

            return JsonConvert.DeserializeObject<FlimsyConfig>(
                File.ReadAllText(file));
        }

        #endregion
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Flimsy {
    public class FlimsyConfig {
        #region Properties

        public bool Debug { get; set; }

        public bool UseProdDatabase { get; set; }

        public FlimsyConfigDatabase TestDatabase { get; set; }

        public FlimsyConfigDatabase ProdDatabase { get; set; }

        public Dictionary<string, string> Settings { get; set; }

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

        #region Helper classes

        public class FlimsyConfigDatabase {
            public string Username { get; set; }

            public string Password { get; set; }

            public string Hostname { get; set; }

            public string Database { get; set; }

            public int? ConnectTimeout { get; set; }

            public int? MultipleActiveResultSets { get; set; }

            /// <summary>
            /// Get a compiled connection string.
            /// </summary>
            public override string ToString() {
                return string.Format(
                    "Data Source={0};Initial Catalog={1};User ID={2};Password={3};{4}{5}",
                    this.Hostname,
                    this.Database,
                    this.Username,
                    this.Password,
                    this.ConnectTimeout.HasValue
                        ? "Connect Timeout=" + this.ConnectTimeout.Value + ";"
                        : null,
                    this.MultipleActiveResultSets.HasValue
                        ? "MultipleActiveResultSets=" + this.MultipleActiveResultSets.Value + ";"
                        : null);
            }
        }

        #endregion
    }
}
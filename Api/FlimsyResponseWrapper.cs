using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flimsy.Api {
    public class FlimsyResponseWrapper {
        /// <summary>
        /// The HTTP statuscode of the response.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Object to serialize for body.
        /// </summary>
        public object Response { get; set; }

        /// <summary>
        /// The serialized version of the response.
        /// </summary>
        public string ResponseBody {
            get {
                return this.Response != null
                    ? JsonConvert.SerializeObject(this.Response)
                    : string.Empty;
            }
        }

        /// <summary>
        /// Response headers.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Stacktrace from exception.
        /// </summary>
        public string StackTrace { get; set; }
    }
}
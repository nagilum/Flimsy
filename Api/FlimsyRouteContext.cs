using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Flimsy.Api {
    public class FlimsyRouteContext {
        public HttpRequest Request { get; set; }

        public bool IsLocal { get; set; }

        public Dictionary<string, string> RequestHeaders { get; set; }

        public Dictionary<string, string> ResponseHeaders { get; set; }

        public Dictionary<string, string> Parameters { get; set; }

        public Dictionary<string, object> Objects { get; set; }

        public string Body { get; set; }

        public T BodyTo<T>() {
            try {
                return JsonConvert.DeserializeObject<T>(this.Body);
            }
            catch {
                return default(T);
            }
        }
    }
}
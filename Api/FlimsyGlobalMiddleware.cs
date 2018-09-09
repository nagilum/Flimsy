using System;
using System.Collections.Generic;

namespace Flimsy.Api {
    public class FlimsyGlobalMiddleware {
        /// <summary>
        /// Function to run.
        /// </summary>
        public Action<FlimsyRouteContext> Function { get; set; }

        /// <summary>
        /// Endpoints that will bypass this middleware.
        /// </summary>
        public List<FlimsyGlobalMiddlewareException> Exceptions { get; set; }
    }
}
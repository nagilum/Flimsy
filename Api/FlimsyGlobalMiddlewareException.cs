namespace Flimsy.Api {
    public class FlimsyGlobalMiddlewareException {
        /// <summary>
        /// HTTP method to match.
        /// </summary>
        public FlimsyRouter.HttpMethod HttpMethod { get; set; }

        /// <summary>
        /// URL endpoint to match.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// URL endpoint to match, split.
        /// </summary>
        public string[] EndpointSections { get; set; }

        /// <summary>
        /// Create a new middleware exception.
        /// </summary>
        public FlimsyGlobalMiddlewareException(
            FlimsyRouter.HttpMethod httpMethod,
            string endpoint) {

            this.HttpMethod = httpMethod;
            this.Endpoint = endpoint;
            this.EndpointSections = endpoint.Split('/');
        }
    }
}
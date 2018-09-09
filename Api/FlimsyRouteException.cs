using System;

namespace Flimsy.Api {
    public class FlimsyRouteException : Exception {
        /// <summary>
        /// The HTTP statuscode of the response.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Error message from throw.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Create a new instance of this error.
        /// </summary>
        /// <param name="statusCode">Statuscode for response.</param>
        /// <param name="errorMessage">Message for response.</param>
        public FlimsyRouteException(int statusCode, string errorMessage = null) {
            this.StatusCode = statusCode;
            this.ErrorMessage = errorMessage;
        }
    }
}
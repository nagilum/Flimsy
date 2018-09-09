using System;
using System.Collections.Generic;
using System.Linq;

namespace Flimsy.Api {
    public class FlimsyRouter {
        /// <summary>
        /// HTTP methods.
        /// </summary>
        public enum HttpMethod {
            DELETE,
            GET,
            HEAD,
            OPTIONS,
            PATCH,
            POST,
            PUT
        }

        /// <summary>
        /// REST method.
        /// </summary>
        private enum RestMethod {
            GetAll,
            Get,
            Create,
            Update,
            Delete
        }

        #region Global middleware

        /// <summary>
        /// Register a global middleware function.
        /// </summary>
        /// <param name="function">Function to run.</param>
        /// <param name="routeExceptions">Routes exempt from the middleware.</param>
        public void RegisterGlobalMiddleware(
            Action<FlimsyRouteContext> function,
            List<FlimsyGlobalMiddlewareException> routeExceptions = null) {

            if (FlimsyRouteHandler.GlobalMiddlewares == null) {
                FlimsyRouteHandler.GlobalMiddlewares =
                    new List<FlimsyGlobalMiddleware>();
            }

            var entry = FlimsyRouteHandler.GlobalMiddlewares
                .FirstOrDefault(n => n.Function == function);

            if (entry == null) {
                entry = new FlimsyGlobalMiddleware {
                    Function = function
                };

                FlimsyRouteHandler.GlobalMiddlewares.Add(entry);
            }

            if (entry.Exceptions == null) {
                entry.Exceptions = new List<FlimsyGlobalMiddlewareException>();
            }

            if (routeExceptions == null ||
                !routeExceptions.Any()) {

                return;
            }

            foreach (var routeException in routeExceptions) {
                if (entry.Exceptions.Any(n => n.HttpMethod == routeException.HttpMethod &&
                                              n.Endpoint == routeException.Endpoint)) {

                    continue;
                }

                entry.Exceptions.Add(
                    new FlimsyGlobalMiddlewareException(
                        routeException.HttpMethod,
                        routeException.Endpoint));
            }
        }

        #endregion

        #region Route functions

        /// <summary>
        /// Register a single API route.
        /// </summary>
        /// <param name="endpoint">Route endpoint.</param>
        /// <param name="httpMethod">HTTP method.</param>
        /// <param name="function">Function to run.</param>
        public void RegisterRouteFunction(
            string endpoint,
            HttpMethod httpMethod,
            Func<FlimsyRouteContext, object> function) {

            RegisterRouteFunction(
                endpoint,
                httpMethod,
                function,
                new List<Action<FlimsyRouteContext>>());
        }

        /// <summary>
        /// Register a single API route.
        /// </summary>
        /// <param name="endpoint">Route endpoint.</param>
        /// <param name="httpMethod">HTTP method.</param>
        /// <param name="function">Function to run.</param>
        /// <param name="middleware">Middleware function.</param>
        public void RegisterRouteFunction(
            string endpoint,
            HttpMethod httpMethod,
            Func<FlimsyRouteContext, object> function,
            Action<FlimsyRouteContext> middleware) {

            RegisterRouteFunction(
                endpoint,
                httpMethod,
                function,
                new List<Action<FlimsyRouteContext>> {
                    middleware
                });
        }

        /// <summary>
        /// Register a single API route.
        /// </summary>
        /// <param name="endpoint">Route endpoint.</param>
        /// <param name="httpMethod">HTTP method.</param>
        /// <param name="function">Function to run.</param>
        /// <param name="middlewares">List of middleware functions.</param>
        public void RegisterRouteFunction(
            string endpoint,
            HttpMethod httpMethod,
            Func<FlimsyRouteContext, object> function,
            List<Action<FlimsyRouteContext>> middlewares) {

            if (FlimsyRouteHandler.Routes == null) {
                FlimsyRouteHandler.Routes = new List<FlimsyRoute>();
            }

            if (endpoint.StartsWith("/")) {
                endpoint = endpoint.Substring(1);
            }

            if (endpoint.EndsWith("/")) {
                endpoint = endpoint.Substring(0, endpoint.Length - 1);
            }

            FlimsyRouteHandler.Routes.Add(
                new FlimsyRoute {
                    Endpoint = endpoint,
                    EndpointSections = endpoint.Split('/'),
                    HttpMethod = httpMethod,
                    Function = function,
                    MiddlewareFunctions = middlewares
                });
        }

        #endregion

        #region REST functions

        /// <summary>
        /// Register a new REST endpoint.
        /// </summary>
        /// <typeparam name="T">Class to handle the REST endpoints.</typeparam>
        /// <param name="endpoint">Route common endpoint.</param>
        public void RegisterRestFunction<T>(string endpoint) {
            RegisterRestFunction<T>(
                endpoint,
                new List<Action<FlimsyRouteContext>>());
        }

        /// <summary>
        /// Register a new REST endpoint.
        /// </summary>
        /// <typeparam name="T">Class to handle the REST endpoints.</typeparam>
        /// <param name="endpoint">Route common endpoint.</param>
        /// <param name="middleware">Middleware function.</param>
        public void RegisterRestFunction<T>(
            string endpoint,
            Action<FlimsyRouteContext> middleware) {

            RegisterRestFunction<T>(
                endpoint,
                new List<Action<FlimsyRouteContext>> {
                    middleware
                });
        }

        /// <summary>
        /// Register a new REST endpoint.
        /// </summary>
        /// <typeparam name="T">Class to handle the REST endpoints.</typeparam>
        /// <param name="endpoint">Route common endpoint.</param>
        /// <param name="middlewares">List of middleware functions.</param>
        public void RegisterRestFunction<T>(
            string endpoint,
            List<Action<FlimsyRouteContext>> middlewares) {

            if (FlimsyRouteHandler.Routes == null) {
                FlimsyRouteHandler.Routes = new List<FlimsyRoute>();
            }

            if (endpoint.StartsWith("/")) {
                endpoint = endpoint.Substring(1);
            }

            if (endpoint.EndsWith("/")) {
                endpoint = endpoint.Substring(0, endpoint.Length - 1);
            }

            var type = typeof(T);

            RegisterRestFunction(endpoint, type, RestMethod.GetAll, middlewares);
            RegisterRestFunction(endpoint, type, RestMethod.Get, middlewares);
            RegisterRestFunction(endpoint, type, RestMethod.Create, middlewares);
            RegisterRestFunction(endpoint, type, RestMethod.Update, middlewares);
            RegisterRestFunction(endpoint, type, RestMethod.Delete, middlewares);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="type"></param>
        /// <param name="restMethod"></param>
        /// <param name="middlewares"></param>
        private static void RegisterRestFunction(
            string endpoint,
            Type type,
            RestMethod restMethod,
            List<Action<FlimsyRouteContext>> middlewares) {

            var route = new FlimsyRoute {
                IsRestRoute = true,
                RestType = type,
                MiddlewareFunctions = middlewares
            };

            var method = type.GetMethod(restMethod.ToString());

            if (method == null) {
                return;
            }

            route.RestMethod = method;
            route.RestMethodIsStatic = method.IsStatic;

            // Endpoint and HTTP method.
            switch (restMethod) {
                case RestMethod.GetAll:
                    route.Endpoint = endpoint;
                    route.HttpMethod = HttpMethod.GET;

                    break;

                case RestMethod.Get:
                    route.Endpoint = endpoint + "/{id}";
                    route.HttpMethod = HttpMethod.GET;
                    route.RestHasIdParam = true;

                    break;

                case RestMethod.Create:
                    route.Endpoint = endpoint;
                    route.HttpMethod = HttpMethod.POST;
                    route.RestHasPayloadParam = true;

                    break;

                case RestMethod.Update:
                    route.Endpoint = endpoint + "/{id}";
                    route.HttpMethod = HttpMethod.POST;
                    route.RestHasIdParam = true;
                    route.RestHasPayloadParam = true;

                    break;

                case RestMethod.Delete:
                    route.Endpoint = endpoint + "/{id}";
                    route.HttpMethod = HttpMethod.DELETE;
                    route.RestHasIdParam = true;

                    break;
            }

            route.EndpointSections = route.Endpoint.Split('/');

            // ReturnType
            if (method.ReturnType != typeof(void)) {
                route.RestReturnType = method.ReturnType;
            }

            // Params
            var mparams = method.GetParameters();

            // Id param
            if (route.RestHasIdParam) {
                var found = false;

                foreach (var mparam in mparams) {
                    if (mparam.Name.ToLower() != "id") {
                        continue;
                    }

                    found = true;
                    route.RestIdParamType = mparam.ParameterType;
                    break;
                }

                if (!found) {
                    throw new Exception(
                        string.Format(
                            "Missing param 'id' in function {0}.{1}",
                            route.RestType.ToString(),
                            route.RestMethod.Name));
                }
            }

            // Payload param
            if (route.RestHasPayloadParam) {
                var found = false;

                foreach (var mparam in mparams) {
                    if (mparam.Name.ToLower() == "id") {
                        continue;
                    }

                    if (mparam.ParameterType == typeof(FlimsyRouteContext)) {
                        route.RestHasContextParam = true;
                    }
                    else {
                        found = true;
                        route.RestPayloadParamType = mparam.ParameterType;
                        break;
                    }
                }

                if (!found) {
                    throw new Exception(
                        string.Format(
                            "Missing payload param in function {0}.{1}",
                            route.RestType.ToString(),
                            route.RestMethod.Name));
                }
            }

            // Route Context?
            foreach (var mparam in mparams) {
                if (mparam.ParameterType == typeof(FlimsyRouteContext)) {
                    route.RestHasContextParam = true;
                }
            }

            // Done
            FlimsyRouteHandler.Routes.Add(route);
        }

        #endregion

        #region CORS functions

        /// <summary>
        /// Disable CORS.
        /// </summary>
        public void DisableCors() {
            FlimsyRouteHandler.CorsEnabled = false;
        }

        /// <summary>
        /// Enable CORS.
        /// </summary>
        /// <param name="origin">Allowed origins.</param>
        /// <param name="headers">Allowed headers.</param>
        public void EnableCors(string origin = "*", string headers = "Authorization, Content-Type") {
            FlimsyRouteHandler.CorsEnabled = true;
            FlimsyRouteHandler.CorsOrigin = origin;
            FlimsyRouteHandler.CorsHeaders = headers;
        }

        #endregion

        #region Config functions

        /// <summary>
        /// Set the base URL for the API.
        /// </summary>
        /// <param name="url">Base URL, will be added to all future register functions.</param>
        public void SetBaseUrl(string url) {
            FlimsyRouteHandler.BaseUrl = url;
        }

        #endregion
    }
}
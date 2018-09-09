using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Flimsy.Api {
    public class FlimsyRouteHandler {
        #region Routes

        /// <summary>
        /// List of all routes.
        /// </summary>
        public static List<FlimsyRoute> Routes { get; set; }

        /// <summary>
        /// List of all middleware functions.
        /// </summary>
        public static List<FlimsyGlobalMiddleware> GlobalMiddlewares { get; set; }

        /// <summary>
        /// Base URL for all endpoints.
        /// </summary>
        public static string BaseUrl { get; set; }

        #endregion

        #region CORS

        /// <summary>
        /// Whether CORS is enabled.
        /// </summary>
        public static bool CorsEnabled { get; set; }

        /// <summary>
        /// Allowed CORS origins.
        /// </summary>
        public static string CorsOrigin { get; set; }

        /// <summary>
        /// Allowed CORS headers.
        /// </summary>
        public static string CorsHeaders { get; set; }

        #endregion

        #region Route handling

        /// <summary>
        /// Setup the request handler internally.
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            app.Run(async context => {
                // Do we have any routes?
                if (Routes == null || !Routes.Any()) {
                    context.Response.StatusCode = 405;
                    await context.Response.WriteAsync(string.Empty);
                    return;
                }

                // Handle CORS.
                if (CorsEnabled && context.Request.Method.ToUpper() == "OPTIONS") {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", CorsOrigin);
                    context.Response.Headers.Add("Access-Control-Allow-Headers", CorsHeaders);
                    context.Response.Headers.Add("Content-Length", "0");
                    context.Response.StatusCode = 200;

                    await context.Response.WriteAsync(string.Empty);
                    return;
                }

                // Handle the request.
                var started = DateTimeOffset.Now;

                var wrapper = new FlimsyResponseWrapper {
                    Headers = new Dictionary<string, string>()
                };

                try {
                    var temp = await HandleRequest(context);

                    wrapper.StatusCode = temp.StatusCode;
                    wrapper.Headers = temp.Headers ?? new Dictionary<string, string>();
                    wrapper.Response = temp.Response;
                }
                catch (FlimsyRouteException ex) {
                    wrapper.StatusCode = ex.StatusCode;
                    wrapper.StackTrace = ex.StackTrace;

                    if (!string.IsNullOrWhiteSpace(ex.ErrorMessage)) {
                        wrapper.Response = new {
                            message = ex.ErrorMessage
                        };
                    }
                }
                catch (Exception ex) {
                    wrapper.StatusCode = 500;
                    wrapper.StackTrace = ex.StackTrace;

                    if (!string.IsNullOrWhiteSpace(ex.Message)) {
                        wrapper.Response = new {
                            message = ex.Message
                        };
                    }
                }

                var ended = DateTimeOffset.Now;
                var duration = ended - started;
                
                // Iz debug?
                if (FlimsyApp.Config.Debug) {
                    wrapper.Headers.Add("X-REQUEST-STARTED", started.ToString("yyyy-MM-dd HH:mm:ss"));
                    wrapper.Headers.Add("X-REQUEST-ENDED", ended.ToString("yyyy-MM-dd HH:mm:ss"));
                    wrapper.Headers.Add("X-REQUEST-DURATION", duration.ToString());
                    wrapper.Headers.Add("X-REQUEST-DURATION-MS", duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                }

                // Output the response.
                context.Response.StatusCode = wrapper.StatusCode;

                if (wrapper.ResponseBody != null &&
                    !wrapper.Headers.ContainsKey("Content-Type")) {

                    wrapper.Headers.Add(
                        "Content-Type",
                        "application/json; charset=utf-8");
                }

                if (wrapper.Headers.Any()) {
                    foreach (var header in wrapper.Headers) {
                        context.Response.Headers.Add(
                            header.Key,
                            header.Value);
                    }
                }

                await context.Response.WriteAsync(wrapper.ResponseBody);
            });
        }

        /// <summary>
        /// Handle the routing and execute function.
        /// </summary>
        private async Task<FlimsyResponseWrapper> HandleRequest(HttpContext context) {
            // Prepare context.
            var ctx = new FlimsyRouteContext {
                Request = context.Request,
                IsLocal = context.Request.Host.Host == "localhost",
                RequestHeaders = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                ResponseHeaders = new Dictionary<string, string>(),
                Parameters = new Dictionary<string, string>(),
                Objects = new Dictionary<string, object>()
            };

            try {
                ctx.Body = new StreamReader(ctx.Request.Body).ReadToEnd();
            }
            catch {
                //
            }

            // Prepare the wrapper.
            var wrapper = new FlimsyResponseWrapper {
                Headers = new Dictionary<string, string>()
            };

            // Figure out the route.
            var url = ctx.Request.Path.Value;

            if (url.StartsWith("/")) {
                url = url.Substring(1);
            }

            if (BaseUrl != null &&
                url.StartsWith(BaseUrl + "/")) {

                url = url.Substring(BaseUrl.Length + 1);
            }

            var sections = url.Split('/');
            var routes = Routes
                .Where(n => n.HttpMethod.ToString() == ctx.Request.Method.ToUpper() &&
                            n.EndpointSections.Length == sections.Length)
                .ToList();

            if (!routes.Any()) {
                return new FlimsyResponseWrapper {
                    StatusCode = 405
                };
            }

            var route = null as FlimsyRoute;

            foreach (var temp in routes) {
                ctx.Parameters.Clear();

                var matches = 0;

                for (var i = 0; i < sections.Length; i++) {
                    if (temp.EndpointSections[i] == sections[i]) {
                        matches++;
                    }
                    else if (temp.EndpointSections[i].StartsWith("{") &&
                             temp.EndpointSections[i].EndsWith("}")) {

                        ctx.Parameters.Add(
                            temp.EndpointSections[i].Substring(1, temp.EndpointSections[i].Length - 2),
                            sections[i]);

                        matches++;
                    }
                    else {
                        break;
                    }
                }

                if (matches != sections.Length) {
                    continue;
                }

                route = temp;
                break;
            }

            // No route found.
            if (route == null) {
                return new FlimsyResponseWrapper {
                    StatusCode = 404
                };
            }

            // Execute all global middleware.
            if (GlobalMiddlewares != null &&
                GlobalMiddlewares.Any()) {

                foreach (var gw in GlobalMiddlewares) {
                    var exmatch = false;

                    if (gw.Exceptions != null &&
                        gw.Exceptions.Any()) {

                        var exceptions = gw.Exceptions
                            .Where(n => n.HttpMethod.ToString() == ctx.Request.Method.ToUpper() &&
                                        n.EndpointSections.Length == sections.Length)
                            .ToList();

                        if (exceptions.Any()) {
                            foreach (var exception in exceptions) {
                                var matches = 0;

                                for (var i = 0; i < sections.Length; i++) {
                                    if (exception.EndpointSections[i] == sections[i]) {
                                        matches++;
                                    }
                                    else if (exception.EndpointSections[i].StartsWith("{") &&
                                             exception.EndpointSections[i].EndsWith("}")) {

                                        matches++;
                                    }
                                    else {
                                        break;
                                    }
                                }

                                if (matches != sections.Length) {
                                    continue;
                                }

                                exmatch = true;
                                break;
                            }
                        }
                    }

                    if (exmatch) {
                        continue;
                    }

                    gw.Function.Invoke(ctx);
                }
            }

            // Execute route specific middleware.
            if (route.MiddlewareFunctions != null &&
                route.MiddlewareFunctions.Any()) {

                foreach (var mw in route.MiddlewareFunctions) {
                    mw.Invoke(ctx);
                }
            }

            // Execute route function or REST function.
            if (route.IsRestRoute) {
                var restParams = new List<object>();

                // Add 'id' param.
                if (route.RestHasIdParam) {
                    var added = false;

                    if (ctx.Parameters.ContainsKey("id")) {
                        var code = Type.GetTypeCode(route.RestIdParamType);

                        switch (code) {
                            case TypeCode.Int32: {
                                if (int.TryParse(ctx.Parameters["id"], out var id)) {
                                    restParams.Add(id);
                                    added = true;
                                }

                                break;
                            }
                            case TypeCode.Int64: {
                                if (long.TryParse(ctx.Parameters["id"], out var id)) {
                                    restParams.Add(id);
                                    added = true;
                                }

                                break;
                            }
                            default:
                                restParams.Add(ctx.Parameters["id"]);
                                added = true;
                                break;
                        }
                    }

                    if (!added) {
                        throw new Exception("Missing 'id' from route, or parse error.");
                    }
                }

                // Add body payload.
                if (route.RestHasPayloadParam) {
                    var obj = JsonConvert.DeserializeAnonymousType(
                        ctx.Body,
                        route.RestPayloadParamType);

                    restParams.Add(obj);
                }

                // Add the 'ctx' param.
                if (route.RestHasContextParam) {
                    restParams.Add(ctx);
                }

                var x = Activator.CreateInstance(route.RestType);
                var method = route.RestType.GetMethod(route.RestMethod.Name);

                await Task.Run(() => {
                    try {
                        wrapper.Response = method.Invoke(x, restParams.ToArray());
                    }
                    catch (FlimsyRouteException ex) {
                        wrapper.StatusCode = ex.StatusCode;
                        wrapper.StackTrace = ex.StackTrace;

                        if (!string.IsNullOrWhiteSpace(ex.ErrorMessage)) {
                            wrapper.Response = new {
                                message = ex.ErrorMessage
                            };
                        }
                    }
                    catch (Exception ex) {
                        if (ex.InnerException != null &&
                            ex.InnerException is FlimsyRouteException) {

                            var frex = (FlimsyRouteException) ex.InnerException;

                            wrapper.StatusCode = frex.StatusCode;
                            wrapper.StackTrace = frex.StackTrace;

                            if (!string.IsNullOrWhiteSpace(frex.ErrorMessage)) {
                                wrapper.Response = new {
                                    message = frex.ErrorMessage
                                };
                            }
                        }
                        else {
                            wrapper.StatusCode = 500;
                            wrapper.StackTrace = ex.StackTrace;

                            if (!string.IsNullOrWhiteSpace(ex.Message)) {
                                wrapper.Response = new {
                                    message = ex.Message
                                };
                            }
                        }
                    }
                });
            }
            else {
                await Task.Run(() => {
                    try {
                        wrapper.Response = route.Function.Invoke(ctx);
                    }
                    catch (FlimsyRouteException ex) {
                        wrapper.StatusCode = ex.StatusCode;
                        wrapper.StackTrace = ex.StackTrace;

                        if (!string.IsNullOrWhiteSpace(ex.ErrorMessage)) {
                            wrapper.Response = new {
                                message = ex.ErrorMessage
                            };
                        }
                    }
                    catch (Exception ex) {
                        if (ex.InnerException != null &&
                            ex.InnerException is FlimsyRouteException) {

                            var frex = (FlimsyRouteException) ex.InnerException;

                            wrapper.StatusCode = frex.StatusCode;
                            wrapper.StackTrace = frex.StackTrace;

                            if (!string.IsNullOrWhiteSpace(frex.ErrorMessage)) {
                                wrapper.Response = new {
                                    message = frex.ErrorMessage
                                };
                            }
                        }
                        else {
                            wrapper.StatusCode = 500;
                            wrapper.StackTrace = ex.StackTrace;

                            if (!string.IsNullOrWhiteSpace(ex.Message)) {
                                wrapper.Response = new {
                                    message = ex.Message
                                };
                            }
                        }
                    }
                });
            }

            if (ctx.ResponseHeaders.Any()) {
                foreach (var rh in ctx.ResponseHeaders) {
                    if (wrapper.Headers.ContainsKey(rh.Key)) {
                        continue;
                    }

                    wrapper.Headers.Add(
                        rh.Key,
                        rh.Value);
                }
            }

            if (!wrapper.Headers.ContainsKey("Content-Length")) {
                wrapper.Headers.Add(
                    "Content-Length",
                    !string.IsNullOrWhiteSpace(wrapper.ResponseBody)
                        ? wrapper.ResponseBody.Length.ToString()
                        : "0");
            }

            if (wrapper.StatusCode == 0) {
                wrapper.StatusCode = !string.IsNullOrWhiteSpace(wrapper.ResponseBody)
                    ? 200
                    : 204;
            }

            // We're done here.
            return wrapper;
        }

        #endregion
    }
}
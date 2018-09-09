using System;
using System.Collections.Generic;
using System.Reflection;

namespace Flimsy.Api {
    public class FlimsyRoute {
        public string Endpoint { get; set; }

        public string[] EndpointSections { get; set; }

        public FlimsyRouter.HttpMethod HttpMethod { get; set; }

        public List<Action<FlimsyRouteContext>> MiddlewareFunctions { get; set; }

        #region Function route

        public Func<FlimsyRouteContext, object> Function { get; set; }

        #endregion

        #region Rest route

        public bool IsRestRoute { get; set; }

        public Type RestType { get; set; }

        public Type RestReturnType { get; set; }

        public MethodInfo RestMethod { get; set; }

        public bool RestMethodIsStatic { get; set; }

        public bool RestHasIdParam { get; set; }

        public bool RestHasPayloadParam { get; set; }

        public bool RestHasContextParam { get; set; }

        public Type RestIdParamType { get; set; }

        public Type RestPayloadParamType { get; set; }

        #endregion
    }
}
using Flimsy.Api;

namespace Flimsy.Interfaces {
    public interface IFlimsyModule {
        /// <summary>
        /// Allow each module to register their routes.
        /// </summary>
        void OnRegisterRoutes(FlimsyRouter router);
    }
}
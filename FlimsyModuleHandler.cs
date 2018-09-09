using System;
using System.Collections.Generic;
using System.Linq;
using Flimsy.Api;
using Flimsy.Interfaces;

namespace Flimsy {
    public class FlimsyModuleHandler {
        /// <summary>
        /// All available module functions.
        /// </summary>
        public enum ModuleFunction {
            OnRegisterRoutes
        }

        /// <summary>
        /// Run a specific function.
        /// </summary>
        /// <param name="function">Function to run.</param>
        public static void RunModuleFunction(ModuleFunction function) {
            var router = new FlimsyRouter();

            foreach (var module in GetModules()) {
                try {
                    var instance = Activator.CreateInstance(module) as IFlimsyModule;

                    if (instance == null) {
                        throw new Exception(string.Format(
                            "Could not create instance of {0}",
                            module.FullName));
                    }

                    switch (function) {
                        case ModuleFunction.OnRegisterRoutes:
                            instance.OnRegisterRoutes(router);
                            break;
                    }
                }
                catch (Exception ex) {
                    // TODO: Log unhandeled exception as critical.
                }
            }
        }

        /// <summary>
        /// Get a list of all classes implementing IBaseModule.
        /// </summary>
        private static IEnumerable<Type> GetModules() {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(n => n.GetTypes())
                .Where(n => typeof(IFlimsyModule).IsAssignableFrom(n) &&
                            !n.IsInterface &&
                            !n.IsAbstract);
        }
    }
}
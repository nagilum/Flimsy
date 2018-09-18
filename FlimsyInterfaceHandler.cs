using System;
using System.Collections.Generic;
using System.Linq;
using Flimsy.Interfaces;

namespace Flimsy {
    public class FlimsyInterfaceHandler {
        /// <summary>
        /// All available interface functions.
        /// </summary>
        public enum InterfaceFunction {
            Module_RegisterRoutes,
            Logger_NewEntry
        }

        /// <summary>
        /// Run a specific interface function.
        /// </summary>
        /// <typeparam name="T">Type of payload to pass along.</typeparam>
        /// <param name="function">Function to run.</param>
        /// <param name="payload">Payload to pass along to the run function.</param>
        public static void RunInterfaceFunction<T>(InterfaceFunction function, T payload) {
            switch (function) {
                // IFlimsyModule
                case InterfaceFunction.Module_RegisterRoutes:
                    foreach (var cls in GetInterfaceClasses<IFlimsyModule>()) {
                        var instance = Activator.CreateInstance(cls) as IFlimsyModule;

                        if (instance == null) {
                            throw new Exception(
                                string.Format(
                                    "Could not create instance of {0}",
                                    cls.FullName));
                        }

                        switch (function) {
                            // RegisterRoutes
                            case InterfaceFunction.Module_RegisterRoutes:
                                instance.RegisterRoutes((dynamic) payload);
                                break;
                        }
                    }

                    break;

                case InterfaceFunction.Logger_NewEntry:
                    // TODO !!!
                    break;
            }
        }

        /// <summary>
        /// Get a list of all classes implementing given interface.
        /// </summary>
        /// <returns>List of classes.</returns>
        private static IEnumerable<Type> GetInterfaceClasses<T>() {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(n => n.GetTypes())
                .Where(n => typeof(T).IsAssignableFrom(n) &&
                            !n.IsInterface &&
                            !n.IsAbstract);
        }
    }
}
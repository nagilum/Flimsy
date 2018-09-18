using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flimsy.Logger;

namespace Flimsy.Interfaces {
    public interface IFlimsyLogger {
        void Debug(FlimsyLog entry);
    }
}
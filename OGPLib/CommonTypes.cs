using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGPLib {
    public enum ResponseStatus {
        SUCCESS,
        INVALID_ACTION, // GameInput could not be understood
        ILLEGAL_ACTION, // GameInput was successfully parsed, but action was not legal by the game rules
    }
}

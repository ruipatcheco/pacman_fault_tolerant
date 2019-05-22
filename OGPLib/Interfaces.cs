using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGPLib {
    /// <summary>
    /// Represents the remote Game object. The implementation of this interface must be done server-side.
    /// </summary>
    public interface IRemoteGame : IRemoteEntity {
        /// <summary>
        /// Submits an action to the server. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        GameParameters register(OGPPlayer player, bool primary);
        void setState(GameState state);
        ResponseStatus sumbitInput(GameInput input, OGPPlayer player);
        List<OGPPlayer> getPlayerList();
        void SetAsPrimary();
    }

    public interface IRemoteChat {
        ResponseStatus deliverMessage(ChatMessage message);
    }

    public interface IRemoteClient : IRemoteEntity {
        void begin(GameState initialState);
        void deliverState(GameState state);
    }

    public interface IRemoteEntity
    {
        string LocalState(string pid, int round_id);
        string ping();
        void SetDelay(int d);
    }

}

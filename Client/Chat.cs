using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Remoting;
using OGPLib;
using OGPLib.Multicast;

namespace Client {
    public class Chat {
        private class EnqueuedChatMessage {
            public ChatMessage message;
            public bool remove = false;

            public EnqueuedChatMessage(ChatMessage message) {
                this.message = message ?? throw new ArgumentNullException(nameof(message));
            }
        }
        private RemoteReliableMulticast multicast;
        private IRemoteGame game;
        public static readonly string CHAT_REMOTE_NAME = "RemoteChat";
        private OGPPlayer self;
        private List<OGPPlayer> participants;
        private List<ChatMessage> messages = new List<ChatMessage>();
        private List<EnqueuedChatMessage> delayQ = new List<EnqueuedChatMessage>();
        private List<Task> deliverCheckPool = new List<Task>();
        private readonly GameParameters parameters;
        private VectorClock localClock;
        private bool enabled = false;

        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;

        public Chat(Pacman game) : this(game.Game, game.Player1, game.Players, game.Parameters) {
        }

        public Chat(IRemoteGame game, OGPPlayer sender, List<OGPPlayer> players, GameParameters parameters) {
            var participants_uris = new HashSet<string>();
            List<string> procNames = new List<string>();

            this.game = game ?? throw new ArgumentNullException(nameof(game));
            this.self = sender ?? throw new ArgumentNullException(nameof(sender));
            this.participants = players ?? throw new ArgumentNullException(nameof(players));
            this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

            foreach (var player in players) {
                procNames.Add(player.ChatProcessName);
                participants_uris.Add(player.ChatUri);
            }

            this.localClock = new VectorClock(procNames);
            this.multicast = new RemoteReliableMulticast(participants_uris, sender.ChatUri);
            this.multicast.MessageDeliveredEvent += multicastDeliverMessageEventHandler;
        }

        ~Chat() { //Make sure message pool list finalizes all tasks before exiting
            Task.WaitAll(this.deliverCheckPool.ToArray());
        }

        public List<OGPPlayer> Participants { get => participants; set => participants = value; }

        public void broadcast(string message) {
            ChatMessage m = new ChatMessage(this.Self, message, localClock);
            this.localClock.increment(this.Self.ChatProcessName);
            multicast.broadcast(m);

        }

        public ResponseStatus deliverMessage(ChatMessage message) {
            if (this.canDeliver(message)) {
                this.addNewMessage(message);
                if (message.Sender.ChatProcessName != self.ChatProcessName) {
                    this.incrementClock(message.Sender.ChatProcessName);
                }
            } else {
                this.delayQ.Add(new EnqueuedChatMessage(message));
            }
            
            return ResponseStatus.SUCCESS;
        }

        private bool canDeliver(ChatMessage message) {
            string sender = message.Sender.ChatProcessName;
            // check if we delivered all previous messages from sender
            if ((message.Clock.clockAt(sender) - 1) == (this.localClock.clockAt(sender))) {
                // check if we delivered all messages the sender delivered
                foreach (var clock in this.localClock.Clocks) {
                    //ignore self or sender vector clock values
                    if (clock.Key == this.self.ChatProcessName || clock.Key == message.Sender.ChatProcessName) {
                        continue;
                    }
                    if (clock.Value < message.Clock.clockAt(clock.Key)) {
                        return false;
                    }
                }
                return true;
            } else if (message.Sender.ChatProcessName == self.ChatProcessName) return true;
            return false;
        }

        private void incrementClock(string key) {
            if (key == null) key = this.Self.ChatProcessName;
            this.localClock.increment(key);
            this.deliverCheckPool.Add(Task.Run(delegate () {
                this.updateDeliveredMessages();
            }));
        }

        private void updateDeliveredMessages() {
            foreach (var enq_message in this.delayQ) {
                if (!enq_message.remove && this.canDeliver(enq_message.message)) {
                    this.addNewMessage(enq_message.message);
                    if (enq_message.message.Sender.ChatProcessName != this.Self.ChatProcessName) {
                        this.incrementClock(enq_message.message.Sender.ChatProcessName);
                    }
                    enq_message.remove = true;
                    return;
                }
            }
        }

        private void cleanDelayQ() {
            //removing from list we are iterating over is a bad idea
            var removeList = new List<EnqueuedChatMessage>();
            foreach (var message in delayQ) {
                if (message.remove) {
                    removeList.Add(message);
                }
            }
            foreach (var message in removeList) {
                delayQ.Remove(message);
            }
        }

        public void addNewMessage(ChatMessage message) {
            this.messages.Add(message);
            this.MessageReceivedEvent?.Invoke(this,
                new MessageReceivedEventArgs(message.Message, message.Sender.Username));
        }

        /// <summary>
        /// Exposes chat for other client to connect to. Multiple calls to this function only execute the firt one.
        /// </summary>
        public void enable() {
            if (!enabled) {
                RemotingServices.Marshal(this.multicast, Chat.CHAT_REMOTE_NAME, typeof(RemoteReliableMulticast));
                enabled = true;
            }
        }

        /// <summary>
        /// Serializes all cached messages into a single string, sorting with respect to causal order.
        /// Output string looks like:
        /// <user1>: hello world!
        /// <user2>: hi!
        /// ...
        /// </summary>
        public string Messages { //TODO return array of strings
            get {
                string msgs = "";
                foreach (var message in messages) {
                    msgs += string.Format("<{0}>: {1}\r\n", message.Sender.Username, message.Message);
                }
                return msgs;
            }
        }

        private void multicastDeliverMessageEventHandler(object sender, MessageDeliveredEventArgs args) {
            ChatMessage message = args.Message as ChatMessage;
            this.deliverMessage(message);
        }

        public OGPPlayer Self { get => self; set => self = value; }
    }

    public class MessageReceivedEventArgs : EventArgs {
        public string Message;
        public string Sender;

        public MessageReceivedEventArgs(string message, string sender) {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }
    }

}

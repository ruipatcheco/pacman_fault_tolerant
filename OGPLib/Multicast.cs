using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace OGPLib {
    namespace Multicast {
        /// <summary>
        /// Interface representing all remote operations a client can do on a multicast instance
        /// </summary>
        public interface IRemoteMulticastOps {
            ResponseStatus onReceive(MulticastMessage message);
        }

        /// <summary>
        /// Implementation of IRemoteMulticastOps using Reliable Multicast
        /// </summary>
        public class RemoteReliableMulticast : MarshalByRefObject, IRemoteMulticastOps {
            private RMulticast multicast;
            public event EventHandler<MessageDeliveredEventArgs> MessageDeliveredEvent;

            /// <summary>
            /// </summary>
            /// <param name="participants">participants that will be receiving and sending messages in multicast session</param>
            /// <param name="selfId">unique identifier of process that is constructing this object</param>
            public RemoteReliableMulticast (IEnumerable<MulticastParticipant> participants, string selfId) {
                postInit(participants, selfId);
            }

            public RemoteReliableMulticast(IEnumerable<string> participants_uris, string selfId) {
                HashSet<MulticastParticipant> participant_set = new HashSet<MulticastParticipant>();
                foreach (var p in participants_uris) {
                    IRemoteMulticastOps multicast = Activator.GetObject(typeof(IRemoteMulticastOps), p) as IRemoteMulticastOps;
                    participant_set.Add(new MulticastParticipant(multicast, p));
                }
                postInit(participant_set, selfId);
            }

            private void postInit(IEnumerable<MulticastParticipant> participants, string selfId) {
                //set up participants data structure
                var _participants = participants as HashSet<MulticastParticipant>;
                if (_participants == null) _participants = new HashSet<MulticastParticipant>(participants);

                //construct Reliable Multicast
                multicast = new RMulticast(_participants, selfId);

                //wrap Reliable multicast message delivered event. This is not strictly necessary, but helps readibility of
                //higher layers.
                multicast.MessageDeliveredEvent += messageDeliveredHandler;
            }

            public ResponseStatus onReceive(MulticastMessage message) {
                multicast.BMulticast.deliver(message);
                return ResponseStatus.SUCCESS;
            }

            public void broadcast(object message) {
                MulticastMessage m = new MulticastMessage(message);
                multicast.broadcast(m);
            }

            private void messageDeliveredHandler(object sender, MessageDeliveredEventArgs args) {
                MessageDeliveredEvent?.Invoke(this, args);
            }
        }


        public class MulticastParticipant {
            private IRemoteMulticastOps remoteObject;
            private string id;

            public MulticastParticipant(IRemoteMulticastOps remoteObject, string id) {
                this.remoteObject = remoteObject ?? throw new ArgumentNullException(nameof(remoteObject));
                this.id = id;
            }

            public IRemoteMulticastOps RemoteObject { get => remoteObject; set => remoteObject = value; }
            public string Id { get => id; set => id = value; }
        }

        public class MessageDeliveredEventArgs : EventArgs {
            internal MulticastMessage message;

            public MessageDeliveredEventArgs(MulticastMessage message) {
                this.message = message ?? throw new ArgumentNullException(nameof(message));
            }

            public object Message { get => message.message; }
        }

        [Serializable]
        public class MulticastMessage {
            private string msgId;
            public string SenderId;
            public object message;

            public MulticastMessage(string msgId, object message) {
                this.msgId = msgId;
                this.message = message ?? throw new ArgumentNullException(nameof(message));
            }

            public MulticastMessage(object message) : this(Guid.NewGuid().ToString(), message) {
            }

            public string MsgId { get => msgId; }
        }

        public abstract class AbstractMulticast {
            protected HashSet<MulticastParticipant> participants;

            public IEnumerable<MulticastParticipant> Participants { get => participants; }

            public AbstractMulticast(HashSet<MulticastParticipant> participants) {
                this.participants = participants ?? throw new ArgumentNullException(nameof(participants));
            }

            public AbstractMulticast() {
                participants = new HashSet<MulticastParticipant>();
            }

            public event EventHandler<MessageDeliveredEventArgs> MessageDeliveredEvent;

            public abstract void broadcast(MulticastMessage message);
            public abstract void concreteDeliver(MulticastMessage message);

            public void deliver(MulticastMessage message) {
                concreteDeliver(message);
                MessageDeliveredEvent?.Invoke(this, new MessageDeliveredEventArgs(message));
            }
            public virtual void addParticipant(MulticastParticipant participant) {
                participants.Add(participant);
            }
            public virtual void removeParticipant(MulticastParticipant participant) {
                participants.Remove(participant);
            }
        }

        public class BMulticast : AbstractMulticast {
            List<Task> tasks = new List<Task>();

            public BMulticast() : base() {
            }

            public BMulticast(HashSet<MulticastParticipant> participants) : base(participants) {
            }

            ~BMulticast() { //Make sure we try to deliver everything before exiting 
                Task.WaitAll(tasks.ToArray());
            }

            public override void broadcast(MulticastMessage message) {
                lock (this) {
                    foreach (var p in participants) {
                        tasks.Add(Task.Factory.StartNew(delegate () {
                            p.RemoteObject.onReceive(message);
                        }));
                    }
                }
            }

            public override void concreteDeliver(MulticastMessage message) {
            }
        }

        public class RMulticast : AbstractMulticast {
            private string selfId;
            private BMulticast bMulticast;
            private Dictionary<string, MulticastMessage> messages = new Dictionary<string, MulticastMessage>();

            public BMulticast BMulticast { get => bMulticast; }

            public RMulticast(HashSet<MulticastParticipant> participants, string selfId) {
                this.selfId = selfId ?? throw new ArgumentNullException(nameof(selfId));

                HashSet<MulticastParticipant> _participants = new HashSet<MulticastParticipant>(participants);
                foreach(var p in _participants) {
                    if (p.Id == selfId) _participants.Remove(p);
                    break;
                }

                bMulticast = new BMulticast(_participants);
                bMulticast.MessageDeliveredEvent += bDeliverEventHandler;
                this.participants = participants;
            }

            public RMulticast(string selfId) : this(new HashSet<MulticastParticipant>(), selfId) {
            }

            private bool haveMessage(MulticastMessage message) {
                return messages.ContainsKey(message.MsgId);
            }

            public override void broadcast(MulticastMessage message) {
                bMulticast.broadcast(message);
                bMulticast.deliver(message);
            }

            public override void concreteDeliver(MulticastMessage message) {
            }

            public override void addParticipant(MulticastParticipant participant) {
                this.bMulticast.addParticipant(participant);
            }

            public override void removeParticipant(MulticastParticipant participant) {
                this.bMulticast.removeParticipant(participant);
            }

            public void bDeliverEventHandler(object sender, MessageDeliveredEventArgs args) {
                var basicMulticast = sender as BMulticast;
                var message = args.message as MulticastMessage;
                lock (this) {
                    // Are we receiving from right stream and did we not receive this message already?
                    if (basicMulticast == this.bMulticast && !haveMessage(message)) {
                        this.messages.Add(message.MsgId, message);
                        if (message.SenderId != this.selfId) {
                            this.bMulticast.broadcast(message);
                        }
                        this.deliver(message);
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGPLib {
    [Serializable]
    public class ChatMessage : IComparable<ChatMessage> {
        private OGPPlayer sender;
        private string message;
        private VectorClock clock;

        public ChatMessage(OGPPlayer sender, string message, VectorClock clock) {
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
            this.message = message ?? throw new ArgumentNullException(nameof(message));
            this.Clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public OGPPlayer Sender { get => sender; set => sender = value; }
        public string Message { get => message; set => message = value; }
        public VectorClock Clock { get => clock; set => clock = value; }

        public int CompareTo(ChatMessage other) {
            if (this.Clock == other.Clock) return 0;
            if (this.Clock <= other.Clock) return -1;
            return 1;
        }
    }

    [Serializable]
    public class VectorClock {
        private Dictionary<string, int> clocks = new Dictionary<string, int>();

        public Dictionary<string, int> Clocks { get => clocks; set => clocks = value; }

        public VectorClock(IEnumerable<string> processes) {
            foreach (var p in processes) {
                clocks.Add(p, 0);
            }
        }

        public VectorClock(IEnumerable<Tuple<string, int>> clocks) {
            foreach (var i in clocks) {
                this.clocks.Add(i.Item1, i.Item2);
            }
        }

        public VectorClock(params Tuple<string, int>[] clocks) {
            for (int i = 0; i < clocks.Length; i++) {
                this.clocks.Add(clocks[i].Item1, clocks[i].Item2);
            }
        }

        public void increment(string process) {
            this.clocks[process]++;
        }

        public static bool operator ==(VectorClock first, VectorClock second) {
            if (first.clocks.Count != second.clocks.Count) return false;
            foreach (var i in first.clocks) {
                if (first.clockAt(i.Key) != second.clockAt(i.Key)) return false;
            }
            return true;
        }

        public static bool operator !=(VectorClock first, VectorClock second) {
            return !(first == second);
        }

        public static bool operator >=(VectorClock first, VectorClock second) {
            if (first.clocks.Count != second.clocks.Count) throw new ArgumentException(
                "Vector clocks you are trying to compare are not comparable (different sizes)");
            foreach (var i in first.clocks) {
                if (first.clockAt(i.Key) < second.clockAt(i.Key)) return false;
            }
            return true;
        }

        public static bool operator <=(VectorClock first, VectorClock second) {
            if (first.clocks.Count != second.clocks.Count) throw new ArgumentException(
                "Vector clocks you are trying to compare are not comparable (different sizes)");
            foreach (var i in first.clocks) {
                if (first.clockAt(i.Key) > second.clockAt(i.Key)) return false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns>clock value of process `i`</returns>
        public int clockAt(string i) {
            return clocks[i];
        }

        public override string ToString() {
            return base.ToString();
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace pacman {
    public class Utils {
        public static void doInGUI(Form form, Action action) {
            if(form.InvokeRequired) {
                form.BeginInvoke(action);
            } else {
                action();
            }
        }
    }
    public static class ExtensionMethods {
        // Deep clone
        public static T DeepClone<T>(this T a) {
            using (MemoryStream stream = new MemoryStream()) {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T) formatter.Deserialize(stream);
            }
        }
    }
}

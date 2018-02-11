using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameEngineX.Utility {
    public static class SerializationUtils {
        //[AttributeUsage(AttributeTargets.Property, Inherited = true)]
        //public class Serializable : Attribute {
        //}

        public static IEnumerable<string> Serialize(params object[] objects) {
            using (MemoryStream stream = new MemoryStream()) {
                BinaryFormatter formatter = new BinaryFormatter();

                foreach (object o in objects) {
                    formatter.Serialize(stream, o);
                    stream.Flush();
                    stream.Position = 0;

                    yield return Convert.ToBase64String(stream.ToArray());
                }
            }
        }

        public static IEnumerable<object> Deserialize(params string[] dataStrings) {
            using (MemoryStream stream = new MemoryStream()) {
                BinaryFormatter formatter = new BinaryFormatter();

                foreach (string dataString in dataStrings) {
                    byte[] data = Convert.FromBase64String(dataString);
                    stream.Position = 0;
                    stream.Write(data, 0, data.Length);
                    stream.SetLength(data.Length);
                    stream.Seek(0, SeekOrigin.Begin);

                    yield return formatter.Deserialize(stream);
                }
            }
        }

    }
}

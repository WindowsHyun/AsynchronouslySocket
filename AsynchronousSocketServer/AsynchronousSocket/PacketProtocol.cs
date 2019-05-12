using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace publicPacketProtocol {
  public class PacketProtocol {

    [Serializable]
    public enum packetType {
      char_pos = 0
    }

    [Serializable]
    public class Packet {
      public int packet_Type;

      public Packet() {
        this.packet_Type = 0;
      }

      public static byte[] Serialize(Object data) {
        try {
          MemoryStream ms = new MemoryStream(1024 * 4); // packet size will be maximum 4k
          BinaryFormatter bf = new BinaryFormatter();
          bf.Serialize(ms, data);
          return ms.ToArray();
        } catch {
          return null;
        }
      }

      public static Object Deserialize(byte[] data) {
        try {
          MemoryStream ms = new MemoryStream(1024 * 4);
          ms.Write(data, 0, data.Length);

          ms.Position = 0;
          BinaryFormatter bf = new BinaryFormatter();
          bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
          Object obj = bf.Deserialize(ms);
          ms.Close();
          return obj;
        } catch {
          return null;
        }
      }
    }
    sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder {
      public override Type BindToType(string assemblyName, string typeName) {
        Type typeToDeserialize = null;
        String currentAssembly = Assembly.GetExecutingAssembly().FullName;
        assemblyName = currentAssembly;
        typeToDeserialize = Type.GetType(string.Format("{0},{1}", typeName, assemblyName));
        return typeToDeserialize;
      }
    }


    [Serializable]
    public class Char_Pos : Packet {
      public int x { get; set; }
      public int y { get; set; }
      public int z { get; set; }
    }

  }
}

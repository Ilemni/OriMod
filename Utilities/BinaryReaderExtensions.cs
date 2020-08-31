using System.IO;
using Microsoft.Xna.Framework;

namespace OriMod.Utilities {
  public static class BinaryReaderExtensions {
    public static Color ReadRGBA(this BinaryReader reader) => new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
  }

  public static class BinaryWriterExtensions {
    public static void WriteRGBA(this BinaryWriter writer, Color c) {
      writer.Write(c.R);
      writer.Write(c.G);
      writer.Write(c.B);
      writer.Write(c.A);
    }
  }
}

using System.IO;
using Microsoft.Xna.Framework;

namespace OriMod.Utilities; 

public static class BinaryReaderExtensions {
  public static Color ReadRgba(this BinaryReader reader) => new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
}

public static class BinaryWriterExtensions {
  public static void WriteRgba(this BinaryWriter writer, Color c) {
    writer.Write(c.R);
    writer.Write(c.G);
    writer.Write(c.B);
    writer.Write(c.A);
  }
}

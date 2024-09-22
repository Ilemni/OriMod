using System.IO;
using Microsoft.Xna.Framework;

namespace OriMod.Utilities;

public static class BinaryReaderExtensions {
  // ReSharper disable once InconsistentNaming - ReadRGBA named to match existing extension ReadRGB
  public static Color ReadRGBA(this BinaryReader reader) =>
    new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
}

public static class BinaryWriterExtensions {
  // ReSharper disable once InconsistentNaming - WriteRGBA named to match existing extension WriteRGB
  public static void WriteRGBA(this BinaryWriter writer, Color c) {
    writer.Write(c.R);
    writer.Write(c.G);
    writer.Write(c.B);
    writer.Write(c.A);
  }
}

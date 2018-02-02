using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEDGame
{
    public static class Extensions
    {
        public static float DeltaTime(this GameTime time)
        {
            return (float)time.ElapsedGameTime.TotalSeconds;
        }

        public static Vector2 ToVector(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static Point ToPointFloor(this Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        public static void WriteColor(BinaryWriter writer, Microsoft.Xna.Framework.Color color)
        {
            writer.Write((byte)color.R);
            writer.Write((byte)color.G);
            writer.Write((byte)color.B);
            writer.Write((byte)color.A);
        }

        public static Microsoft.Xna.Framework.Color ReadColor(BinaryReader reader)
        {
            return new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }
    }
}

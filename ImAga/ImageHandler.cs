using System;
using System.Drawing;
using System.IO;

namespace ImAga
{

    public static class ImageHandler
    {
        public static Bitmap Rotate(string file)
        {
            // Get photo in memory.
            bool loaded = false;
            Image image = null;
            Bitmap bitmap;
            FileStream stream = null;
            do
            {
                try
                {
                    stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    image = Image.FromStream(stream, false, false);
                    loaded = true;

                }
                catch (IOException)
                {
                    //
                }
            } while (!loaded);

            if (Array.IndexOf(image.PropertyIdList, 274) <= -1)
            {
                bitmap = new Bitmap(image);
                stream.Dispose();
                return bitmap;
            }

            var item = image.GetPropertyItem(0x112); //Orientation
            UInt16 uintval = BitConverter.ToUInt16(item.Value, 0);

            switch (uintval)
            {
                case 2:
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
                case 3:
                    image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 4:
                    image.RotateFlip(RotateFlipType.Rotate180FlipX);
                    break;
                case 5:
                    image.RotateFlip(RotateFlipType.Rotate90FlipX);
                    break;
                case 6:
                    image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 7:
                    image.RotateFlip(RotateFlipType.Rotate270FlipX);
                    break;
                case 8:
                    image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }

            image.RemovePropertyItem(0x112);
            bitmap = new Bitmap(image);
            stream.Dispose();
            return bitmap;
        }
    }
}
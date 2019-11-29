using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChatClient.Utility
{
    public class ByteImageConverter
    {
        public static BitmapImage ByteToImage(byte[] imageByteArray)
        {
            using (MemoryStream ms = new MemoryStream(imageByteArray))
            {
                BitmapImage res = new BitmapImage();
                res.BeginInit();
                res.CacheOption = BitmapCacheOption.OnLoad;
                res.StreamSource = ms;
                res.DecodePixelWidth = 1920;
                res.EndInit();
                res.Freeze();
                return res;
            }
        }
    }
}

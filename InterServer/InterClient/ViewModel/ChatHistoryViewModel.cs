using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InterClient.ViewModel
{
    public class ChatHistoryViewModel : ViewModelBase
    {
        private Random rnd = new Random((int)DateTime.Now.Ticks);
        public ChatHistoryViewModel(string user, string data)
        {
            this.User = user;
            if(data.EndsWith("="))
            {
                InitImage(data);
            }
            else
            {
                this.Message = data;
            }
        }

        private async Task InitImage(string data)
        {
            await Task.Delay(200);
            this.FileBlob = Convert.FromBase64String(data);
            var filePath = $".\\tmp\\image{rnd.Next(0, 150000)}.png";
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.CreateNew);
                fs.Write(this.FileBlob, 0, this.FileBlob.Length);
                fs.Flush();
                fs.Close();
            }
            catch
            {

            }
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.UriSource = new Uri(filePath, UriKind.Relative);
            bitmap.DecodePixelWidth = 1920;
            bitmap.EndInit();
            bitmap.Freeze();
            while (bitmap.IsDownloading)
                Thread.Sleep(500);
            this.ImageSource = bitmap;
            this.Message = null;
        }

        public string User { get; set; }
        public string Message { get; set; }

        private BitmapImage imageSource;
        public BitmapImage ImageSource
        {
            get => this.imageSource;
            set => this.Set(ref this.imageSource, value);
        }
            

        public byte[] FileBlob { get; set; }
    }
}

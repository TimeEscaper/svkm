using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.ComponentModel;


namespace new_svkm
{
    class Media
    {
        private static Image PictureBox;
        private static Window MediaWindow;
        private static List<String> photos;
        private static int counter = 0;

        private static void Photo_Window_Init()
        {
            MediaWindow = new Window();
            PictureBox = new Image();

            PictureBox.MouseLeftButtonDown += PictureBox_MouseLeftButtonDown;

            MediaWindow.Content = PictureBox;

            MediaWindow.Show();
            
        }

        private static void PictureBox_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (counter != (photos.Count - 1))
                counter++;
            else
                counter = 0;

            PictureBox.Source = new BitmapImage(new Uri(photos[counter]));
        }

        private static void MediaWindow_Closing(object sender, CancelEventArgs e)
        {
            PictureBox = null;
            MediaWindow = null;
        }

        public static void Show_Photo(List<VkAttachment> sources, string cache_path)
        {
            WebClient downloader = new WebClient();
            Photo_Window_Init();
            photos = new List<String>();

            int i = 0;
          
            foreach(VkAttachment item in sources)
            {

                if (item.type == "photo")
                {
                    string cur_photo = cache_path + "\\" + item.id;
                    if (!File.Exists(cur_photo))
                        downloader.DownloadFile(new Uri(item.source), cur_photo);
                    photos.Add(cur_photo);

                    i++;
                }

            }
            if (i == 0)
                MessageBox.Show("Сообщение не содержит фото!");
            else
            {
                counter = 0;
                PictureBox.Source = new BitmapImage(new Uri(photos[counter]));
            }
        }

        public static void Show_WallRepost(List<VkAttachment> source)
        {

        }
    }
}

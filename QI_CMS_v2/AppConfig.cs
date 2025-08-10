using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace QI_CMS_v2
{
    public static class AppConfig
    {

        public static string FFMPEG_DIR_PATH = "ffmpeg";
        public static string FFMPEG_FILE_PATH = Path.Combine(FFMPEG_DIR_PATH, "ffmpeg.exe");
        public static string FFPROBE_FILE_PATH = Path.Combine(FFMPEG_DIR_PATH, "ffprobe.exe");

        public static SolidColorBrush PrimaryColor = (SolidColorBrush)Application.Current.Resources["ColorPrimary"];
        public static SolidColorBrush SecondaryColor = (SolidColorBrush)Application.Current.Resources["ColorSecondary"];

    }
}

using System;
using System.IO;
using System.Windows;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;

namespace Camera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        static string ip;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //http://192.168.0.102/ AC:CB:51:41:00:3C
            string ipAddress = IPMacMapper.FindIPFromMacAddress(macTextBox.Text); //Получить из ARP таблицы по MAC адресу / ADVANCED IP Scanner
            ip = ipAddress; 
            adressBox.Content = $"Adress: {ipAddress}";
        }
        static DirectoryInfo vlcLibDirectory = new DirectoryInfo("VLC"); // VLC from Project folder

        private void Camera() //Display into frame
        {
            //  this.MyControl.SourceProvider.MediaPlayer.Play($"rtsp://freja.hiof.no:1935/rtplive/_definst_/hessdalen03.stream"); Random camera for test
            var media = $"rtsp://admin:123sakha@{ip}:554/ISAPI/Streaming/Channels/101";

            var mediaOptions = new[]
                {
                    "--file-logging", "-vvv", "--logfile=Logs.log",
                };
            this.MyControl.SourceProvider.CreatePlayer(vlcLibDirectory, mediaOptions);
            this.MyControl.SourceProvider.MediaPlayer.SetMedia(media, mediaOptions);
            this.MyControl.SourceProvider.MediaPlayer.Play();
            Record();
        }
        private void Record() // HeadLess Record
        {
            var media = $"rtsp://admin:123sakha@{ip}:554/ISAPI/Streaming/Channels/101";
            Directory.CreateDirectory("VLCRecord");
            var destination = "VLCRecord/Record.mpg";

            var mediaOptions = new[]
                {
                    ":sout=#file{dst=" + destination + "}",
                    ":sout-keep",
                    "--file-logging", "-vvv", "--logfile=LogsRecord.log",
                };
            this.MyControl2.SourceProvider.CreatePlayer(vlcLibDirectory, mediaOptions);
            this.MyControl2.SourceProvider.MediaPlayer.SetMedia(media, mediaOptions);
            this.MyControl2.SourceProvider.MediaPlayer.Play();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Camera();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            using (var engine = new Engine()) // For get 24 Frames in 1s -> FFMPEG
            {
                var mpg = new MediaFile { Filename = "VLCRecord/Record.mpg" }; // MPG / MP4 / Other

                engine.GetMetadata(mpg);
                int i = int.Parse(timeTextBox.Text);
                Directory.CreateDirectory("images");
                var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(i) };
                var outputFile = new MediaFile { Filename = string.Format("{0}\\image-{1}.jpeg", "images", i) };
                engine.GetThumbnail(mpg, outputFile, options);

            }
        }
    }
}

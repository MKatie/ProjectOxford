using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AForge.Video;
using System.Drawing;
using System.IO;
using System.Windows.Threading;
using System.Drawing.Imaging;
using System.ComponentModel;
using FaceRec;

namespace ProjectOxford
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window, INotifyPropertyChanged
   {
      DispatcherTimer Timer = new DispatcherTimer();

      public MainWindow()
      {
         InitializeComponent();

         ShowCameraImage();
      }

      private void ShowCameraImage()
      {
         var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
         VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

         videoSource.NewFrame += video_NewFrame;
         videoSource.Start();

      }

      private BitmapSource currentImage;

      public event PropertyChangedEventHandler PropertyChanged;

      public BitmapSource CurrentImage
      {
         get { return currentImage; }
         set
         {
            currentImage = value;
            NotifyPropertyChanged("CurrentImage");
         }
      }

      private void NotifyPropertyChanged(string info)
      {
         if (PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs(info));
         }
      }

      private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
      {
         var image = Convert(eventArgs.Frame);
         image.Freeze();
         CurrentImage = image;
      }

      public BitmapSource Convert(Bitmap bitmap)
      {
         var bitmapData = bitmap.LockBits(
             new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
             ImageLockMode.ReadOnly, bitmap.PixelFormat);

         var bitmapSource = BitmapSource.Create(
             bitmapData.Width, bitmapData.Height, 96, 96, PixelFormats.Bgr24, null,
             bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

         bitmap.UnlockBits(bitmapData);
         return bitmapSource;
      }

      private void _snapshot_Click(object sender, RoutedEventArgs e)
      {
         var snap = _cameraImage.Source as BitmapSource;
         
         snap.Freeze();
         Task.Run(() =>
         {
            var faceRec = new FaceRecognition();
            faceRec.SetNotifier(new ControlNotifier(_messages));
            faceRec.CheckPerson("inhabitants", ToByteArray(snap));

            var speechRec = new SpeechToTextRecognition();
            speechRec.SetNotifier(new ControlNotifier(_messages));
            speechRec.StartMicAndRecognition();
         });         
      }

      public Stream ToStream(BitmapSource source)
      {
         JpegBitmapEncoder encoder = new JpegBitmapEncoder();
         encoder.QualityLevel = 15;
         MemoryStream memoryStream = new MemoryStream();

         encoder.Frames.Add(BitmapFrame.Create(source));
         encoder.Save(memoryStream);

         return memoryStream;
      }

      public byte[] ToByteArray(BitmapSource source)
      {
         JpegBitmapEncoder encoder = new JpegBitmapEncoder();
         encoder.QualityLevel = 50;

         byte[] bit = new byte[0];
         using (MemoryStream stream = new MemoryStream())
         {
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(stream);
            bit = stream.ToArray();
            stream.Close();
         }

         return bit;
      }
   }
}

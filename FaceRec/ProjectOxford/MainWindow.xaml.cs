using AForge.Video.DirectShow;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge.Video;
using System.Drawing;
using System.IO;
using System.Windows.Threading;
using System.Drawing.Imaging;
using System.ComponentModel;
using FaceRec;
using NAudio.Wave;
using System.Collections.Generic;

namespace ProjectOxford
{
   public partial class MainWindow : Window, INotifyPropertyChanged
   {
      DispatcherTimer Timer = new DispatcherTimer();
      BitmapSource _snap;

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

      private void _identify_Click(object sender, RoutedEventArgs e)
      {
         var notifier = new ControlNotifier(_messages);
         _snap = _cameraImage.Source as BitmapSource;

         notifier.Notify("Snapshot created");

         var audioRec = new AudioRecorder();
         audioRec.SetNotifier(notifier);
         audioRec.RecordingStopped += AudioRec_RecordingStopped;
         audioRec.Record();

         _snap.Freeze();
         Task.Run(() =>
         {
            var faceRec = new FaceRecognition();
            faceRec.SetNotifier(notifier);
            var recognisedPeople = faceRec.CheckPerson("inhabitants", ToByteArray(_snap));
            SwitchResultTileColor(_faceTile, recognisedPeople.Any());
         });         
      }

      private void AudioRec_RecordingStopped(object sender, AudioRecorderEventArgs e)
      {
         Task.Run(() =>
         {
            var speakerRec = new SpeakerRecognition();
            speakerRec.SetNotifier(new ControlNotifier(_messages));
            var speakerResult = speakerRec.IdentifySpeaker(
               new Guid("309403b9-32f7-4eb8-8b1d-f9a0aead3d29"), e.Bytes);
            SwitchResultTileColor(_speakerTile, speakerResult);

            var speechRec = new SpeechToTextRecognition();
            speechRec.SetNotifier(new ControlNotifier(_messages));
            speechRec.ResultReceived += SpeechRec_ResultReceived;
            speechRec.StartRecognition(e.Bytes);
         });
      }

      private void SpeechRec_ResultReceived(object sender, bool e)
      {
         SwitchResultTileColor(_passwordTile, e);
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

      private void SwitchResultTileColor(Border tile, bool result)
      {
         Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
         {
            var brush = new SolidColorBrush();
            brush.Color = result ? System.Windows.Media.Color.FromRgb(152, 255, 102) : System.Windows.Media.Color.FromRgb(255, 51, 51);
            tile.Background = brush;
            TileChanged(tile, result);
         }));
      }


      private static Dictionary<Border, bool> _summary = new Dictionary<Border, bool>();
      private static readonly object _locker = new object();
      private void TileChanged(Border tile, bool result)
      {
         lock (_locker)
         {
            _summary.Add(tile, result);
         }                 
         if(_summary.Count.Equals(3) && _summary.All(x => x.Value))
         {
            Task.Run(() =>
            {
               var emotionRec = new EmotionRecognition();
               emotionRec.SetNotifier(new ControlNotifier(_messages));
               emotionRec.RecognizeEmotionAndPlayMusic(ToByteArray(_snap));
            });            
         }
      }
   }
}

using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;

using System.IO;
using System.Linq;
using WMPLib;

namespace FaceRec
{
   public class EmotionRecognition
   {   
      private EmotionServiceClient _emotionServiceClient;
      private WindowsMediaPlayer _player;

      private INotifier _notifier;

      public EmotionRecognition()
      {
         _emotionServiceClient = new EmotionServiceClient("3efe0786c0dd4ee3a14d48501f2a83d1");
         _player = new WindowsMediaPlayer();
      }

      public void SetNotifier(INotifier notifier)
      {
         _notifier = notifier;
      }

      public void RecognizeEmotionAndPlayMusic(byte[] imageData)
      {
         Emotion[] emotionResult;
         using (Stream imageFileStream = new MemoryStream(imageData))
         {
            emotionResult = _emotionServiceClient.RecognizeAsync(imageFileStream).Result;

            var recognizedEmotion = emotionResult.First().Scores.ToRankedList().First().Key;
            _notifier?.Notify(string.Format("Recognized emotion: {0}", recognizedEmotion));
            SelectMusic(recognizedEmotion);
         }
      }

      private void SelectMusic(string emotion)
      {
         switch (emotion.ToLower())
         {
            case "anger":
            case "contempt":
            case "disgust":
               PlayMusic("Angry");
               break;

            case "happiness":
            case "surprise":
            case "neutral":
               PlayMusic("Happy");
               break;

            case "sadness":
            case "fear":
               PlayMusic("Sad");
               break;
         }
      }

      private void PlayMusic(string folder)
      {
         string musicFolder = Path.GetFullPath(string.Format(@"..\..\Music\{0}\", folder));         

         _notifier?.Notify(string.Format("Playing music from playlist: {0}", folder));
         foreach (string file in Directory.GetFiles(musicFolder))
         {
            _player.URL = file;
            _player.controls.play();
         }
      }
   }
}

using Microsoft.ProjectOxford.SpeechRecognition;
using System;
using System.Linq;

namespace FaceRec
{
   public class SpeechToTextRecognition
   {
      private INotifier _notifier;

      private MicrophoneRecognitionClient microphoneClient;
      public SpeechToTextRecognition()
      {
         microphoneClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient
         (SpeechRecognitionMode.ShortPhrase, "en-US", "08e1727427c640808a5d242aeec7fd97", "08e1727427c640808a5d242aeec7fd97");

         microphoneClient.OnMicrophoneStatus += OnMicrophoneStatus;
         microphoneClient.OnResponseReceived += OnResponseReceived;
      }

      public void SetNotifier(INotifier notifier)
      {
         _notifier = notifier;
      }


      private void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
      {
         if(e.Recording)
         {
            _notifier?.Notify("Please, start speaking.");         
         }
      }

      private void OnResponseReceived(object sender, SpeechResponseEventArgs e)
      {
         microphoneClient.EndMicAndRecognition();

         string recognizedText = e.PhraseResponse.Results.Select(x => x.LexicalForm).Aggregate((x, y) => x + " " + y);
         string[] baseTexts = new[] { "Hello this is dog", "Houston we have a problem" };

         _notifier?.Notify(recognizedText);

         if(baseTexts.Contains(recognizedText, StringComparer.OrdinalIgnoreCase))
         {
            _notifier?.Notify("Password correct");
         }
         else
         {
            _notifier?.Notify("Error");
         }

      }

      public void StartMicAndRecognition()
      {
         microphoneClient.StartMicAndRecognition();
      }
   }
}

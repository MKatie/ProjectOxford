using Microsoft.ProjectOxford.SpeechRecognition;
using System;
using System.Linq;

namespace FaceRec
{
   public class SpeechToTextRecognition
   {
      private static MicrophoneRecognitionClient microphoneClient;
      static SpeechToTextRecognition()
      {
         microphoneClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient
         (SpeechRecognitionMode.ShortPhrase, "en-US", "08e1727427c640808a5d242aeec7fd97", "08e1727427c640808a5d242aeec7fd97");

         microphoneClient.OnMicrophoneStatus += OnMicrophoneStatus;
         microphoneClient.OnResponseReceived += OnResponseReceived;
      }

      private static void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
      {
         if(e.Recording)
         {
            Console.WriteLine("Please, start speaking.");         
         }
      }

      private static void OnResponseReceived(object sender, SpeechResponseEventArgs e)
      {
         microphoneClient.EndMicAndRecognition();

         string recognizedText = e.PhraseResponse.Results.Select(x => x.LexicalForm).Aggregate((x, y) => x + " " + y);
         string[] baseTexts = new[] { "Hello this is dog", "Houston we have a problem" };

         Console.WriteLine(recognizedText);

         if(baseTexts.Contains(recognizedText, StringComparer.OrdinalIgnoreCase))
         {
            Console.WriteLine("Password correct");
         }
         else
         {
            Console.WriteLine("Error");
         }

      }

      public static void StartMicAndRecognition()
      {
         microphoneClient.StartMicAndRecognition();
      }
   }
}

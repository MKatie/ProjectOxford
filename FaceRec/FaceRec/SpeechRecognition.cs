using Microsoft.ProjectOxford.SpeechRecognition;
using System;
using System.IO;
using System.Linq;

namespace FaceRec
{
   public class SpeechToTextRecognition
   {
      private INotifier _notifier;
      private DataRecognitionClient microphoneClient;

      public event EventHandler<bool> ResultReceived;

      public SpeechToTextRecognition()
      {
         microphoneClient = SpeechRecognitionServiceFactory.CreateDataClient
         (SpeechRecognitionMode.ShortPhrase, "en-US", "08e1727427c640808a5d242aeec7fd97", "08e1727427c640808a5d242aeec7fd97");
         microphoneClient.OnResponseReceived += OnResponseReceived;
      }

      public void SetNotifier(INotifier notifier)
      {
         _notifier = notifier;
      }

      private void OnResponseReceived(object sender, SpeechResponseEventArgs e)
      {
         string recognizedText = string.Join(" ", e.PhraseResponse.Results.Select(x => x.LexicalForm));
         string[] baseTexts = new[] { "Hello this is dog", "Houston we have a problem" };

         _notifier?.Notify(recognizedText);

         var passwordCorrect = false;
         if(baseTexts.Contains(recognizedText, StringComparer.OrdinalIgnoreCase))
         {
            passwordCorrect = true;
            _notifier?.Notify("Password correct");
         }
         else
         {
            _notifier?.Notify("Password incorrect");
         }
         ResultReceived(this, passwordCorrect);
      }

      public void StartRecognition(byte[] bytes)
      {         
         microphoneClient.SendAudioFormat(SpeechAudioFormat.create16BitPCMFormat(16000));

         using (MemoryStream stream = new MemoryStream(bytes))
         {
            int bytesRead = 0;
            byte[] buffer = new byte[1024];

            try
            {
               do
               {
                  bytesRead = stream.Read(buffer, 0, buffer.Length);

                  microphoneClient.SendAudio(buffer, bytesRead);
               }
               while (bytesRead > 0);
            }
            finally
            {
               microphoneClient.EndAudio();
            }
         }
      }
   }
}

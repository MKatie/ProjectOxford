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
         microphoneClient.OnPartialResponseReceived += OnPartialResponseReceived;
         microphoneClient.OnResponseReceived += OnResponseReceived;
         microphoneClient.OnConversationError += OnConversationError;
      }

      private void OnConversationError(object sender, SpeechErrorEventArgs e)
      {
         
      }

      private void OnPartialResponseReceived(object sender, PartialSpeechResponseEventArgs e)
      {
         
      }

      public void SetNotifier(INotifier notifier)
      {
         _notifier = notifier;
      }


      private void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
      {         
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
                  // Get more Audio data to send into byte buffer.
                  bytesRead = stream.Read(buffer, 0, buffer.Length);

                  // Send of audio data to service. 
                  microphoneClient.SendAudio(buffer, bytesRead);
               }
               while (bytesRead > 0);
            }
            finally
            {
               // We are done sending audio.  Final recognition results will arrive in OnResponseReceived event call.
               microphoneClient.EndAudio();
            }
         }
      }
   }
}

using Microsoft.ProjectOxford.SpeakerRecognition;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;


namespace FaceRec
{
   public class SpeakerRecognition
   {
      private SpeakerIdentificationServiceClient _speakerServiceClient;
      private Dictionary<Guid, string> _speakers;
      private INotifier _notifier;

      public SpeakerRecognition()
      {
         _speakerServiceClient = new SpeakerIdentificationServiceClient("b2ffd80aaff54fe39e57de88dd2e9c1c");
         _speakers = new Dictionary<Guid, string>
         {
            { new Guid("309403b9-32f7-4eb8-8b1d-f9a0aead3d29"), "Kasia" }
         };
      }

      public void SetNotifier(INotifier notifier)
      {
         _notifier = notifier;
      }

      public Guid CreateProfile()
      {
         CreateProfileResponse creationResponse = _speakerServiceClient.CreateProfileAsync("en-US").Result;
         return creationResponse.ProfileId;
      }

      public void CreateEnrollment(Guid profileId)
      {
         OperationLocation location;
         using (Stream audioStream = File.OpenRead(@"..\..\Records\KasiaRec.wav"))
         {
            location = _speakerServiceClient.EnrollAsync(audioStream, profileId).Result;
         }

         while (true)
         {
            EnrollmentOperation enrollmentResult = _speakerServiceClient.CheckEnrollmentStatusAsync(location).Result;
            if (enrollmentResult.Status == Status.Succeeded)
            {
               _notifier?.Notify("Enrollment succeded");
               break;
            }
            else if (enrollmentResult.Status == Status.Failed)
            {
               _notifier?.Notify("Enrollment failed");
               break;
            }
            Thread.Sleep(5000);
         }

      }

      public bool IdentifySpeaker(Guid profileId, byte[] bytes)
      {
         OperationLocation processPollingLocation;

         using (MemoryStream audioStream = new MemoryStream(bytes))        
         {            
            WriteWavHeader(audioStream, false, 1, 16, 16000, bytes.Length);
            audioStream.Position = 0;
            processPollingLocation = _speakerServiceClient.IdentifyAsync(audioStream, new[] { profileId }, true).Result;
         }

         IdentificationOperation identyficationResult;

         while (true)
         {
            identyficationResult = _speakerServiceClient.CheckIdentificationStatusAsync(processPollingLocation).Result;
            if (identyficationResult.Status == Status.Succeeded)
            {
               _notifier?.Notify("Identification finished");
               break;
            }
            else if (identyficationResult.Status == Status.Failed)
            {
               _notifier?.Notify("Identification failed");
               break;
            }
            Thread.Sleep(5000);
         }

         bool speakerFound = _speakers.ContainsKey(identyficationResult.ProcessingResult.IdentifiedProfileId);
         if (speakerFound)
            _notifier?.Notify($"Identified speaker: {_speakers[identyficationResult.ProcessingResult.IdentifiedProfileId]}");

         return speakerFound;
      }

      private void WriteWavHeader(MemoryStream stream, bool isFloatingPoint, ushort channelCount, ushort bitDepth, int sampleRate, int totalSampleCount)
      {
         stream.Position = 0;

         stream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);

         stream.Write(BitConverter.GetBytes(totalSampleCount), 0, 4);

         stream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);

         stream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);

         stream.Write(BitConverter.GetBytes(16), 0, 4);

         stream.Write(BitConverter.GetBytes((ushort)(isFloatingPoint ? 3 : 1)), 0, 2);

         stream.Write(BitConverter.GetBytes(channelCount), 0, 2);

         stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);

         stream.Write(BitConverter.GetBytes(sampleRate * channelCount * (bitDepth / 8)), 0, 4);

         stream.Write(BitConverter.GetBytes((ushort)channelCount * (bitDepth / 8)), 0, 2);

         stream.Write(BitConverter.GetBytes(bitDepth), 0, 2);

         stream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);

         stream.Write(BitConverter.GetBytes((bitDepth / 8) * totalSampleCount), 0, 4);
      }

   }
}

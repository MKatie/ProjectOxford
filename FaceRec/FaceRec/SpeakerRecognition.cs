using Microsoft.ProjectOxford.SpeakerRecognition;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FaceRec
{
   public class SpeakerRecognition
   {
      private static SpeakerIdentificationServiceClient speakerServiceClient = new SpeakerIdentificationServiceClient("b2ffd80aaff54fe39e57de88dd2e9c1c");


      //speech API
      //metoda tworzaca profil i zwracajaca jego Id
      public static Guid CreateProfile()
      {
         CreateProfileResponse creationResponse = speakerServiceClient.CreateProfileAsync("en-US").Result;
         return creationResponse.ProfileId;
      }

      //metoda przypisujaca plik dzwiekowy do profilu
      public static void CreateEnrollment(Guid profileId)
      {
         OperationLocation location;
         using (Stream audioStream = File.OpenRead(@"..\..\Records\KasiaRec.wav"))
         {
            //zlecenie utworzenia powiazania pliku dzwiekowego z profilem (enrollment)
            location = speakerServiceClient.EnrollAsync(audioStream, profileId).Result;
         }

         //sprawdzenie czy proces proces tworzenia enrollmentu sie zakonczyl
         while (true)
         {
            //wywolanie metody sprawdzajacej stan procesu
            EnrollmentOperation enrollmentResult = speakerServiceClient.CheckEnrollmentStatusAsync(location).Result;
            if (enrollmentResult.Status == Status.Succeeded)
            {
               Console.WriteLine("Enrollment succeded");
               break;
            }
            else if (enrollmentResult.Status == Status.Failed)
            {
               Console.WriteLine("Enrollment failed");
               break;
            }
            Thread.Sleep(5000);
         }

      }

      public static bool IdentifySpeaker(Guid profileId)
      {
         OperationLocation processPollingLocation;

         using (Stream audioStream = File.OpenRead(@"..\..\Records\KasiaShortTest.wav"))
         {
            processPollingLocation = speakerServiceClient.IdentifyAsync(audioStream, new[] { profileId }, true).Result;
         }

         IdentificationOperation identyficationResult;

         while (true)
         {
            //wywolanie metody sprawdzajacej stan procesu
            identyficationResult = speakerServiceClient.CheckIdentificationStatusAsync(processPollingLocation).Result;
            if (identyficationResult.Status == Status.Succeeded)
            {
               Console.WriteLine("Identyfication succeded");
               break;
            }
            else if (identyficationResult.Status == Status.Failed)
            {
               Console.WriteLine("Identyfication failed");
               break;
            }
            Thread.Sleep(5000);
         }

         return identyficationResult.ProcessingResult.IdentifiedProfileId == profileId;
      }

   }
}

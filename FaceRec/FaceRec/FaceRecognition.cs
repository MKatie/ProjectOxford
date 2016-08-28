using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRec
{
   public class FaceRecognition
   {
      static IFaceServiceClient faceServiceClient = new FaceServiceClient("e8be260d45f840808f6c8999c9fd8881");

      static void DetectTest()
      {
         using (Stream imageFileStream = File.OpenRead(@"..\..\Images\Sherlock\sherlok1.jpg"))
         {
            var faces = faceServiceClient.DetectAsync(imageFileStream, true, true).Result;
         }
      }

      static void PersonGroupTest()
      {
         string personGroupId = "inhabitants";
         //CreatePersonGroup(personGroupId);

         //testowanie zdjecia 
         string testImage = @"..\..\Images\Test\DobrySherlockiWatson.jpg";

         //otwarcie pliku i detekcja twarzy
         using (Stream s = File.OpenRead(testImage))
         {
            Face[] faces = faceServiceClient.DetectAsync(s).Result;

            //przypisanie id twarzy do tablicy face
            Guid[] faceIds = faces.Select(face => face.FaceId).ToArray();

            //Identyfikacja Id twarzy w bazie person grupy
            IdentifyResult[] results = faceServiceClient.IdentifyAsync(personGroupId, faceIds).Result;  //typ z microsoft proj oxford
            foreach (IdentifyResult identifyResult in results)
            {
               Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
               if (identifyResult.Candidates.Length == 0)
               {
                  Console.WriteLine("No one identified");
               }
               else
               {
                  var candidateId = identifyResult.Candidates[0].PersonId;
                  var person = faceServiceClient.GetPersonAsync(personGroupId, candidateId).Result;
                  Console.WriteLine("Identified as {0}", person.Name);
               }
            }
         }
      }

      //tworzenie i trenowanie person grupy pokemonow
      static void CreatePersonGroup(string personGroupId)
      {
         faceServiceClient.CreatePersonGroupAsync(personGroupId, "Inhabitants").Wait();

         //zdefiniowanie osoby Sherlocka
         CreatePersonResult inhabitant1 = faceServiceClient.CreatePersonAsync(personGroupId, "Sherlock").Result;

         //przypisanie sciezki do zmiennej i wyszukanie zdjec
         const string imageFolder = @"..\..\Images\Sherlock\";

         foreach (string image in Directory.GetFiles(imageFolder))
         {
            using (Stream s = File.OpenRead(image))
            {
               //wykrywanie twarzy i dodawanie zdjec do person grupy Sherlock
               faceServiceClient.AddPersonFaceAsync(personGroupId, inhabitant1.PersonId, s).Wait();
            }
         }

         //trenowanie grupy
         faceServiceClient.TrainPersonGroupAsync(personGroupId).Wait();

         //sprawdzanie statusu trenowania grupy 
         TrainingStatus trainingStatus = null;
         while (true)
         {
            trainingStatus = faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId).Result;

            if (trainingStatus.Status != Status.Running)
            {
               break;
            }

            Task.Delay(1000).Wait();
         }
      }
   }
}

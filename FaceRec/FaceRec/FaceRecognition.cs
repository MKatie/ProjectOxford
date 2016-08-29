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

      static FaceRecognition()
      {
         string personGroupId = "inhabitants";
         CreatePersonGroup(personGroupId);
         CreatePersons(personGroupId);
      }

      static void DetectTest()
      {
         using (Stream imageFileStream = File.OpenRead(@"..\..\Images\Sherlock\sherlok1.jpg"))
         {
            var faces = faceServiceClient.DetectAsync(imageFileStream, true, true).Result;
         }
      }

      public static List<Person> CheckPerson(string personGroupId, string imagePath)
      {
         List<Person> detectedPersons = new List<Person>();

         //otwarcie pliku i detekcja twarzy
         using (Stream s = File.OpenRead(imagePath))
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
                  detectedPersons.Add(person);
                  Console.WriteLine("Identified as {0}", person.Name);
               }
            }
         }
         return detectedPersons;
      }

      //tworzenie i trenowanie person grupy pokemonow
      static void CreatePersonGroup(string personGroupId)
      {
         //sprawdzanie czy grupa juz istnieje 
         if(faceServiceClient.GetPersonGroupAsync(personGroupId).Result != null)
         {
            return;
         }

         faceServiceClient.CreatePersonGroupAsync(personGroupId, "Inhabitants").Wait();
      }

      static void CreatePerson(string personGroupId, string personName)
      {
         //sprawdzenie czy dana osoba jest juz w grupie
         if(faceServiceClient.GetPersonsAsync(personGroupId).Result.Select(x => x.Name).Contains(personName))
         {
            return;
         }

         //utworzenie osoby
         CreatePersonResult createdPerson = faceServiceClient.CreatePersonAsync(personGroupId, personName).Result;

         //przypisanie sciezki do zmiennej i wyszukanie zdjec
         string imageFolder = string.Format(@"..\..\Inhabitants\{0}\", personName);

         foreach (string image in Directory.GetFiles(imageFolder))
         {
            using (Stream imageStream = File.OpenRead(image))
            {
               //wykrywanie twarzy i dodawanie zdjec do person grupy Sherlock
               faceServiceClient.AddPersonFaceAsync(personGroupId, createdPerson.PersonId, imageStream).Wait();
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

      //tworzenie osob z folderu inhabitants, na podstwie nazw folderow
      static void CreatePersons(string personGroupId)
      {
         foreach (string personName in Directory.GetDirectories(@"..\..\Inhabitants"))
         {
            CreatePerson(personGroupId, new DirectoryInfo(personName).Name);
         }
      }
   }
}

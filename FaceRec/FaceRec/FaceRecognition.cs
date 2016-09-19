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
      private IFaceServiceClient _faceServiceClient;

      private INotifier _notifier;

      public FaceRecognition()
      {
         _faceServiceClient = new FaceServiceClient("e8be260d45f840808f6c8999c9fd8881");
         string personGroupId = "inhabitants";
         CreatePersonGroup(personGroupId);
         CreatePersons(personGroupId);
      }

      public void SetNotifier(INotifier notifier)
      {
         _notifier = notifier;
      }

      public List<Person> CheckPerson(string personGroupId, string imagePath)
      {
         //otwarcie pliku i detekcja twarzy
         using (Stream imageStream = File.OpenRead(imagePath))
         {
            return CheckPerson(personGroupId, imageStream);
         }
      }

      public List<Person> CheckPerson(string personGroupId, byte[] bytes)
      {
         using(Stream stream = new MemoryStream())
         {
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;
            return CheckPerson(personGroupId, stream);
         }
      }

      public List<Person> CheckPerson(string personGroupId, Stream imageStream)
      {
         List<Person> detectedPersons = new List<Person>();
         try
         {            
            Face[] faces = _faceServiceClient.DetectAsync(imageStream).Result;

            //przypisanie id twarzy do tablicy face
            Guid[] faceIds = faces.Select(face => face.FaceId).ToArray();

            //Identyfikacja Id twarzy w bazie person grupy
            IdentifyResult[] results = _faceServiceClient.IdentifyAsync(personGroupId, faceIds).Result;  //typ z microsoft proj oxford
            foreach (IdentifyResult identifyResult in results)
            {
               _notifier?.Notify(string.Format("Result of face: {0}", identifyResult.FaceId));
               //_notifier?.Notify($"Result of face: {identifyResult.FaceId}");

               if (identifyResult.Candidates.Length == 0)
               {
                  _notifier?.Notify("No one identified");
               }
               else
               {
                  var candidateId = identifyResult.Candidates[0].PersonId;
                  var person = _faceServiceClient.GetPersonAsync(personGroupId, candidateId).Result;
                  detectedPersons.Add(person);
                  _notifier?.Notify(string.Format("Identified as {0}", person.Name));
               }
            }
         }
         catch(Exception)
         {
            _notifier?.Notify("Face not found.");
         }

         return detectedPersons;
      }


      //tworzenie i trenowanie person grupy pokemonow
      private void CreatePersonGroup(string personGroupId)
      {
         //sprawdzanie czy grupa juz istnieje 
         if (_faceServiceClient.GetPersonGroupAsync(personGroupId).Result != null)
         {
            return;
         }

         _faceServiceClient.CreatePersonGroupAsync(personGroupId, "Inhabitants").Wait();
      }

      private void CreatePerson(string personGroupId, string personName)
      {
         //sprawdzenie czy dana osoba jest juz w grupie
         if (_faceServiceClient.GetPersonsAsync(personGroupId).Result.Select(x => x.Name).Contains(personName))
         {
            return;
         }

         //utworzenie osoby
         CreatePersonResult createdPerson = _faceServiceClient.CreatePersonAsync(personGroupId, personName).Result;

         //przypisanie sciezki do zmiennej i wyszukanie zdjec
         string imageFolder = string.Format(@"..\..\Inhabitants\{0}\", personName);

         foreach (string image in Directory.GetFiles(imageFolder))
         {
            using (Stream imageStream = File.OpenRead(image))
            {
               //wykrywanie twarzy i dodawanie zdjec do person grupy Sherlock
               _faceServiceClient.AddPersonFaceAsync(personGroupId, createdPerson.PersonId, imageStream).Wait();
            }
         }

         //trenowanie grupy
         _faceServiceClient.TrainPersonGroupAsync(personGroupId).Wait();

         //sprawdzanie statusu trenowania grupy 
         TrainingStatus trainingStatus = null;
         while (true)
         {
            trainingStatus = _faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId).Result;

            if (trainingStatus.Status != Status.Running)
            {
               break;
            }

            Task.Delay(1000).Wait();
         }
      }

      //tworzenie osob z folderu inhabitants, na podstwie nazw folderow
      private void CreatePersons(string personGroupId)
      {
         foreach (string personName in Directory.GetDirectories(@"..\..\Inhabitants"))
         {
            CreatePerson(personGroupId, new DirectoryInfo(personName).Name);
         }
      }
   }
}

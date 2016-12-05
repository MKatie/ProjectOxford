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
      
      //CheckPerson dla aplikacji konsolowej
      public List<Person> CheckPerson(string personGroupId, string imagePath)
      {
         using (Stream imageStream = File.OpenRead(imagePath))
         {
            return CheckPerson(personGroupId, imageStream);
         }
      }

      //CheckPerson dla aplikacji desktopowej 
      public List<Person> CheckPerson(string personGroupId, byte[] bytes)
      {
         using(MemoryStream stream = new MemoryStream())
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

            Guid[] faceIds = faces.Select(face => face.FaceId).ToArray();

            IdentifyResult[] results = _faceServiceClient.IdentifyAsync(personGroupId, faceIds).Result;  //typ z microsoft proj oxford
            foreach (IdentifyResult identifyResult in results)
            {
               _notifier?.Notify(string.Format("Result of face: {0}", identifyResult.FaceId));

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

      private void CreatePersonGroup(string personGroupId)
      {
         if (_faceServiceClient.GetPersonGroupAsync(personGroupId).Result != null)
         {
            return;
         }

         _faceServiceClient.CreatePersonGroupAsync(personGroupId, "Inhabitants").Wait();
      }

      private void CreatePerson(string personGroupId, string personName)
      {
         if (_faceServiceClient.GetPersonsAsync(personGroupId).Result.Select(x => x.Name).Contains(personName))
         {
            return;
         }

         CreatePersonResult createdPerson = _faceServiceClient.CreatePersonAsync(personGroupId, personName).Result;

         string imageFolder = string.Format(@"..\..\Inhabitants\{0}\", personName);

         foreach (string image in Directory.GetFiles(imageFolder))
         {
            using (Stream imageStream = File.OpenRead(image))
            {
               _faceServiceClient.AddPersonFaceAsync(personGroupId, createdPerson.PersonId, imageStream).Wait();
            }
         }

         _faceServiceClient.TrainPersonGroupAsync(personGroupId).Wait();

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

      private void CreatePersons(string personGroupId)
      {
         foreach (string personName in Directory.GetDirectories(@"..\..\Inhabitants"))
         {
            CreatePerson(personGroupId, new DirectoryInfo(personName).Name);
         }
      }
   }
}

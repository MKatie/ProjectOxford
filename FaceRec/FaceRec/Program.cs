using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WMPLib;

namespace FaceRec
{
   class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine("To start identyfication press any key");
         Console.ReadLine();

         //face recognition

         string testImagePath = @"..\..\Test\TestWatson2.jpg";

         var faceRec = new FaceRecognition();
         faceRec.CheckPerson("inhabitants", testImagePath);

         //speaker recognition
         Guid profileId;  
         //profileId = SpeakerRecognition.CreateProfile();     //wykonywaa raz, na poczatku
         profileId = new Guid("309403b9-32f7-4eb8-8b1d-f9a0aead3d29"); 
         //SpeakerRecognition.CreateEnrollment(profileId);

         //Console.WriteLine(string.Format("Identyfication result: {0}", SpeakerRecognition.IdentifySpeaker(profileId)));

         //speech recognition
        // SpeechToTextRecognition.StartMicAndRecognition();

      /*  if (detectedPersons.Count != 0)
         {
            EmotionRecognition.EmotionTest(testImagePath);
         }
       */ 
       //  Console.WriteLine("Identyfication ended");
         Console.ReadKey();
      }

      /*
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
      */


      /*
      
      //emotion API 
      static void EmotionTest()
      {
         Emotion[] emotionResult;
         using (Stream imageFileStream = File.OpenRead(@"..\..\Images\Sherlock\sherlock4.png"))
         {
            emotionResult = emotionServiceClient.RecognizeAsync(imageFileStream).Result;
            foreach (Emotion f in emotionResult)
            {
               foreach (KeyValuePair<string, float> score in f.Scores.ToRankedList())
               {
                  Console.WriteLine(string.Format("{0}: {1}", score.Key, score.Value));
               }
            }

            //pobranie pierwszej twarzy i emocji najwyzszej ranga
            SelectMusic(emotionResult.First().Scores.ToRankedList().First().Key);
         }
      }

      static void SelectMusic(string emotion)
      {
         switch (emotion.ToLower())
         {
            case "anger":
            case "contempt":
            case "disgust":
               PlayMusic("Angry");
               break;

            case "happiness":
            case "surprise":
            case "neutral":
               PlayMusic("Happy");
               break;

            case "sadness":
            case "fear":
               PlayMusic("Sad");
               break;
         }
      }

      //odtwarzanie muzyki z folderow
      static void PlayMusic(string folder)
      {
         //pobieranie pelnej sciezki do folderu ze sciezki wzglednej
         string musicFolder = Path.GetFullPath(string.Format(@"..\..\Music\{0}\", folder));

         WindowsMediaPlayer myplayer = new WindowsMediaPlayer();

         //pobranie i odtworzenie wszystkich plikow z folderu
         foreach (string file in Directory.GetFiles(musicFolder))
         {
            myplayer.URL = file;
            myplayer.controls.play();
         }
      }

   */
   }
}

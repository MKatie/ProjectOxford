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

         //string testImagePath = @"..\..\Test\TestWatson2.jpg";

         //var faceRec = new FaceRecognition();
         //faceRec.CheckPerson("inhabitants", testImagePath);

         //speaker recognition
         Guid profileId;  
         //profileId = SpeakerRecognition.CreateProfile();     //wykonywaa raz, na poczatku
         profileId = new Guid("309403b9-32f7-4eb8-8b1d-f9a0aead3d29");
         var speaker = new SpeakerRecognition();
         speaker.CreateEnrollment(profileId);

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
   }
}

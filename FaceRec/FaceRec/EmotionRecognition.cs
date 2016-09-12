using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMPLib;

namespace FaceRec
{
   public class EmotionRecognition
   {

      static EmotionServiceClient emotionServiceClient = new EmotionServiceClient("3efe0786c0dd4ee3a14d48501f2a83d1");

      //emotion API 
      public static void EmotionTest(string imagePath)
      {
         Emotion[] emotionResult;
         using (Stream imageFileStream = File.OpenRead(imagePath))
         {
            emotionResult = emotionServiceClient.RecognizeAsync(imageFileStream).Result;
            foreach (Emotion f in emotionResult)
            {
               foreach (KeyValuePair<string, float> score in f.Scores.ToRankedList())
               {
                  Console.WriteLine(string.Format("{0}: {1}", score.Key, score.Value));
               }
               Console.WriteLine();
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
   }
}

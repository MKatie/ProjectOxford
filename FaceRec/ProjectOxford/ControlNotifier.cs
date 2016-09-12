using FaceRec;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProjectOxford
{
   public class ControlNotifier : INotifier
   {
      private TextBlock _control;

      public ControlNotifier(TextBlock control)
      {
         _control = control;
      }

      public void Notify(string message)
      {
         Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
         {
            _control.Text += string.Format("{0}\n", message);
         }));         
      }
   }
}

using FaceRec;
using NAudio.Wave;
using System;
using System.IO;


namespace ProjectOxford
{
   public class AudioRecorder
   {
      private WaveIn _recorder;
      private WaveFormat _recordingFormat;
      private WaveFileWriter _writer;
      private MemoryStream _buffer;
      private byte[] _recordBytes;
      private bool _recordingStopped;

      public event EventHandler<AudioRecorderEventArgs> RecordingStopped;

      private INotifier _notifier;


      public AudioRecorder()
      {
         _recordingFormat = new WaveFormat(16000, 1);

         _recorder = new WaveIn();
         _recorder.DeviceNumber = 0;
         _recorder.DataAvailable += RecorderOnDataAvailable;
         _recorder.RecordingStopped += RecorderOnRecordingStopped;
         _recorder.WaveFormat = _recordingFormat;
      }

      public void SetNotifier(INotifier notifier)
      {
         _notifier = notifier;
      }


      public byte[] Record()
      {
         _buffer = new MemoryStream();
         _writer = new WaveFileWriter(_buffer, _recorder.WaveFormat);

         _notifier?.Notify("Please start speaking...");
         _recorder.StartRecording();

         return _recordBytes;
      }

      private void RecorderOnDataAvailable(object sender, WaveInEventArgs e)
      {
         long maxFileLength = _recordingFormat.AverageBytesPerSecond * 8;
         var bytesRecorded = e.BytesRecorded;

         var toWrite = (int)Math.Min(maxFileLength - _buffer.Length, bytesRecorded);
         if (toWrite > 0)
         {
            _buffer.Write(e.Buffer, 0, bytesRecorded);
         }
         else if(_recordingStopped == false)
         {
            Stop();
         }         
      }

      private void Stop()
      {
         _recordingStopped = true;
         _recorder.StopRecording();
      }

      private void RecorderOnRecordingStopped(object sender, StoppedEventArgs e)
      {
         _recordBytes = _buffer.ToArray();
         _buffer.Dispose();
         _recordingStopped = true;
         RecordingStopped(this, new AudioRecorderEventArgs(_recordBytes));
         _notifier?.Notify("Recording stopped");
      }
   }

   public class AudioRecorderEventArgs : EventArgs
   {
      public AudioRecorderEventArgs(byte[] bytes)
      {
         Bytes = bytes;
      }

      public byte[] Bytes { get; set;}
   }
}

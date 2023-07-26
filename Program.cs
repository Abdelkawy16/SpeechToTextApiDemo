using Google.Cloud.Speech.V1;
using NAudio.Wave;
using System;

namespace SpeechToTextApiDemo
{
    public class Program
    {
        static WaveFileWriter waveFileWriter;
        static void Record(string outputPath)
        {
            int sampleRate = 44100;
            int channels = 1;
            //int bitsPerSample = 16;
            int millisecondsToRecord = 3000;

            // Create a WaveInEvent instance to capture audio from the default microphone
            WaveInEvent waveIn = new WaveInEvent();
            waveIn.WaveFormat = new WaveFormat(sampleRate, channels);
            //waveIn.BitsPerSample = bitsPerSample;
            waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(waveIn_DataAvailable);

            // Create a WaveFileWriter instance to write the recorded audio to a WAV file
            waveFileWriter = new WaveFileWriter(outputPath, waveIn.WaveFormat);

            // Start recording
            Console.WriteLine("Recording...");
            waveIn.StartRecording();

            // Wait for the specified number of milliseconds
            System.Threading.Thread.Sleep(millisecondsToRecord);

            // Stop recording and close the WaveFileWriter
            waveIn.StopRecording();
            waveFileWriter.Close();

            Console.WriteLine("Finished recording.");
        }

        private static void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFileWriter != null)
                waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
        }
        public static void Main(string[] args)
        {
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", (Directory.GetCurrentDirectory()) + "\\key.json");
            var speech = SpeechClient.Create();
            var config = new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 44100,
                LanguageCode = LanguageCodes.English.UnitedStates,
                EnableWordTimeOffsets = true
            };
            var audioPath = (Directory.GetCurrentDirectory()) + "\\audio.flac";
            Record(audioPath);
            var audio = RecognitionAudio.FromFile(audioPath);

            var response = speech.Recognize(config, audio);

            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    Console.WriteLine(alternative.Transcript);
                }
            }
        }
    }
}
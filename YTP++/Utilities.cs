using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace YTPDeluxe
{
    public static class Utilities
    {
        public static readonly string[] VideoExtensions = new string[] { ".mp4", ".avi", ".mov", ".mkv", ".wmv" };
        public static readonly string[] AudioExtensions = new string[] { ".mp3", ".wav", ".aac", ".m4a", ".ogg" };

        public static string GetConfiguredFFmpegPath()
        {
            string path = ConfigurationManager.AppSettings["FFmpegPath"];
            if (String.IsNullOrEmpty(path))
                path = "ffmpeg.exe";
            return path;
        }

        public static string GetConfiguredImageMagickPath()
        {
            string path = ConfigurationManager.AppSettings["ImageMagickPath"];
            if (String.IsNullOrEmpty(path))
                path = "magick.exe";
            return path;
        }

        public static bool IsVideoFile(string path)
        {
            return HasExtension(path, VideoExtensions);
        }

        public static bool IsAudioFile(string path)
        {
            return HasExtension(path, AudioExtensions);
        }

        public static IList<string> GetMediaFiles(string folder, bool includeVideo, bool includeAudio)
        {
            List<string> files = new List<string>();
            if (String.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                return files;

            string[] discovered = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
            for (int i = 0; i < discovered.Length; i++)
            {
                bool ok = (includeVideo && IsVideoFile(discovered[i])) || (includeAudio && IsAudioFile(discovered[i]));
                if (ok)
                    files.Add(discovered[i]);
            }

            return files;
        }

        public static string Quote(string value)
        {
            if (value == null)
                value = String.Empty;
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        public static string EscapeForConcatFile(string value)
        {
            return value.Replace("\\", "/").Replace("'", "'\\''");
        }

        public static string Invariant(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        public static string ClampAtempo(double speed)
        {
            if (speed < 0.5)
                speed = 0.5;
            if (speed > 2.0)
                speed = 2.0;
            return Invariant(speed);
        }

        public static T PickRandom<T>(IList<T> list, Random random)
        {
            if (list == null || list.Count == 0)
                return default(T);
            return list[random.Next(list.Count)];
        }

        public static void Shuffle<T>(IList<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        public static string CreateTempDirectory()
        {
            string folder = Path.Combine(Path.GetTempPath(), "YTPDeluxe_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            return folder;
        }

        public static int RunProcess(string fileName, string arguments, string workingDirectory, DataReceivedEventHandler outputHandler)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = fileName;
            startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = String.IsNullOrEmpty(workingDirectory) ? Environment.CurrentDirectory : workingDirectory;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                if (outputHandler != null)
                {
                    process.OutputDataReceived += outputHandler;
                    process.ErrorDataReceived += outputHandler;
                }

                process.Start();
                if (outputHandler != null)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }

                process.WaitForExit();
                return process.ExitCode;
            }
        }

        public static void WriteConcatFile(string concatFilePath, IList<string> segmentPaths)
        {
            using (StreamWriter writer = new StreamWriter(concatFilePath, false, Encoding.UTF8))
            {
                for (int i = 0; i < segmentPaths.Count; i++)
                    writer.WriteLine("file '" + EscapeForConcatFile(segmentPaths[i]) + "'");
            }
        }

        public static void SavePreset(string path, GeneratorConfig config)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GeneratorConfig));
            using (FileStream stream = File.Create(path))
                serializer.Serialize(stream, config);
        }

        public static GeneratorConfig LoadPreset(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GeneratorConfig));
            using (FileStream stream = File.OpenRead(path))
                return (GeneratorConfig)serializer.Deserialize(stream);
        }

        public static string SelectFolder(IWin32Window owner, string description)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = description;
                dialog.ShowNewFolderButton = true;
                return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.SelectedPath : null;
            }
        }

        private static bool HasExtension(string path, string[] extensions)
        {
            string extension = Path.GetExtension(path);
            for (int i = 0; i < extensions.Length; i++)
            {
                if (String.Equals(extension, extensions[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}

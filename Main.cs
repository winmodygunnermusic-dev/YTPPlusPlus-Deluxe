using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace YTPDeluxe
{
    public partial class Main : Form
    {
        private GeneratorConfig config;
        private BackgroundWorker renderWorker;

        public Main()
        {
            InitializeComponent();
            config = GeneratorConfig.CreateDeluxeCompletePreset();
            renderWorker = new BackgroundWorker();
            renderWorker.WorkerReportsProgress = true;
            renderWorker.WorkerSupportsCancellation = true;
            renderWorker.DoWork += RenderWorkerDoWork;
            renderWorker.ProgressChanged += renderWorker_ProgressChanged;
            renderWorker.RunWorkerCompleted += RenderWorkerCompleted;
            LoadConfigIntoUi();
        }

        private void LoadConfigIntoUi()
        {
            txtFfmpegPath.Text = config.Settings.FFmpegPath;
            txtOutputPath.Text = config.Settings.OutputPath;
            nudClipCount.Value = Clamp(config.Settings.ClipCount, (int)nudClipCount.Minimum, (int)nudClipCount.Maximum);
            nudMinSeconds.Value = Clamp(config.Settings.MinClipSeconds, (int)nudMinSeconds.Minimum, (int)nudMinSeconds.Maximum);
            nudMaxSeconds.Value = Clamp(config.Settings.MaxClipSeconds, (int)nudMaxSeconds.Minimum, (int)nudMaxSeconds.Maximum);
            trkStackLevel.Value = Clamp(config.Settings.StackLevel, trkStackLevel.Minimum, trkStackLevel.Maximum);
            lblStackLevel.Text = trkStackLevel.Value.ToString();
            chkChaosTimeline.Checked = config.Settings.ChaosTimeline;
            chkAutoSourceSwitcher.Checked = config.Settings.AutoSourceSwitcher;
            chkCleanupTemp.Checked = config.Settings.CleanupTempFiles;
            RefreshLibraryCounts();
            RefreshEffectsList();
        }

        private void SaveUiIntoConfig()
        {
            config.Settings.FFmpegPath = txtFfmpegPath.Text.Trim();
            config.Settings.OutputPath = txtOutputPath.Text.Trim();
            config.Settings.ClipCount = (int)nudClipCount.Value;
            config.Settings.MinClipSeconds = (int)nudMinSeconds.Value;
            config.Settings.MaxClipSeconds = (int)nudMaxSeconds.Value;
            config.Settings.StackLevel = trkStackLevel.Value;
            config.Settings.ChaosTimeline = chkChaosTimeline.Checked;
            config.Settings.AutoSourceSwitcher = chkAutoSourceSwitcher.Checked;
            config.Settings.CleanupTempFiles = chkCleanupTemp.Checked;
        }

        private void RefreshLibraryCounts()
        {
            lstLibraries.Items.Clear();
            AddLibraryCount("Materials", config.Library.Materials.Count);
            AddLibraryCount("Transitions", config.Library.Transitions.Count);
            AddLibraryCount("Intros", config.Library.Intros.Count);
            AddLibraryCount("Outros", config.Library.Outros.Count);
            AddLibraryCount("Overlays", config.Library.Overlays.Count);
            AddLibraryCount("Sound FX", config.Library.SoundFx.Count);
            AddLibraryCount("Music", config.Library.Music.Count);
            AddLibraryCount("Rave", config.Library.Rave.Count);
            AddLibraryCount("Vocoder", config.Library.Vocoder.Count);
        }

        private void AddLibraryCount(string name, int count)
        {
            lstLibraries.Items.Add(name + ": " + count + " files");
        }

        private void RefreshEffectsList()
        {
            clbEffects.Items.Clear();
            for (int i = 0; i < config.Effects.Count; i++)
                clbEffects.Items.Add(config.Effects[i].Name, config.Effects[i].Enabled);

            if (clbEffects.Items.Count > 0)
                clbEffects.SelectedIndex = 0;
        }

        private EffectSetting SelectedEffectSetting()
        {
            if (clbEffects.SelectedIndex < 0)
                return null;
            string name = clbEffects.Items[clbEffects.SelectedIndex].ToString();
            return FindEffectSetting(name);
        }

        private EffectSetting FindEffectSetting(string name)
        {
            for (int i = 0; i < config.Effects.Count; i++)
            {
                if (String.Equals(config.Effects[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return config.Effects[i];
            }

            return null;
        }

        private void ImportVideoLibrary(List<string> target, string title)
        {
            string folder = Utilities.SelectFolder(this, "Import " + title + " folder");
            if (folder == null)
                return;

            IList<string> files = Utilities.GetMediaFiles(folder, true, false);
            AddUnique(target, files);
            Log("Imported " + files.Count + " video files into " + title + ".");
            RefreshLibraryCounts();
        }

        private void ImportAudioLibrary(List<string> target, string title)
        {
            string folder = Utilities.SelectFolder(this, "Import " + title + " folder");
            if (folder == null)
                return;

            IList<string> files = Utilities.GetMediaFiles(folder, false, true);
            AddUnique(target, files);
            Log("Imported " + files.Count + " audio files into " + title + ".");
            RefreshLibraryCounts();
        }

        private void AddUnique(List<string> target, IList<string> files)
        {
            for (int i = 0; i < files.Count; i++)
            {
                if (!target.Contains(files[i]))
                    target.Add(files[i]);
            }
        }

        private void Log(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action<string>(Log), message);
                return;
            }

            txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + message + Environment.NewLine);
        }

        private void btnImportMaterials_Click(object sender, EventArgs e) { ImportVideoLibrary(config.Library.Materials, "Materials"); }
        private void btnImportTransitions_Click(object sender, EventArgs e) { ImportVideoLibrary(config.Library.Transitions, "Transitions"); }
        private void btnImportIntros_Click(object sender, EventArgs e) { ImportVideoLibrary(config.Library.Intros, "Intros"); }
        private void btnImportOutros_Click(object sender, EventArgs e) { ImportVideoLibrary(config.Library.Outros, "Outros"); }
        private void btnImportOverlays_Click(object sender, EventArgs e) { ImportVideoLibrary(config.Library.Overlays, "Overlays"); }
        private void btnImportSoundFx_Click(object sender, EventArgs e) { ImportAudioLibrary(config.Library.SoundFx, "Sound FX"); }
        private void btnImportMusic_Click(object sender, EventArgs e) { ImportAudioLibrary(config.Library.Music, "Music"); }
        private void btnImportRave_Click(object sender, EventArgs e) { ImportAudioLibrary(config.Library.Rave, "Rave"); }
        private void btnImportVocoder_Click(object sender, EventArgs e) { ImportAudioLibrary(config.Library.Vocoder, "Vocoder"); }

        private void clbEffects_SelectedIndexChanged(object sender, EventArgs e)
        {
            EffectSetting setting = SelectedEffectSetting();
            if (setting == null)
                return;

            trkProbability.Value = Clamp(setting.Probability, trkProbability.Minimum, trkProbability.Maximum);
            trkIntensity.Value = Clamp(setting.Intensity, trkIntensity.Minimum, trkIntensity.Maximum);
            lblProbability.Text = trkProbability.Value + "%";
            lblIntensity.Text = trkIntensity.Value + "%";
        }

        private void clbEffects_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string name = clbEffects.Items[e.Index].ToString();
            EffectSetting setting = FindEffectSetting(name);
            if (setting != null)
                setting.Enabled = e.NewValue == CheckState.Checked;
        }

        private void trkProbability_Scroll(object sender, EventArgs e)
        {
            EffectSetting setting = SelectedEffectSetting();
            if (setting != null)
                setting.Probability = trkProbability.Value;
            lblProbability.Text = trkProbability.Value + "%";
        }

        private void trkIntensity_Scroll(object sender, EventArgs e)
        {
            EffectSetting setting = SelectedEffectSetting();
            if (setting != null)
                setting.Intensity = trkIntensity.Value;
            lblIntensity.Text = trkIntensity.Value + "%";
        }

        private void trkStackLevel_Scroll(object sender, EventArgs e)
        {
            lblStackLevel.Text = trkStackLevel.Value.ToString();
        }

        private void btnSelectAllEffects_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbEffects.Items.Count; i++)
            {
                clbEffects.SetItemChecked(i, true);
                config.Effects[i].Enabled = true;
            }
        }

        private void btnClearEffects_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbEffects.Items.Count; i++)
            {
                clbEffects.SetItemChecked(i, false);
                config.Effects[i].Enabled = false;
            }
        }

        private void btnResetDefaults_Click(object sender, EventArgs e)
        {
            MediaLibrary keepLibrary = config.Library;
            config = GeneratorConfig.CreateDeluxeCompletePreset();
            config.Library = keepLibrary;
            LoadConfigIntoUi();
            Log("Loaded preset: YTP Deluxe Complete.");
        }

        private void btnBrowseFfmpeg_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "FFmpeg executable|ffmpeg.exe|Executable files|*.exe|All files|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    txtFfmpegPath.Text = dialog.FileName;
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "MP4 video|*.mp4";
                dialog.FileName = "YTPDeluxe_Output.mp4";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    txtOutputPath.Text = dialog.FileName;
            }
        }

        private void btnSavePreset_Click(object sender, EventArgs e)
        {
            SaveUiIntoConfig();
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "YTP preset|*.ytppreset.xml";
                dialog.FileName = "YTP Deluxe Complete.ytppreset.xml";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    Utilities.SavePreset(dialog.FileName, config);
                    Log("Preset saved: " + dialog.FileName);
                }
            }
        }

        private void btnLoadPreset_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "YTP preset|*.ytppreset.xml|XML files|*.xml|All files|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    config = Utilities.LoadPreset(dialog.FileName);
                    LoadConfigIntoUi();
                    Log("Preset loaded: " + dialog.FileName);
                }
            }
        }

        private void btnStartRender_Click(object sender, EventArgs e)
        {
            if (renderWorker.IsBusy)
            {
                renderWorker.CancelAsync();
                Log("Cancel requested.");
                return;
            }

            SaveUiIntoConfig();
            if (config.Library.Materials.Count == 0)
            {
                MessageBox.Show(this, "Import at least one video into Materials before rendering.", "Materials required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (String.IsNullOrEmpty(config.Settings.OutputPath))
            {
                MessageBox.Show(this, "Choose an output MP4 path.", "Output required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            progressBar.Value = 0;
            btnStartRender.Text = "Cancel Render";
            Log("Starting render...");
            renderWorker.RunWorkerAsync(config);
        }

        private void RenderWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                YTPGenerator generator = new YTPGenerator();
                generator.Log += delegate(object logSender, GeneratorLogEventArgs logArgs) { Log(logArgs.Message); };
                generator.ProgressChanged += delegate(object progressSender, GeneratorProgressEventArgs progressArgs)
                {
                    renderWorker.ReportProgress(progressArgs.Progress, progressArgs.Status);
                };
                generator.Render((GeneratorConfig)e.Argument, renderWorker);
            }
            catch (OperationCanceledException)
            {
                e.Cancel = true;
            }
        }

        private void renderWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int value = Clamp(e.ProgressPercentage, progressBar.Minimum, progressBar.Maximum);
            progressBar.Value = value;
            if (e.UserState != null)
                lblRenderStatus.Text = e.UserState.ToString();
        }

        private void RenderWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnStartRender.Text = "Start Render";
            if (e.Error != null)
            {
                Log("Render failed: " + e.Error.Message);
                MessageBox.Show(this, e.Error.Message, "Render failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (e.Cancelled)
            {
                Log("Render cancelled.");
                return;
            }

            progressBar.Value = 100;
            lblRenderStatus.Text = "Render complete";
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox box = new AboutBox())
                box.ShowDialog(this);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using SharedLibrary;
using SharedLibrary.Models;

namespace BWGenerator.Models
{
    public class ExportWindowViewModel : ModelBase
    {
        string fileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\untitled.wav";
        PresetModel preset = null;
        int exportProgressValue = 0;
        string statusText = "Ready to export";
        bool controlsAreEnabled = true;

        public string FileName
        {
            get => fileName;

            set
            {
                fileName = value;
                RaisePropertyChanged();
            }
        }

        public string FileSize
        {
            get
            {
                ulong samplesCount = (ulong)preset.TotalLength.TotalSeconds * 44100; // TODO: make discretization customizable somehow
                string suffix = "B";

                float fileSize = samplesCount * 2.0f * sizeof(short) + (ulong)WavFile.HeaderSize;
                if (fileSize > 1000.0f)
                {
                    fileSize /= 1024.0f;
                    suffix = "KB";
                }

                if (fileSize > 1000.0f)
                {
                    fileSize /= 1024.0f;
                    suffix = "MB";
                }

                if (fileSize > 1000.0f)
                {
                    fileSize /= 1024.0f;
                    suffix = "GB";
                }

                if (fileSize > 1000.0f)
                {
                    fileSize /= 1024.0f;
                    suffix = "TB";
                }

                return string.Format("{0:F2} {1}", fileSize, suffix);
            }
        }

        public int ExportProgressValue
        {
            get => exportProgressValue;

            set
            {
                exportProgressValue = value;
                RaisePropertyChanged();
            }
        }

        public string StatusText
        {
            get => statusText;
            set
            {
                statusText = value;
                RaisePropertyChanged();
            }
        }

        public bool ControlsAreEnabled
        {
            get => controlsAreEnabled;
            set
            {
                controlsAreEnabled = value;
                RaisePropertyChanged();
            }
        }

        public async void Export()
        {
            ControlsAreEnabled = false;
            preset.OnExportProgressUpdated += Preset_OnExportProgressUpdated;
            StatusText = "Working on it...";
            bool result = await preset.ExportPresetAsync(FileName);
            if (result)
            {
                StatusText = "File exported successfully";
                Process.Start("explorer.exe", string.Format("/select,\"{0}\"", FileName));
            }
            else
            {
                StatusText = "Failed to export";
                ExportProgressValue = 0;
            }
            preset.OnExportProgressUpdated -= Preset_OnExportProgressUpdated;
            ControlsAreEnabled = true;
        }

        private void Preset_OnExportProgressUpdated(object sender, PresetModel.ProgressUpdatedEventArgs e)
        {
            ExportProgressValue = e.Progress;
        }

        public ExportWindowViewModel(PresetModel preset)
        {
            this.preset = preset ?? throw new ArgumentNullException("preset");
        }
    }
}

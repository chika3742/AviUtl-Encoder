using System;
using System.Diagnostics;
using System.Net;
using System.Windows;

namespace AUEncoder
{
    /// <summary>
    /// UpdateDownloadWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class UpdateDownloadWindow : Window
    {
        string version;
        WebClient downloadClient;
        public UpdateDownloadWindow(string version)
        {
            InitializeComponent();

            this.version = version;
            string fileName = $"{Environment.GetEnvironmentVariable("temp")}\\AUEncoderSetup.msi";
            Uri uri = new Uri($"https://cchsubw.firebaseapp.com/distribution/ae/AUEncoderSetup.msi");

            if (downloadClient == null)
            {
                downloadClient = new WebClient();
                downloadClient.DownloadProgressChanged +=
                    new DownloadProgressChangedEventHandler(DownloadClient_DownloadProgressChanged);
                downloadClient.DownloadFileCompleted +=
                    new System.ComponentModel.AsyncCompletedEventHandler(DownloadClient_DownloadFileCompleted);
            }
            downloadClient.DownloadFileAsync(uri, fileName);
        }

        private void DownloadClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Close();
            }
            else if (e.Error == null)
            {
                cancel_button.IsEnabled = false;
                downloading_progress_text.Text = "インストーラーの起動中";
                Process.Start($"msiexec", $"/a {Environment.GetEnvironmentVariable("temp")}\\AUEncoderSetup.msi");
                Environment.Exit(0);
            }
            else
            {
                downloading_progress_text.Text = $"エラーが発生しました。\n{e.Error.Message}";
                download_progressbar.Visibility = Visibility.Collapsed;
            }
            cancel_button.IsEnabled = false;
        }

        private void DownloadClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            download_progressbar.Maximum = e.TotalBytesToReceive;
            download_progressbar.Value = e.BytesReceived;
            downloading_progress_text.Text = $"{e.ProgressPercentage}%  {e.TotalBytesToReceive / 1024}KiB/{e.BytesReceived / 1024}KiB Downloaded";
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            if (downloadClient != null)
            {
                downloadClient.CancelAsync();
            }
        }
    }
}

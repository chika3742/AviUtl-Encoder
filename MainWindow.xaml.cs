using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Taskbar;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    /// 
    

    public partial class MainWindow : Window
    {
        String AUCPathText;
        string WindowNumber;
        string CurrentFile;
        int CurrentNum;
        int AllFiles;
        bool IsRunning;
        StringBuilder LogStrBuilder = new StringBuilder();
        
        public MainWindow()
        {
            InitializeComponent();
            
            profileNumber.Text = Properties.Settings.Default.Profile_Number;
            pluginNumber.Text = Properties.Settings.Default.Plugin_Number;
            extFind.Text = Properties.Settings.Default.Find_Ext;
            extOutput.Text = Properties.Settings.Default.Output_Ext;

            if (Properties.Settings.Default.Plugin_Labels == null)
            {
                Properties.Settings.Default.Plugin_Labels = new List<Label>();
            }
            if (Properties.Settings.Default.Profile_Labels == null)
            {
                Properties.Settings.Default.Profile_Labels = new List<Label>();
            }

            Properties.Settings.Default.Profile_Labels.Sort((a, b) => a.Number - b.Number);
            foreach (Label label in Properties.Settings.Default.Profile_Labels)
            {
                Profile_ComboBox.Items.Add(label);
            }
            Properties.Settings.Default.Plugin_Labels.Sort((a, b) => a.Number - b.Number);
            foreach (Label label in Properties.Settings.Default.Plugin_Labels)
            {
                Plugin_ComboBox.Items.Add(label);
            }

            if (Properties.Settings.Default.Profile_Labels.Exists(delegate (Label l) { return l.Number.ToString() == profileNumber.Text; }))
            {
                Profile_ComboBox.SelectedItem = Properties.Settings.Default.Profile_Labels.Find(delegate (Label l) { return l.Number.ToString() == profileNumber.Text; });
            }
            if (Properties.Settings.Default.Plugin_Labels.Exists(delegate (Label l) { return l.Number.ToString() == pluginNumber.Text; }))
            {
                Plugin_ComboBox.SelectedItem = Properties.Settings.Default.Plugin_Labels.Find(delegate (Label l) { return l.Number.ToString() == pluginNumber.Text; });
            }
            LogStrBuilder.Append("Log Output");
            log.Text = LogStrBuilder.ToString();

            //progressBar.Style = ProgressBarStyle.Blocks;
        }
        
        private void Window_Closing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsRunning)
            {
                if (System.Windows.MessageBox.Show("出力を実行中です。本当に終了しますか？", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            Properties.Settings.Default.Profile_Number = profileNumber.Text;
            Properties.Settings.Default.Plugin_Number = pluginNumber.Text;
            Properties.Settings.Default.Find_Ext = extFind.Text;
            Properties.Settings.Default.Output_Ext = extOutput.Text;

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// AUCコマンドを実行します。
        /// </summary>
        /// <param name="cmd">実行するAUCコマンドと引数</param>
        /// <returns>実行結果</returns>
        public string RUN_COMMAND(string cmd)
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = false;

            p.StartInfo.CreateNoWindow = true;

            //for (int a = 0; a < cmd.Length; a++)
            //{
                p.StartInfo.Arguments = $"/c cd /d {AUCPathText} & {cmd}";
                p.Start();
            //}

            

            var result = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();

            p.WaitForExit();
            p.Close();

            return result;
        }

        /// <summary>
        /// 拡張子が入力されたものと一致するかチェックします。
        /// </summary>
        /// <param name="input">検査するファイルのパス</param>
        /// <returns></returns>
        private bool isExtMatch(string input)
        {
            var index = input.LastIndexOf('.');
            var ext = input.Substring(index);
            return ext == extFind.Text.Insert(0, ".");
        }

        /// <summary>
        /// それぞれの入力欄・ボタンのIsEnabledを一括で変更します。
        /// </summary>
        /// <param name="to">変更する値</param>
        private void switchState(bool to)
        {
            startButton.IsEnabled = to;
            fromFolder.IsEnabled = to;
            toFolder.IsEnabled = to;
            //completedFolder.IsEnabled = to;
            profileNumber.IsEnabled = to;
            pluginNumber.IsEnabled = to;
            extFind.IsEnabled = to;
            extOutput.IsEnabled = to;
            Plugin_ComboBox.IsEnabled = to;
            Profile_ComboBox.IsEnabled = to;

            openFile1.IsEnabled = to;
            openFile2.IsEnabled = to;
            pauseButton.IsEnabled = false;

            if (!to)
            {
                DDText1.Visibility = Visibility.Hidden;
                DDText2.Visibility = Visibility.Hidden;
            }
            else
            {
                DDText1.Visibility = Visibility.Visible;
                DDText2.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// エンコードを非同期で実行します。
        /// </summary>
        /// <param name="wndNum">AviUtlのウィンドウ番号</param>
        /// <param name="inputFiles">エンコード対象のファイル一覧</param>
        private async void runEncode(string wndNum, string[] inputFiles)
        {
            progressBar.Maximum = inputFiles.Length * 100;

            await Task.Run(() =>
            {
                int success = 0;
                int failure = 0;
                for (int i = 0; i < inputFiles.Length; i++)
                {
                    var inputFilePath = inputFiles[i];
                    string profNum = "";
                    string plugNum = "";
                    string forFolder = "";
                    string outputExt = "";
                    Dispatcher.Invoke(() =>
                    {
                        CurrentFile = inputFilePath.Substring(inputFilePath.LastIndexOf('\\'));
                        CurrentNum = i + 1;
                        AllFiles = inputFiles.Length;
                        progress.Text = $"{CurrentFile.Remove(0, 1)} を出力中・・・({CurrentNum}/{AllFiles})";
                        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                        TaskbarManager.Instance.SetProgressValue(i, inputFiles.Length);

                        profNum = profileNumber.Text;
                        plugNum = pluginNumber.Text;
                        forFolder = toFolder.Text;
                        outputExt = extOutput.Text.Insert(0, ".");
                        
                        progressBar.Value = i;
                        pb1.Value = 0;
                        progressPercent.Text = "0%";
                        AllProgressText.Text = "0%";
                    });

                    Dispatcher.Invoke(() =>
                    {
                        pauseButton.IsEnabled = false;
                        LogStrBuilder.Append($"\n{inputFilePath}:ファイルの読み込みを開始");
                        log.Text = LogStrBuilder.ToString();
                        LogScroller.ScrollToBottom();
                    });
                    var res1 = RUN_COMMAND($@"auc_open {wndNum} ""{inputFilePath}""");
                    Thread.Sleep(5000);
                    RUN_COMMAND($"auc_setprof {wndNum} {profNum}");
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(() =>
                    {
                        LogStrBuilder.Append($"\n{inputFilePath}:ファイルのエンコード開始");
                        log.Text = LogStrBuilder.ToString();
                        LogScroller.ScrollToBottom();
                    });
                    
                    var li = inputFilePath.LastIndexOf('\\');
                    var inputFileName = inputFilePath.Substring(li);
                    var filename = forFolder + inputFileName.Remove(inputFileName.LastIndexOf('.')) + "-enc" + outputExt;
                    Dispatcher.Invoke(() =>
                    {
                        LogStrBuilder.Append($"\n出力ファイル名：{filename}");
                        log.Text = LogStrBuilder.ToString();
                        LogScroller.ScrollToBottom();
                    });
                    RUN_COMMAND($@"auc_plugout {wndNum} {plugNum} ""{filename}""");
                    Dispatcher.Invoke(() =>
                    {
                        pauseButton.IsEnabled = true;
                    });
                    var res2 = RUN_COMMAND($"auc_wait {wndNum}");

                    if (res1.Trim() != "" || res2.Trim() != "")
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (res1.Trim().Contains("操作可能なプログラムまたはバッチ ファイルとして認識されていません。"))
                            {
                                LogStrBuilder.Append($"\n{inputFilePath}:ファイルの出力に失敗\n<エラー>AUCコマンドが見つかりません。AUCのパスが間違っている可能性があります。");
                                log.Text = LogStrBuilder.ToString();
                                LogScroller.ScrollToBottom();
                            } 
                            else
                            {
                                LogStrBuilder.Append($"\n{inputFilePath}:ファイルの出力に失敗");
                                log.Text = LogStrBuilder.ToString();
                                LogScroller.ScrollToBottom();
                            }
                            
                        });
                        failure++;
                        continue;
                    } else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LogStrBuilder.Append($"\n{inputFilePath}:ファイルの出力が完了({CurrentNum}/{AllFiles})");
                            log.Text = LogStrBuilder.ToString();
                            LogScroller.ScrollToBottom();
                        });
                        success++;
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    progress.Text = "完了";
                    switchState(true);
                    progressBar.Value = 0;
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                    LogStrBuilder.Append($"\n処理がすべて完了しました。({success}個成功/{failure}個失敗)");
                    log.Text = LogStrBuilder.ToString();
                    LogScroller.ScrollToBottom();
                    RUN_COMMAND($"auc_close {wndNum}");
                    pb1.Value = 0;
                    progressPercent.Text = "出力の進捗";
                    AllProgressText.Text = "全体の進捗";
                    IsRunning = false;
                });
                
            });
            
            
        }

        /// <summary>
        /// AviUtlのエンコードの進捗を取得します。
        /// </summary>
        private async void GetPercentage()
        {
            await Task.Run(() =>
            {
                while (IsRunning)
                {
                    var processes = System.Diagnostics.Process.GetProcessesByName("aviutl");
                    var AviUtlProcess = processes.ToList().Find(delegate (System.Diagnostics.Process process) { return process.MainWindowTitle.StartsWith("出力中 "); });
                    if (AviUtlProcess != null)
                    {
                        string percent;
                        if (AviUtlProcess.MainWindowTitle.Substring(4, 2).EndsWith("%"))
                        {
                            percent = AviUtlProcess.MainWindowTitle.Substring(4, 1);
                        }
                        else
                        {
                            percent = AviUtlProcess.MainWindowTitle.Substring(4, 2);
                        }
                        Dispatcher.Invoke(() =>
                        {
                            progressPercent.Text = percent + "%";
                            pb1.Value = double.Parse(percent);
                            progressBar.Value = int.Parse(percent) + ((CurrentNum - 1) * 100);
                            TaskbarManager.Instance.SetProgressValue(int.Parse(percent) + ((CurrentNum - 1) * 100), AllFiles * 100);
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            progressBar.Value = ((CurrentNum - 1) * 100); ;
                        });
                    }
                    //foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
                    //{
                    //    if (p.MainWindowTitle.Length != 0 && p.MainWindowTitle.StartsWith("出力中"))
                    //    {
                    //        string percent;
                    //        if (p.MainWindowTitle.Substring(4, 2).EndsWith("%"))
                    //        {
                    //            percent = p.MainWindowTitle.Substring(4, 1);
                    //        }
                    //        else
                    //        {
                    //            percent = p.MainWindowTitle.Substring(4, 2);
                    //        }
                    //        Dispatcher.Invoke(() =>
                    //        {
                    //            progressPercent.Text = percent + "%";
                    //            pb1.Value = double.Parse(percent);
                    //        });
                    //    }
                    //}
                    Thread.Sleep(1000);
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
            //{
            //    if (p.MainWindowTitle.Length != 0 && p.MainWindowTitle.StartsWith("出力中"))
            //    {
            //        MessageBox.Show(p.MainWindowTitle.Substring(4, 3));
            //    }
            //}
            
            AUCPathText = Properties.Settings.Default.AUC_Path;
            if (fromFolder.Text == "" ||
                toFolder.Text == "" ||
                profileNumber.Text == "" ||
                pluginNumber.Text == "" || extFind.Text == "" ||
                extOutput.Text == "")
            {
                MessageBox.Show("すべての欄に入力してください。");
                return;
            }
            string wndNum;
            wndNum = RUN_COMMAND("auc_findwnd");
            var aviutlPathText = Properties.Settings.Default.AviUtl_Path;
            if (wndNum.Trim() == "0")
            {
                if (aviutlPathText != "")
                {
                    log.Text = log.Text + "\nAviutlを起動中";
                    wndNum = RUN_COMMAND($"auc_exec {aviutlPathText}");
                }
                else
                {
                    MessageBox.Show("AviUtlのパスを入力してください。", "AviUtl Batch Encoder", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            wndNum = wndNum.Trim();

            string[] files = System.IO.Directory.GetFiles(fromFolder.Text);
            var filteredFiles = Array.FindAll(files, isExtMatch);
            if (filteredFiles.Length == 0)
            {
                MessageBox.Show("条件に合うファイルが見つかりません。", "AviUtl Batch Encoder", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            IsRunning = true;
            switchState(false);
            WindowNumber = wndNum;
            runEncode(wndNum, filteredFiles);
            GetPercentage();
            
        }

        private void profileNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            var text = profileNumber.Text + e.Text;
            if (regex.IsMatch(text))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
                Profile_ComboBox.SelectedIndex = 0;
            }
        }

        private void openFile1_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                fromFolder.Text = dialog.FileName;
            }
        }

        private void openFile2_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                toFolder.Text = dialog.FileName;
            }
        }
        

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (pauseButton.Content.ToString() == "一時停止")
            {
                RUN_COMMAND($"auc_veropen {WindowNumber}");
                pauseButton.Content = "再開";
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Paused);
                progress.Text = "出力を一時停止中";
            } else
            {
                RUN_COMMAND($"auc_verclose {WindowNumber}");
                pauseButton.Content = "一時停止";
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                progress.Text = $"{CurrentFile.Remove(0, 1)} を出力中・・・({CurrentNum}/{AllFiles})";
            }
        }

        private void MenuItem_Open_Preference_Click(object sender, RoutedEventArgs e)
        {
            var PrefWindow = new PreferenceWindow();
            PrefWindow.ShowDialog();

            Properties.Settings.Default.Profile_Labels.Sort((a, b) => a.Number - b.Number);
            while (Profile_ComboBox.Items.Count > 1)
            {
                Profile_ComboBox.Items.RemoveAt(1);
            }
            while (Plugin_ComboBox.Items.Count > 1)
            {
                Plugin_ComboBox.Items.RemoveAt(1);
            }

            foreach (Label label in Properties.Settings.Default.Profile_Labels)
            {
                Profile_ComboBox.Items.Add(label);
            }
            foreach (Label label in Properties.Settings.Default.Plugin_Labels)
            {
                Plugin_ComboBox.Items.Add(label);
            }

            if (Properties.Settings.Default.Profile_Labels.Exists(delegate (Label l) { return l.Number.ToString() == profileNumber.Text; }))
            {
                Profile_ComboBox.SelectedItem = Properties.Settings.Default.Profile_Labels.Find(delegate (Label l) { return l.Number.ToString() == profileNumber.Text; });
            }
            else
            {
                Profile_ComboBox.SelectedIndex = 0;
            }
            if (Properties.Settings.Default.Plugin_Labels.Exists(delegate (Label l) { return l.Number.ToString() == pluginNumber.Text; }))
            {
                Plugin_ComboBox.SelectedItem = Properties.Settings.Default.Plugin_Labels.Find(delegate (Label l) { return l.Number.ToString() == pluginNumber.Text; });
            }
            else
            {
                Plugin_ComboBox.SelectedIndex = 0;
            }
        }

        private void MenuItem_Open_Readme_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://sites.google.com/site/chikachuploader/aviutl-encoder/readme");
        }

        private void fromFolder_Drop(object sender, DragEventArgs e)
        {
            OnDropped(sender as TextBox, e);
        }

        private void fromFolder_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Copy;
            } else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void toFolder_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void toFolder_Drop(object sender, DragEventArgs e)
        {
            OnDropped(sender as TextBox, e);
        }

        static public void OnDropped(TextBox sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files.Count() != 1 || files == null)
            {
                MessageBox.Show("複数のフォルダーを指定することはできません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            };
            if (!System.IO.Directory.Exists(files[0]))
            {
                MessageBox.Show("ファイルを直接指定することはできません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            sender.Text = files[0];
        }

        private void MenuItem_GetVersion_Click(object sender, RoutedEventArgs e)
        {
            //var version = File.ReadAllText("Resources/version.json");
            //var text = version.Replace(Environment.NewLine, "");
            //dynamic obj = JObject.Parse(version);
            var versionStr = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(App).Assembly.Location).FileVersion;
            var client = new HttpClient();
            var text = client.GetStringAsync("https://script.google.com/macros/s/AKfycbyOj4z6rWTAUJWLJrOwCbF0g9s3ZoQGPF8xSIMTu3geOmVJ6fA/exec");
            text.ContinueWith(Task =>
            {
                if (text.IsFaulted)
                {
                    MessageBox.Show($"現在のバージョン:{versionStr}\n\n※アップデート情報の取得に失敗しました。", "バージョン情報", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var latestVersionInfo = JObject.Parse(text.Result);
                var verNum = versionStr.Replace(".", "");
                if (int.Parse(latestVersionInfo["version"].ToString().Replace(".", "")) > int.Parse(verNum))
                {
                    var result = MessageBox.Show($"現在のバージョン:{versionStr}\n\nアップデートがあります。最新バージョン:{latestVersionInfo["version"]}\n\n☆アップデート内容\n{latestVersionInfo["content"]}\n\nダウンロードしますか？", "バージョン情報", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(latestVersionInfo["url"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show($"現在のバージョン:{versionStr}", "バージョン情報", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Profile_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != 0 && Profile_ComboBox.SelectedItem != null)
            {
                profileNumber.Text = (Profile_ComboBox.SelectedItem as Label).Number.ToString();
            }
        }

        private void Plugin_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != 0 && Plugin_ComboBox.SelectedItem != null)
            {
                pluginNumber.Text = (Plugin_ComboBox.SelectedItem as Label).Number.ToString();
            }
        }

        private void MenuItem_Show_License_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad", @"""license.txt""");
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AllProgressText.Text = Math.Floor((progressBar.Value / progressBar.Maximum * 100)).ToString() + "%";
        }

        private void MenuItem_Open_Update_Info_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://sites.google.com/site/chikachuploader/aviutl-encoder");
        }
    }
}

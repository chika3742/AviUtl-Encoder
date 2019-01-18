using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Taskbar;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;
using System.Windows.Input;

namespace AUEncoder
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        string aucPath = "./bin/auc";
        string indexerPath = "./bin/aui_indexer/aui_indexer.exe";
        string WindowNumber;
        string CurrentFile;
        string extFindFirst = "";
        int CurrentNum;
        int AllFiles;
        bool IsRunning;
        Properties.Settings Settings;
        StringBuilder LogStrBuilder = new StringBuilder();
        private List<string> filesList = new List<string>();
        
        public MainWindow()
        {
            InitializeComponent();

            Settings = Properties.Settings.Default;

            fromFolder.Text = Settings.Input_Folder_Path;
            toFolder.Text = Settings.Output_Folder_Path;
            profileNumber.Text = Settings.Profile_Number;
            pluginNumber.Text = Settings.Plugin_Number;
            extFind.Text = Settings.Find_Ext;
            extOutput.Text = Settings.Output_Ext;

            if (Settings.Plugin_Labels == null)
            {
                Settings.Plugin_Labels = new List<Label>();
            }
            if (Settings.Profile_Labels == null)
            {
                Settings.Profile_Labels = new List<Label>();
            }

            Settings.Profile_Labels.Sort((a, b) => a.Number - b.Number);
            foreach (Label label in Settings.Profile_Labels)
            {
                Profile_ComboBox.Items.Add(label);
            }
            Settings.Plugin_Labels.Sort((a, b) => a.Number - b.Number);
            foreach (Label label in Settings.Plugin_Labels)
            {
                Plugin_ComboBox.Items.Add(label);
            }

            if (Settings.Profile_Labels.Exists(delegate (Label l) { return l.Number.ToString() == profileNumber.Text; }))
            {
                Profile_ComboBox.SelectedItem = Settings.Profile_Labels.Find(delegate (Label l) { return l.Number.ToString() == profileNumber.Text; });
            }
            if (Settings.Plugin_Labels.Exists(delegate (Label l) { return l.Number.ToString() == pluginNumber.Text; }))
            {
                Plugin_ComboBox.SelectedItem = Settings.Plugin_Labels.Find(delegate (Label l) { return l.Number.ToString() == pluginNumber.Text; });
            }
            LogStrBuilder.Append("Log Output");
            log.Text = LogStrBuilder.ToString();

            ModeSelector.SelectedIndex = Settings.Mode;
            Behavior_After_Encoding.SelectedIndex = Settings.Behavior_After_Encoding;
            QuitAviUtlCheckbox.IsChecked = Settings.Behavior_Quit_AviUtl;
            QuitThisSoftwareCheckbox.IsChecked = Settings.Behavior_Quit_Software;

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

            Settings.Input_Folder_Path = fromFolder.Text;
            Settings.Output_Folder_Path = toFolder.Text;
            Settings.Profile_Number = profileNumber.Text;
            Settings.Plugin_Number = pluginNumber.Text;
            Settings.Find_Ext = extFind.Text;
            Settings.Output_Ext = extOutput.Text;
            Settings.Mode = ModeSelector.SelectedIndex;
            Settings.Behavior_After_Encoding = Behavior_After_Encoding.SelectedIndex;
            Settings.Behavior_Quit_AviUtl = (bool)QuitAviUtlCheckbox.IsChecked;
            Settings.Behavior_Quit_Software = (bool)QuitThisSoftwareCheckbox.IsChecked;

            Settings.Save();
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
                p.StartInfo.Arguments = $"/c cd /d {aucPath} & {cmd}";
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
        private void SwitchState(bool to)
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
            MenuItem_Preference.IsEnabled = to;
            ModeSelector.IsEnabled = to;

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
            ModeSelector_SelectionChanged(ModeSelector, null);
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
                bool exit = false;
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
                                if (res2.Trim().Contains("AviUtl"))
                                {
                                    LogStrBuilder.Append($"\n{inputFilePath}:ファイルの出力に失敗\n" +
                                        "＜info＞AviUtlが終了された可能性があるため、出力処理を中断しました。");
                                    log.Text = LogStrBuilder.ToString();
                                    LogScroller.ScrollToBottom();
                                    exit = true;
                                }
                                else
                                {
                                    LogStrBuilder.Append($"\n{inputFilePath}:ファイルの出力に失敗");
                                    log.Text = LogStrBuilder.ToString();
                                    LogScroller.ScrollToBottom();
                                }
                            }
                            
                        });
                        if (exit) break;
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
                if (exit)
                {
                    Dispatcher.Invoke(() =>
                    {
                        progress.Text = "中断";
                        SwitchState(true);
                        progressBar.Value = 0;
                        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                        pb1.Value = 0;
                        progressPercent.Text = "出力の進捗";
                        AllProgressText.Text = "全体の進捗";
                        IsRunning = false;
                    });
                    return;
                }
                Dispatcher.Invoke(() =>
                {
                    progress.Text = "完了";
                    SwitchState(true);
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

                    if ((!Settings.Follow_Behavior_Setting && failure == 0) || Settings.Follow_Behavior_Setting)
                    {
                        if (QuitAviUtlCheckbox.IsChecked == true)
                        {
                            if (RUN_COMMAND("auc_exit").Contains("操作可能なプログラムまたはバッチ ファイルとして認識されていません。"))
                            {
                                LogStrBuilder.Append("\n<Error>AUCコマンドが見つかりません。正しくパスが設定されているか確認してください。");
                                log.Text = LogStrBuilder.ToString();
                            }

                        }

                        switch ((Behavior_After_Encoding.SelectedItem as ComboBoxItem).Content)
                        {
                            case "シャットダウン":
                                var startInfo = new ProcessStartInfo();
                                startInfo.FileName = "shutdown";
                                startInfo.Arguments = "/s /hybrid /t 0";
                                startInfo.UseShellExecute = false;
                                startInfo.CreateNoWindow = true;
                                Process.Start(startInfo);
                                break;
                            case "休止状態(ハイバネート)":
                                System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Hibernate, false, false);
                                break;
                            case "スリープ(サスペンド)":
                                System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Suspend, false, false);
                                break;
                        }
                        if (QuitThisSoftwareCheckbox.IsChecked == true)
                        {
                            Close();
                        }
                    }
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
                if (Settings.Getting_Progress_Interval == 0) return;
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
                    Thread.Sleep(Settings.Getting_Progress_Interval);
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var errorStrBuilder = new StringBuilder();
            
            if (fromFolder.Text == "" || (ModeSelector.SelectedIndex == 1 && filesList.Count == 0)) errorStrBuilder.Append("「エンコード対象のフォルダー」が入力されていません。\n");
            else if (!Directory.Exists(fromFolder.Text) && ModeSelector.SelectedIndex != 1) errorStrBuilder.Append("「エンコード対象のフォルダー」は存在しません。\n");
            if (toFolder.Text == "") errorStrBuilder.Append("「エンコードしたファイルの保存場所」が入力されていません。\n");
            else if (!Directory.Exists(toFolder.Text)) errorStrBuilder.Append("「エンコードしたファイルの保存場所」は存在しません。\n");
            if (profileNumber.Text == "") errorStrBuilder.Append("プロファイルが指定されていません。\n");
            if (pluginNumber.Text == "")errorStrBuilder.Append("出力プラグインが指定されていません。\n");
            if (extFind.Text == "" && ModeSelector.SelectedIndex != 1) errorStrBuilder.Append("「検索対象のファイル拡張子」が指定されていません。\n");
            if (extOutput.Text == "") errorStrBuilder.Append("「出力ファイル拡張子」が指定されていません。\n");
            if (!File.Exists(Settings.AviUtl_Path)) errorStrBuilder.Append("設定画面の「AviUtlのパス」が指定されていないか、存在しません。\n");

            if (errorStrBuilder.ToString() != "")
            {
                MessageBox.Show(errorStrBuilder.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ModeSelector.SelectedIndex == 1)
            {
                Start_Encoding(filesList.ToArray());
            }
            else
            {
                string[] files = System.IO.Directory.GetFiles(fromFolder.Text);
                var filteredFiles = Array.FindAll(files, isExtMatch);
                if (filteredFiles.Length == 0)
                {
                    MessageBox.Show("条件に合うファイルが見つかりません。", "AviUtl Encoder", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Start_Encoding(filteredFiles);
            }
        }

        /// <summary>
        /// エンコードの開始処理を非同期で実行します。
        /// </summary>
        /// <param name="Files">エンコード対象ファイル一覧</param>
        private async void Start_Encoding(string[] Files)
        {
            await Task.Run(() =>
            {
                string WndNum = RUN_COMMAND("auc_findwnd").Trim();
                if (WndNum == "0")
                {
                    if (Settings.AviUtl_Path != "")
                    {
                        LogStrBuilder.Append("\nAviUtlを起動中です。");
                        Dispatcher.Invoke(() =>
                        {
                            log.Text = LogStrBuilder.ToString();
                        });
                        WndNum = RUN_COMMAND($"auc_exec {Settings.AviUtl_Path}").Trim();
                    }
                    else
                    {
                        MessageBox.Show("AviUtlのパスを入力してください。", "AviUtl Encoder", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                IsRunning = true;
                Dispatcher.Invoke(() =>
                {
                    Activate();
                    SwitchState(false);
                });
                
                    if (!File.Exists(indexerPath))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            SwitchState(true);
                            MessageBox.Show("aui_indexerが見つかりません。再インストールしてください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                        return;
                    }

                    var proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = indexerPath;
                    proc.StartInfo.WorkingDirectory = Settings.AviUtl_Path.Remove(Settings.AviUtl_Path.LastIndexOf('\\'));
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardInput = true;

                    var arg = new StringBuilder();
                    for (int i = 0; i < Files.Length; i++)
                    {
                        arg.Append($@"""{Files[i]}"" ");
                    }

                    proc.StartInfo.Arguments = arg.ToString();

                    proc.StartInfo.RedirectStandardOutput = true;
                    //proc.StartInfo.RedirectStandardError = true;

                    proc.OutputDataReceived += proc_OutputDataReceived;

                    LogStrBuilder.Append($"\nインデックスファイルを事前生成します。");
                    Dispatcher.Invoke(() =>
                    {
                        log.Text = LogStrBuilder.ToString();
                    });

                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.WaitForExit();
                    proc.Close();
                
                WindowNumber = WndNum;
                Dispatcher.Invoke(() =>
                {
                    runEncode(WndNum, Files);
                    GetPercentage();
                });
            });
        }

        private async void proc_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            await Task.Run(() =>
            {
                if (e.Data == null) return;
                if (e.Data.StartsWith("could not find aui file."))
                {
                    LogStrBuilder.Append("\n＜エラー＞auiの検索に失敗しました。L-SMASH Works File Readerがインストールされていない可能性があります。");
                    Dispatcher.Invoke(() =>
                    {
                        log.Text = LogStrBuilder.ToString();
                    });
                    return;
                }

                var data = e.Data.Substring(11).Remove(e.Data.Substring(11).LastIndexOf(" ..."));
                LogStrBuilder.Append($"\n{data} のインデックスファイルを生成しています...");
                Dispatcher.Invoke(() =>
                {
                    log.Text = LogStrBuilder.ToString();
                });
            });
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
            if ((string)(sender as Button).Content == "参照")
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    fromFolder.Text = dialog.FileName;
                }
            } else
            {
                var dialog = new SelectFilesWindow(filesList);
                dialog.ShowDialog();
                if (dialog.changeList)
                {
                    filesList = dialog.items;
                }
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

            Settings.Profile_Labels.Sort((a, b) => a.Number - b.Number);
            while (Profile_ComboBox.Items.Count > 1)
            {
                Profile_ComboBox.Items.RemoveAt(1);
            }
            while (Plugin_ComboBox.Items.Count > 1)
            {
                Plugin_ComboBox.Items.RemoveAt(1);
            }

            foreach (Label label in Settings.Profile_Labels)
            {
                Profile_ComboBox.Items.Add(label);
            }
            foreach (Label label in Settings.Plugin_Labels)
            {
                Plugin_ComboBox.Items.Add(label);
            }

            if (Settings.Profile_Labels.Exists(delegate (Label l) { return l.Number.ToString() == profileNumber.Text; }))
            {
                Profile_ComboBox.SelectedItem = Settings.Profile_Labels.Find(delegate (Label l) { return l.Number.ToString() == profileNumber.Text; });
            }
            else
            {
                Profile_ComboBox.SelectedIndex = 0;
            }
            if (Settings.Plugin_Labels.Exists(delegate (Label l) { return l.Number.ToString() == pluginNumber.Text; }))
            {
                Plugin_ComboBox.SelectedItem = Settings.Plugin_Labels.Find(delegate (Label l) { return l.Number.ToString() == pluginNumber.Text; });
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

        public void fromFolder_PreviewDragOver(object sender, DragEventArgs e)
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
            Close();
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

        private void MenuItem_Show_Settings_File_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(ConfigurationManager.OpenExeConfiguration(
      ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath.Remove(ConfigurationManager.OpenExeConfiguration(
      ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath.LastIndexOf("\\")));
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void MenuItem_Show_Log(object sender, RoutedEventArgs e)
        {
            var cBox = (MenuItem)sender;
            if (cBox.IsChecked)
            {
                cBox.IsChecked = false;
                LogScroller.Visibility = Visibility.Hidden;
            }
            else
            {
                cBox.IsChecked = true;
                LogScroller.Visibility = Visibility.Visible;
            }
        }

        private void ModeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cBox = (ComboBox)sender;
            switch(cBox.SelectedIndex) {
                case 0:
                    extFind.Text = extFindFirst;
                    extFind.IsEnabled = true;
                    fromFolder.IsEnabled = true;
                    openFile1.Content = "参照";
                    break;
                case 1:
                    extFind.Text = extFindFirst;
                    extFind.IsEnabled = false;
                    fromFolder.IsEnabled = false;
                    openFile1.Content = "選択";
                    break;
                case 2:
                    extFindFirst = extFind.Text;
                    extFind.Text = "aup";
                    extFind.IsEnabled = false;
                    fromFolder.IsEnabled = true;
                    openFile1.Content = "参照";
                    break;
                default:
                    break;
            }
        }

        private void ExtFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ModeSelector.SelectedIndex != 2)
            {
                extFindFirst = extFind.Text;
            }
        }

        private void AfterProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cBox = (ComboBox)sender;
            if (cBox.SelectedIndex == 1)
            {
                QuitThisSoftwareCheckbox.IsEnabled = false;
                QuitAviUtlCheckbox.IsEnabled = false;
            }
            else if (QuitThisSoftwareCheckbox != null)
            {
                QuitThisSoftwareCheckbox.IsEnabled = true;
                QuitAviUtlCheckbox.IsEnabled = true;
            }
        }

        private void pluginNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
                Plugin_ComboBox.SelectedIndex = 0;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.AviUtl_Path == "")
            {
                MessageBox.Show("最初にAviUtlのパスを設定してください。", "AviUtlエンコーダーへようこそ", MessageBoxButton.OK, MessageBoxImage.Information);
                new PreferenceWindow().ShowDialog();
            }
        }
    }
}

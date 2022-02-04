using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Taskbar;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace AUEncoder
{
    /// <summary>
    /// MainWindowの相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("user32.dll")]
        static extern Int32 FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;    // FLASHWINFO構造体のサイズ
            /// <summary>
            /// 点滅対象のウィンドウ・ハンドル
            /// </summary>
            public IntPtr hwnd;      // 点滅対象のウィンドウ・ハンドル
            /// <summary>
            /// 「FLASHW_XXX」のいずれか
            /// </summary>
            public UInt32 dwFlags;   // 以下の「FLASHW_XXX」のいずれか
            public UInt32 uCount;    // 点滅する回数
            /// <summary>
            /// 点滅する間隔（ミリ秒単位）
            /// </summary>
            public UInt32 dwTimeout; // 点滅する間隔（ミリ秒単位）
        }


        public const UInt32 FLASHW_STOP = 0;        // 点滅を止める
        public const UInt32 FLASHW_CAPTION = 1;     // タイトルバーを点滅させる
        public const UInt32 FLASHW_TRAY = 2;        // タスクバー・ボタンを点滅させる
        public const UInt32 FLASHW_ALL = 3;         // タスクバー・ボタンとタイトルバーを点滅させる
        public const UInt32 FLASHW_TIMER = 4;       // FLASHW_STOPが指定されるまでずっと点滅させる
        /// <summary>
        /// ウィンドウが最前面に来るまでずっと点滅させる
        /// </summary>
        public const UInt32 FLASHW_TIMERNOFG = 12;  // ウィンドウが最前面に来るまでずっと点滅させる

        readonly string AUC_DIR = Path.Combine(Environment.CurrentDirectory, "lib/auc/");
        readonly string AUI_INDEXER_PATH = Path.Combine(Environment.CurrentDirectory, "lib/aui_indexer/aui_indexer.exe");
        readonly string FFMPEG_PATH = Path.Combine(Environment.CurrentDirectory, "lib/ffmpeg/ffmpeg.exe");
        readonly string MEDIAINFO_PATH = Path.Combine(Environment.CurrentDirectory, "lib/mediainfo/mediainfo.exe");

        string WindowNumber;
        string CurrentFile;
        string extFindFirst = "";
        int CurrentNum;
        int AllFiles;
        bool IsRunning;
        Properties.Settings Settings;
        StringBuilder LogStrBuilder = new StringBuilder();
        private List<string> Files = new List<string>();
        CancellationTokenSource tokenSource;

        bool errorOccured = false;

        int currentNumber = 0;

        public MainWindow()
        {
            InitializeComponent();

            LogStrBuilder.Append("Log Output");
            var item = new ListBoxItem();
            item.Content = "Log Output";
            item.Selected += Item_Selected;
            log.Items.Add(item);

            //progressBar.Style = ProgressBarStyle.Blocks;
        }

        public void Window_Initialized(object sender, EventArgs e)
        {
            InitFormState();
        }

        private void InitFormState()
        {
            Settings = Properties.Settings.Default;

            Video_Bitrate_TextBox.InputBindings.Add(new KeyBinding(ApplicationCommands.NotACommand, Key.Space, ModifierKeys.None));
            Video_Bitrate_TextBox.InputBindings.Add(new KeyBinding(ApplicationCommands.NotACommand, Key.Space, ModifierKeys.Shift));

            Source_Directory_TextBox.Text = Settings.Input_Folder_Path;
            Destination_Directory_TextBox.Text = Settings.Output_Folder_Path;
            Profile_Number_TextBox.Text = Settings.Profile_Number;
            Plugin_Number_TextBox.Text = Settings.Plugin_Number;
            Target_Extension_TextBox.Text = Settings.Find_Ext;
            Output_Extension_TextBox.Text = Settings.Output_Ext;

            Mode_ComboBox.SelectedIndex = Settings.Mode;
            UpdateFormStateForMode();

            Video_Codec_ComboBox.SelectedIndex = Settings.Video_Codec;
            ACodecComboBox.SelectedIndex = Settings.Audio_Codec;
            UpdateABitrateComboBoxStateForACodec();
            UpdateACodecComboBoxStateForVCodec();
            Video_Bitrate_TextBox.Text = Settings.Video_Bitrate;
            ABitrateComboBox.SelectedIndex = Settings.Audio_Bitrate;
            ForceOverwriteCheckBox.IsChecked = Settings.Force_Overwrite;

            if (Settings.Plugin_Labels == null)
            {
                Settings.Plugin_Labels = new List<PluginLabel>();
            }
            if (Settings.Profile_Labels == null)
            {
                Settings.Profile_Labels = new List<ProfileLabel>();
            }

            Settings.Profile_Labels.Sort((a, b) => a.Number - b.Number);
            foreach (ProfileLabel label in Settings.Profile_Labels)
            {
                Profile_ComboBox.Items.Add(label);
            }
            Settings.Plugin_Labels.Sort((a, b) => a.Number - b.Number);
            foreach (PluginLabel label in Settings.Plugin_Labels)
            {
                Plugin_ComboBox.Items.Add(label);
            }

            if (Settings.Profile_Labels.Exists(delegate (ProfileLabel l) { return l.Number.ToString() == Profile_Number_TextBox.Text; }))
            {
                Profile_ComboBox.SelectedItem = Settings.Profile_Labels.Find(delegate (ProfileLabel l) { return l.Number.ToString() == Profile_Number_TextBox.Text; });
            }
            if (Settings.Plugin_Labels.Exists(delegate (PluginLabel l) { return l.Number.ToString() == Plugin_Number_TextBox.Text; }))
            {
                Plugin_ComboBox.SelectedItem = Settings.Plugin_Labels.Find(delegate (PluginLabel l) { return l.Number.ToString() == Plugin_Number_TextBox.Text; });
            }
            if (Settings.Restore_After_Process)
            {
                Operation_After_Encoded.SelectedIndex = Settings.Behavior_After_Encoding;
                Quit_AviUtl_CheckBox.IsChecked = Settings.Behavior_Quit_AviUtl;
                Quit_AUEnc_CheckBox.IsChecked = Settings.Behavior_Quit_Software;
            }

            if (!Settings.Show_Log)
            {
                log.Visibility = Visibility.Collapsed;
                ((MenuItem)Bar_Menu.FindName("Show_Log")).IsChecked = false;
            }
        }

        private void Item_Selected(object sender, RoutedEventArgs e)
        {
            ((ListBoxItem)sender).IsSelected = false;
        }

        private void Window_Closing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsRunning)
            {
                if (MessageBox.Show("出力を実行中です。本当に終了しますか？", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            Settings.Input_Folder_Path = Source_Directory_TextBox.Text;
            Settings.Output_Folder_Path = Destination_Directory_TextBox.Text;
            Settings.Profile_Number = Profile_Number_TextBox.Text;
            Settings.Plugin_Number = Plugin_Number_TextBox.Text;
            Settings.Find_Ext = Target_Extension_TextBox.Text;
            Settings.Output_Ext = Output_Extension_TextBox.Text;
            Settings.Mode = Mode_ComboBox.SelectedIndex;
            Settings.Behavior_After_Encoding = Operation_After_Encoded.SelectedIndex;
            Settings.Behavior_Quit_AviUtl = (bool)Quit_AviUtl_CheckBox.IsChecked;
            Settings.Behavior_Quit_Software = (bool)Quit_AUEnc_CheckBox.IsChecked;
            Settings.Show_Log = log.Visibility == Visibility.Visible;
            Settings.Video_Codec = Video_Codec_ComboBox.SelectedIndex;
            Settings.Audio_Codec = ACodecComboBox.SelectedIndex;
            Settings.Video_Bitrate = Video_Bitrate_TextBox.Text;
            Settings.Audio_Bitrate = ABitrateComboBox.SelectedIndex;
            Settings.Force_Overwrite = (bool)ForceOverwriteCheckBox.IsChecked;

            Settings.Save();
        }

        /// <summary>
        /// AUCコマンドを実行します。
        /// </summary>
        /// <param name="cmd">実行するAUCコマンドと引数</param>
        /// <returns>実行結果</returns>
        public string RUN_COMMAND(string cmd)
        {
            var p = new Process();
            p.StartInfo.FileName = Environment.GetEnvironmentVariable("ComSpec");
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = false;
            p.EnableRaisingEvents = true;

            p.StartInfo.CreateNoWindow = true;
            
            p.StartInfo.Arguments = $"/c cd /d {AUC_DIR} & {cmd}";
            p.Start();

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
        private bool IsExtMatch(string input)
        {
            if (Target_Extension_TextBox.Text == "")
            {
                return true;
            }
            var extension = input.Split('.').Last();
            return String.Compare(extension, Target_Extension_TextBox.Text, true) == 0;
        }

        /// <summary>
        /// それぞれの入力欄・ボタンのIsEnabledを一括で変更します。
        /// </summary>
        /// <param name="to">変更する値</param>
        private void SwitchState(bool to)
        {
            startButton.IsEnabled = to;
            Source_Directory_TextBox.IsEnabled = to;
            Destination_Directory_TextBox.IsEnabled = to;
            //completedFolder.IsEnabled = to;
            Profile_Number_TextBox.IsEnabled = to;
            Plugin_Number_TextBox.IsEnabled = to;
            Target_Extension_TextBox.IsEnabled = to;
            Output_Extension_TextBox.IsEnabled = to;
            Plugin_ComboBox.IsEnabled = to;
            Profile_ComboBox.IsEnabled = to;
            MenuItem_Preference.IsEnabled = to;
            Mode_ComboBox.IsEnabled = to;
            interrupt_button.IsEnabled = !to;

            if (to)
            {
                Video_Codec_ComboBox.IsEnabled = true;
                UpdateACodecComboBoxStateForVCodec();
                UpdateABitrateComboBoxStateForACodec();
            }
            else
            {
                Video_Codec_ComboBox.IsEnabled = false;
                ACodecComboBox.IsEnabled = false;
                Video_Bitrate_TextBox.IsEnabled = false;
                ABitrateComboBox.IsEnabled = false;
            }

            openFile1.IsEnabled = to;
            openFile2.IsEnabled = to;
            Pause_Button.IsEnabled = false;

            if (!to)
            {
                DDText1.Visibility = Visibility.Hidden;
                DDText2.Visibility = Visibility.Hidden;
            }
            else
            {
                UpdateAfterProcessCheckBoxState();
                DDText1.Visibility = Visibility.Visible;
                DDText2.Visibility = Visibility.Visible;
            }
            
        }

        /// <summary>
        /// エンコードを非同期で実行します。
        /// </summary>
        /// <param name="wndNum">AviUtlのウィンドウ番号</param>
        /// <param name="inputFiles">エンコード対象のファイル一覧</param>
        private async void RunEncodeProcess(string wndNum, List<string> inputFiles)
        {
            progressBar.Maximum = inputFiles.Count * 100;
            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            await Task.Run(() =>
            {
                int success = 0;
                int failure = 0;
                bool exit = false;
                for (int i = 0; i < inputFiles.Count; i++)
                {
                    if (!IsRunning)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            CancelEncoding();
                        });
                        return;
                    }
                    var inputFilePath = inputFiles[i];
                    string profNum = "";
                    string plugNum = "";
                    string forFolder = "";
                    string outputExt = "";
                    Dispatcher.Invoke(() =>
                    {
                        CurrentFile = inputFilePath.Substring(inputFilePath.LastIndexOf('\\'));
                        CurrentNum = i + 1;
                        AllFiles = inputFiles.Count;
                        status.Text = $"{CurrentFile.Remove(0, 1)} を出力中・・・({CurrentNum}/{AllFiles})";
                        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                        TaskbarManager.Instance.SetProgressValue(i, inputFiles.Count);

                        profNum = Profile_Number_TextBox.Text;
                        plugNum = Plugin_Number_TextBox.Text;
                        forFolder = Destination_Directory_TextBox.Text;
                        outputExt = Output_Extension_TextBox.Text.Insert(0, ".");
                        
                        progressBar.Value = i;
                        pb1.Value = 0;
                        progressPercent.Text = "0%";
                        AllProgressText.Text = "0%";
                    });

                    Dispatcher.Invoke(() =>
                    {
                        Pause_Button.IsEnabled = false;
                        OutputLog($"{inputFilePath}:ファイルの読み込みを開始");
                    });
                    var res1 = RUN_COMMAND($@"auc_open {wndNum} ""{inputFilePath}""");
                    Thread.Sleep(5000);
                    RUN_COMMAND($"auc_setprof {wndNum} {profNum}");
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(() =>
                    {
                        OutputLog($"{inputFilePath}:ファイルのエンコード開始");
                    });
                    
                    var li = inputFilePath.LastIndexOf('\\');
                    var inputFileName = inputFilePath.Substring(li);
                    var filename = forFolder + inputFileName.Remove(inputFileName.LastIndexOf('.')) + "-enc" + outputExt;
                    Dispatcher.Invoke(() =>
                    {
                        OutputLog($"出力ファイル名：{filename}");
                    });
                    RUN_COMMAND($@"auc_plugout {wndNum} {plugNum} ""{filename}""");
                    Dispatcher.Invoke(() =>
                    {
                        Pause_Button.IsEnabled = true;
                    });
                    var res2 = RUN_COMMAND($"auc_wait {wndNum}");

                    if (token.IsCancellationRequested)
                    {
                        exit = true;
                        break;
                    }

                    if (res1.Trim() != "" || res2.Trim() != "")
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (res1.Trim().Contains("操作可能なプログラムまたはバッチ ファイルとして認識されていません。"))
                            {
                                OutputLog($"{inputFilePath}:ファイルの出力に失敗\n<エラー>AUCコマンドが見つかりません。AUCのパスが間違っている可能性があります。");
                            } 
                            else
                            {
                                if (res2.Trim().Contains("AviUtl"))
                                {
                                    OutputLog($"{inputFilePath}:ファイルの出力に失敗");
                                    OutputLog("＜info＞AviUtlが終了されたため、出力処理を中断しました。");
                                    exit = true;
                                }
                                else
                                {
                                    OutputLog($"{inputFilePath}:ファイルの出力に失敗");
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
                            OutputLog($"{inputFilePath}:ファイルの出力が完了({CurrentNum}/{AllFiles})");
                        });
                        success++;
                    }
                }
                if (exit)
                {
                    Dispatcher.Invoke(() =>
                    {
                        CancelEncoding();
                    });
                    return;
                }
                Dispatcher.Invoke(() =>
                {
                    status.Text = "完了";
                    SwitchState(true);
                    progressBar.Value = 0;
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                    OutputLog($"処理がすべて完了しました。({success}個成功/{failure}個失敗)");
                    RUN_COMMAND($"auc_close {wndNum}");
                    pb1.Value = 0;
                    progressPercent.Text = "出力の進捗";
                    AllProgressText.Text = "全体の進捗";
                    IsRunning = false;
                    CompleteEncoding();
                });
                
            }, token);

        }

        private void CancelEncoding()
        {
            RUN_COMMAND("auc_close");
            status.Text = "中断";
            SwitchState(true);
            progressBar.Value = 0;
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            pb1.Value = 0;
            progressPercent.Text = "出力の進捗";
            AllProgressText.Text = "全体の進捗";
            IsRunning = false;
            progressBar.IsIndeterminate = false;
        }

        private void CompleteEncoding()
        {
            if ((!Settings.Follow_Behavior_Setting) || Settings.Follow_Behavior_Setting)
            {
                if (Quit_AviUtl_CheckBox.IsChecked == true)
                {
                    if (RUN_COMMAND("auc_exit").Contains("操作可能なプログラムまたはバッチ ファイルとして認識されていません。"))
                    {
                        OutputLog("<Error>AUCコマンドが見つかりません。正しくパスが設定されているか確認してください。");
                    }

                }

                switch ((Operation_After_Encoded.SelectedItem as ComboBoxItem).Content)
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
                if (Quit_AUEnc_CheckBox.IsChecked == true)
                {
                    Close();
                }
            }
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
                    var processes = Process.GetProcessesByName("aviutl");
                    var AviUtlProcess = processes.ToList().Find(delegate (Process process) { return process.MainWindowTitle.StartsWith("出力中 "); });
                    if (AviUtlProcess != null && IsRunning)
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

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            var mode = Mode_ComboBox.SelectedIndex;

            if (((ComboBoxItem)Video_Codec_ComboBox.SelectedItem).Name == "av1")
            {
                if (MessageBox.Show($"このコーデックはエンコードに非常に時間がかかるため推奨しません。それでも続行しますか？", "AOMedia Video Codecでのエンコードについて", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            if (mode == 3)
            {
                Start_Encoding_With_FFmpeg();
            }
            else
            {
                Start_Encoding_With_AviUtl();
            }
        }

        /// <summary>
        /// エンコードの開始処理を非同期で実行します。
        /// </summary>
        private async void Start_Encoding_With_AviUtl()
        {
            var mode = Mode_ComboBox.SelectedIndex;

            List<string> fileList;
            if (mode == 1)
            {
                if (Files.Count == 0)
                {
                    MessageBox.Show("エンコードするファイルが選択されていません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                fileList = Files;
            }
            else
            {
                if (!Directory.Exists(Source_Directory_TextBox.Text))
                {
                    MessageBox.Show("エンコード対象のフォルダーが指定されていないか、存在しません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string[] files = Directory.GetFiles(Source_Directory_TextBox.Text);
                var filteredFiles = Array.FindAll(files, IsExtMatch);
                if (filteredFiles.Length == 0)
                {
                    MessageBox.Show("フォルダー内にエンコード対象のファイルが見つかりません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                fileList = files.ToList();
            }

            // エラーチェック
            var errorStrBuilder = new StringBuilder();
            if (!Directory.Exists(Destination_Directory_TextBox.Text)) errorStrBuilder.AppendLine("「エンコードしたファイルの保存場所」が入力されていないか、存在しません。");
            if (Profile_Number_TextBox.Text == "") errorStrBuilder.AppendLine("プロファイルが指定されていません。");
            if (Plugin_Number_TextBox.Text == "") errorStrBuilder.AppendLine("出力プラグインが指定されていません。");
            if (Output_Extension_TextBox.Text == "") errorStrBuilder.AppendLine("「出力ファイル拡張子」が指定されていません。");
            if (!File.Exists(Settings.AviUtl_Path)) errorStrBuilder.AppendLine("「AviUtlのパス」が指定されていないか、存在しません。「ファイル＞設定」から設定してください。");

            if (errorStrBuilder.Length != 0)
            {
                MessageBox.Show(errorStrBuilder.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 確認を促すダイアログ
            if (!Settings.Do_File_Analize)
            {
                if (MessageBox.Show($"AviUtlの環境設定を確認し、OKをクリックしてください。\n\n・最大画像サイズは入力ファイルのサイズ以上か\n・最大フレーム数は入力ファイルのフレーム数以上か\n・出力プラグインの設定は適切か\n・優先度設定は適切か", "確認事項", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                {
                    return;
                }
            }
            else
            {
                if (MessageBox.Show($"AviUtlの環境設定を確認し、OKをクリックしてください。\n\n・出力プラグインの設定は適切か\n・優先度設定は適切か", "確認事項", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                {
                    return;
                }
            }

            await Task.Run(() =>
            {
                IsRunning = true;
                string WndNum = RUN_COMMAND("auc_findwnd").Trim();
                if (WndNum == "0")
                {
                    if (Settings.AviUtl_Path != "")
                    {
                        Dispatcher.Invoke(() =>
                        {
                            OutputLog("AviUtlを起動中です。");
                            status.Text = "AviUtlの起動中";
                        });
                        WndNum = RUN_COMMAND($"auc_exec {Settings.AviUtl_Path}").Trim();
                    }
                    else
                    {
                        MessageBox.Show("AviUtlのパスを入力してください。", "AviUtl Encoder", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    Activate();
                    SwitchState(false);
                });
                
                if (Settings.Do_File_Analize)
                {
                    Dispatcher.Invoke(() =>
                    {
                        status.Text = "動画の確認中";
                    });
                    var hasError = false;
                    for (int i = 0; i < fileList.Count; i++)
                    {
                        if (!IsRunning)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                CancelEncoding();
                            });
                            return;
                        }
                        try
                        {
                            var mediaInfo = GetMediaInfo(fileList[i]);

                            var iniPath = Settings.AviUtl_Path.Remove(Settings.AviUtl_Path.LastIndexOf('\\')) + "\\aviutl.ini";
                            var isWidthOK = int.Parse(GetIniValue(iniPath, "system", "width")) >= mediaInfo.Width;
                            var isHeightOK = int.Parse(GetIniValue(iniPath, "system", "height")) >= mediaInfo.Height;
                            var isFrameOK = int.Parse(GetIniValue(iniPath, "system", "frame")) >= mediaInfo.FrameCount;

                            if (isWidthOK && isHeightOK && isFrameOK)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    OutputLog($"{fileList[i].Split('\\').Last()} : 検証完了");
                                });
                            }
                            else
                            {
                                hasError = true;
                                Dispatcher.Invoke(() =>
                                {
                                    OutputLog($"{fileList[i]} : AviUtlの設定に不備があります。");
                                });
                                var detail = new StringBuilder();
                                if (!isWidthOK) detail.AppendLine($"「最大画像サイズ」の幅が足りません。( < {mediaInfo.Width})");
                                if (!isHeightOK) detail.AppendLine($"「最大画像サイズ」の高さが足りません。( < {mediaInfo.Height})");
                                if (!isFrameOK) detail.AppendLine($"「最大フレーム数」が足りません。( < {frameCount})");
                                MessageBox.Show($"{fileList[i]}は、現在のAviUtlの設定ではエンコードできません。環境設定を見直し、AviUtlを再起動してください。\n\n{detail.ToString()}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (NullReferenceException)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                OutputLog($"{fileList[i].Split('\\').Last()} : ファイルの分析に失敗しました。動画ファイルではありません。");
                            });
                        }
                        catch (Exception)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                OutputLog($"不明なエラーです。MediaInfoが正しく配置されていない可能性があります。このソフトを再インストールしてください。");
                            });
                        }
                    }

                    if (hasError)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            CancelEncoding();
                        });
                        return;
                    }
                }

                if (!File.Exists(AUI_INDEXER_PATH))
                {
                    Dispatcher.Invoke(() =>
                    {
                        SwitchState(true);
                        MessageBox.Show("aui_indexerが見つかりません。再インストールしてください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    OutputLog("インデックスファイルを事前生成します。");
                    status.Text = "インデックスファイルを生成中";
                });
                
                for (int i = 0; i < fileList.Count; i++)
                {
                    if (!IsRunning)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            CancelEncoding();
                        });
                        return;
                    }
                    try
                    {
                        var proc = new Process();
                        proc.StartInfo.FileName = AUI_INDEXER_PATH;
                        proc.StartInfo.WorkingDirectory = Settings.AviUtl_Path.Remove(Settings.AviUtl_Path.LastIndexOf('\\'));
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.RedirectStandardInput = true;
                        proc.StartInfo.Arguments = "\"" + fileList[i] + "\"";

                        proc.StartInfo.RedirectStandardOutput = true;
                        //proc.StartInfo.RedirectStandardError = true;

                        proc.OutputDataReceived += Proc_OutputDataReceived;

                        proc.Start();
                        proc.BeginOutputReadLine();
                        proc.WaitForExit();
                        proc.Close();
                    }
                    catch (Exception)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            OutputLog($"不明なエラーです。再度お試しください。治らない場合はPCを再起動するか、このソフトを再インストールしてください。");
                        });
                    }
                }

                
                
                WindowNumber = WndNum;
                Dispatcher.Invoke(() =>
                {
                    RunEncodeProcess(WndNum, fileList);
                    GetPercentage();
                });
            });
        }

        private async void Start_Encoding_With_FFmpeg()
        {
            if (this.Files.Count == 0)

            if (MessageBox.Show($"ffmpegでのエンコードを開始しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
            {
                return;
            }

            var destinationDirectory = Destination_Directory_TextBox.Text;
            var extension = Output_Extension_TextBox.Text;
            SwitchState(false);
            fileCount = Files.Count;
            IsRunning = true;
            var forceOverwrite = (bool)ForceOverwriteCheckBox.IsChecked;
            currentNumber = 0;
            await Task.Run(() =>
            {
                var errorCount = 0;
                foreach (var item in Files)
                {

                    errorOccured = false;
                    if (IsRunning == false) break;
                    var fileName = $"{Path.GetFileNameWithoutExtension(item)}{Settings.Suffix}.{extension}";

                    if (!File.Exists(item))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            OutputLog($"[エラー] 指定されたファイル({item})が見つかりません。");
                        });
                        errorCount++;
                        continue;
                    }
                    if (!forceOverwrite && File.Exists(Path.Combine(destinationDirectory, fileName)))
                    {
                        if (MessageBox.Show($"指定されたファイル({fileName})は既に存在します。上書きしますか？", "上書き確認", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                        {
                            continue;
                        }
                    }
                    currentNumber++;
                    Dispatcher.Invoke(() =>
                    {
                        status.Text = $"{Path.GetFileName(item)} を出力中 ({currentNumber}/{Files.Count})";
                        OutputLog($"{Path.GetFileName(item)}の出力開始");
                    });
                    EncodeWithffmpeg(item, Path.Combine(destinationDirectory, fileName));

                    if (errorOccured) errorCount++;
                }

                Dispatcher.Invoke(() =>
                {
                    status.Text = "完了";
                    TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    var flashInfo = new FLASHWINFO();
                    flashInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(flashInfo));
                    flashInfo.dwFlags = FLASHW_TRAY | FLASHW_TIMERNOFG;
                    flashInfo.uCount = uint.MaxValue;
                    flashInfo.dwTimeout = 500;
                    flashInfo.hwnd = new WindowInteropHelper(this).Handle;
                    FlashWindowEx(ref flashInfo);

                    IsRunning = false;
                    SwitchState(true);
                    pb1.Value = 0;
                    progressBar.Value = 0;
                    progressBar.IsIndeterminate = false;
                    progressPercent.Text = "出力の進捗";
                    AllProgressText.Text = "全体の進捗";
                    OutputLog($"処理がすべて完了しました。(成功:{this.Files.Count - errorCount}/{this.Files.Count})");
                    CompleteEncoding();
                });
            });
        }

        Process ffmpegProcess = null;

        private void EncodeWithffmpeg(string targetPath, string toSavePath)
        {
            var proc = new Process();
            ffmpegProcess = proc;
            proc.StartInfo.FileName = FFMPEG_PATH;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;

            string vCodecName = "";
            string aCodecName = "";
            string vBitrate = "";
            string aBitrate = "";
            Dispatcher.Invoke(() =>
            {
                var vCodecId = ((ComboBoxItem)Video_Codec_ComboBox.SelectedItem).Tag;
                vCodecName = vCodecId switch
                {
                    "av1" => "av1 -strict -2",
                    "h264" or "h264_qsv" or "h264_nvenc" => $"{vCodecId} -x264-params log-level=error",
                    "hevc" or "hevc_qsv" or "hevc_nvenc" => $"{vCodecId} -x265-params log-level=error",
                    _ => vCodecId.ToString(),
                };

                aCodecName = ((ComboBoxItem)ACodecComboBox.SelectedItem).Tag switch
                {
                    "vorbis" => aCodecName = "vorbis -strict -2",
                    _ => aCodecName = ((ComboBoxItem)ACodecComboBox.SelectedItem).Tag.ToString()
                };

                if (Video_Bitrate_TextBox.Text != "0")
                {
                    vBitrate = $"-b:v {Video_Bitrate_TextBox.Text}";
                }

                aBitrate = ((ComboBoxItem)ABitrateComboBox.SelectedItem).Tag.ToString();
            });

            var mediaInfo = GetMediaInfo(targetPath);

            videoDuration = mediaInfo.Duration;
            frameCount = mediaInfo.FrameCount ;

            proc.StartInfo.Arguments = $"-i {targetPath} -loglevel error -progress - -codec:v {vCodecName} -codec:a {aCodecName} {vBitrate} -b:a {aBitrate} {toSavePath} -y";

            Dispatcher.Invoke(() =>
            {
                OutputLog("ffmpeg引数:" + proc.StartInfo.Arguments);
            });

            proc.OutputDataReceived += FFMPEG_OutputDataReceived;
            proc.ErrorDataReceived += FFMPEG_ErrorDataReceived;

            proc.Start();

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            proc.WaitForExit();

            proc.Close();
        }

        private void FFMPEG_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            //if (e.Data.Contains("x264[info]") || e.Data.Contains("x265[info]")) return;
            Dispatcher.Invoke(() =>
            {
                OutputLog("[エラー] " + e.Data);
                if (e.Data.Contains("unsupported"))
                {
                    var codecName = ((ComboBoxItem)Video_Codec_ComboBox.SelectedItem).Tag switch
                    {
                        "h264_qsv" or "hevc_qsv" => "QSV",
                        "h264_nvenc" or "hevc_nvenc" => "NVEnc",
                        _ => "?"
                    };

                    OutputLog($"【エラー】お使いの端末では {codecName} がサポートされていません。コーデックの指定をお確かめの上、再度お試しください。");
                }
            });
            errorOccured = true;
        }

        double videoDuration = 0;
        int frameCount = 0;
        int fileCount = 0;

        int frame = 0;

        private void FFMPEG_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && IsRunning)
            {
                Debug.Print(e.Data);
                if (e.Data.Contains("frame="))
                {
                    try
                    {
                        var outputFrames = e.Data.Substring(e.Data.IndexOf("frame=") + 6);
                        frame = int.Parse(outputFrames);
                    }
                    catch (Exception) { }

                }
                if (e.Data.Contains("drop_frames="))
                {
                    try
                    {
                        var dropFrames = e.Data.Substring(e.Data.IndexOf("drop_frames=") + 12);
                        Dispatcher.Invoke(() =>
                        {
                            OutputFrames.Text = $"{frame + int.Parse(dropFrames)}/{frameCount}";
                        });
                    }
                    catch (Exception) { }
                }
                if (e.Data.Contains("out_time="))
                {
                    try
                    {
                        var outTime = e.Data.Substring(e.Data.IndexOf("out_time=") + 9);
                        var outputTimeSpan = TimeSpan.Parse(outTime);
                        var duration = TimeSpan.FromSeconds(videoDuration);
                        var percentage = Math.Round((outputTimeSpan.TotalMilliseconds / duration.TotalMilliseconds) * 100);
                        Dispatcher.Invoke(() =>
                        {
                            pb1.Value = percentage;
                            progressPercent.Text = percentage.ToString() + "%";
                            var allPercentage = Math.Round(percentage / fileCount + (100 / fileCount) * (currentNumber - 1));
                            progressBar.Value = percentage / fileCount + 100 / fileCount * (currentNumber - 1);
                            AllProgressText.Text = allPercentage.ToString() + "%";
                            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                            TaskbarItemInfo.ProgressValue = allPercentage / 100;
                            OutputTime.Text = $"{outTime}";
                        });
                    }
                    catch (Exception) { }
                }
                if (e.Data.Contains("fps="))
                {
                    try
                    {
                        var fps = e.Data.Substring(e.Data.IndexOf("fps=") + 4);
                        Dispatcher.Invoke(() =>
                        {
                            OutputSpeed.Text = $"{fps}Frames/s";
                        });
                    }
                    catch (Exception) { }
                }
            }
        }

        private MediaInfo GetMediaInfo(string filePath)
        {
            var p = new Process();
            p.StartInfo.FileName = MEDIAINFO_PATH;
            p.StartInfo.Arguments = $"\"{filePath}\" --OUTPUT=JSON";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;

            p.Start();
            p.WaitForExit();

            var result = p.StandardOutput.ReadToEnd();

            p.Close();

            var json = JObject.Parse(result);

            var videoTrack = ((JArray)json["media"]["track"]).FirstOrDefault((e) => e["@type"].ToString() == "Video");

            return new MediaInfo()
            {
                Duration = double.Parse(videoTrack["Duration"].ToString()),
                FrameCount = int.Parse(videoTrack["FrameCount"].ToString()),
                Width = int.Parse(videoTrack["Width"].ToString()),
                Height = int.Parse(videoTrack["Height"].ToString())
            };
        }

        private void textBoxPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9のみ
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }
        private void textBoxPrice_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼り付けを許可しない
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private async void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            await Task.Run(() =>
            {
                if (e.Data == null) return;
                if (e.Data.StartsWith("could not find aui file."))
                {
                    Dispatcher.Invoke(() =>
                    {
                        OutputLog("＜エラー＞L-SMASH Works FRの検索に失敗しました。インストールされていない可能性があります。");
                    });
                    return;
                }

                var test = e.Data;

                var data = e.Data.Substring(11).Remove(e.Data.Substring(11).LastIndexOf("..."));
               
                Dispatcher.Invoke(() =>
                {
                    OutputLog($"{data} のインデックスファイルを生成しました。");
                });
            });
        }

        private void ProfileNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            var text = Profile_Number_TextBox.Text + e.Text;
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
                var dialog = new CommonOpenFileDialog("入力フォルダー選択");
                dialog.InitialDirectory = Settings.Input_Folder_Last_Selected;
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    Settings.Input_Folder_Last_Selected = dialog.FileName;
                    Source_Directory_TextBox.Text = dialog.FileName;
                }
            } else
            {
                var dialog = new SelectFilesWindow(Files);
                dialog.ShowDialog();
                if (dialog.changeList)
                {
                    Files = dialog.items;
                }
            }
            
        }

        private void openFile2_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = Settings.Output_Folder_Last_Selected;
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Settings.Output_Folder_Last_Selected = dialog.FileName;
                Destination_Directory_TextBox.Text = dialog.FileName;
            }
        }
        

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Pause_Button.Content.ToString() == "一時停止")
            {
                RUN_COMMAND($"auc_veropen {WindowNumber}");
                status.Text = "出力を一時停止中";
                Pause_Button.Content = "再開";
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Paused);
            }
            else
            {
                RUN_COMMAND($"auc_verclose {WindowNumber}");
                status.Text = $"{CurrentFile.Remove(0, 1)} を出力中・・・({CurrentNum}/{AllFiles})";
                Pause_Button.Content = "一時停止";
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                
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

            foreach (ProfileLabel label in Settings.Profile_Labels)
            {
                Profile_ComboBox.Items.Add(label);
            }
            foreach (PluginLabel label in Settings.Plugin_Labels)
            {
                Plugin_ComboBox.Items.Add(label);
            }

            if (Settings.Profile_Labels.Exists(delegate (ProfileLabel l) { return l.Number.ToString() == Profile_Number_TextBox.Text; }))
            {
                Profile_ComboBox.SelectedItem = Settings.Profile_Labels.Find(delegate (ProfileLabel l) { return l.Number.ToString() == Profile_Number_TextBox.Text; });
            }
            else
            {
                Profile_ComboBox.SelectedIndex = 0;
            }
            if (Settings.Plugin_Labels.Exists(delegate (PluginLabel l) { return l.Number.ToString() == Plugin_Number_TextBox.Text; }))
            {
                Plugin_ComboBox.SelectedItem = Settings.Plugin_Labels.Find(delegate (PluginLabel l) { return l.Number.ToString() == Plugin_Number_TextBox.Text; });
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

        private void Destination_Directory_TextBox_PreviewDragOver(object sender, DragEventArgs e)
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

        private void Destination_Directory_TextBox_Drop(object sender, DragEventArgs e)
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
            if (!Directory.Exists(files[0]))
            {
                MessageBox.Show("ファイルを直接指定することはできません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            sender.Text = files[0];
        }

        private void MenuItem_GetVersion_Click(object sender, RoutedEventArgs e)
        {
            var versionStr = FileVersionInfo.GetVersionInfo(typeof(App).Assembly.Location).FileVersion;
            MessageBox.Show($"現在のバージョン: {versionStr}\n\nアップデートの確認は Microsoft Store から行えます。", "バージョン情報", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Profile_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != 0 && Profile_ComboBox.SelectedItem != null)
            {
                Profile_Number_TextBox.Text = (Profile_ComboBox.SelectedItem as ProfileLabel).Number.ToString();
            }
        }

        private void Plugin_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != 0 && Plugin_ComboBox.SelectedItem != null)
            {
                var labelItem = Plugin_ComboBox.SelectedItem as PluginLabel;
                Plugin_Number_TextBox.Text = labelItem.Number.ToString();
                if (labelItem.Extension != "")
                {
                    Output_Extension_TextBox.Text = labelItem.Extension;
                }
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
            System.Diagnostics.Process.Start("explorer.exe", ConfigurationManager.OpenExeConfiguration(
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
                log.Visibility = Visibility.Hidden;
            }
            else
            {
                cBox.IsChecked = true;
                log.Visibility = Visibility.Visible;
            }
        }

        private void ModeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFormStateForMode();
        }

        private void UpdateFormStateForMode()
        {
            if (!IsInitialized) return;

            Target_Extension_TextBox.Text = extFindFirst;
            Target_Extension_TextBox.IsEnabled = true;
            Source_Directory_TextBox.IsEnabled = true;
            openFile1.Content = "参照";
            Profile_Number_TextBox.IsEnabled = true;
            Profile_ComboBox.IsEnabled = true;
            Plugin_Number_TextBox.IsEnabled = true;
            Plugin_ComboBox.IsEnabled = true;
            Video_Codec_ComboBox.IsEnabled = false;
            ACodecComboBox.IsEnabled = false;
            Video_Bitrate_TextBox.IsEnabled = false;
            ABitrateComboBox.IsEnabled = false;
            ForceOverwriteCheckBox.IsEnabled = false;
            DDText1.Visibility = Visibility.Visible;
            UpdateAfterProcessCheckBoxState();
            UpdateACodecComboBoxStateForVCodec();
            UpdateABitrateComboBoxStateForACodec();

            switch (Mode_ComboBox.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    Target_Extension_TextBox.IsEnabled = false;
                    Source_Directory_TextBox.IsEnabled = false;
                    openFile1.Content = "選択";
                    DDText1.Visibility = Visibility.Hidden;
                    break;
                case 2:
                    extFindFirst = Target_Extension_TextBox.Text;
                    Target_Extension_TextBox.Text = "aup";
                    Target_Extension_TextBox.IsEnabled = false;
                    break;
                case 3:
                    Target_Extension_TextBox.IsEnabled = false;
                    Source_Directory_TextBox.IsEnabled = false;
                    openFile1.Content = "選択";
                    Profile_Number_TextBox.IsEnabled = false;
                    Profile_ComboBox.IsEnabled = false;
                    Plugin_Number_TextBox.IsEnabled = false;
                    Plugin_ComboBox.IsEnabled = false;
                    Video_Codec_ComboBox.IsEnabled = true;
                    Video_Bitrate_TextBox.IsEnabled = true;
                    ACodecComboBox.IsEnabled = true;
                    ForceOverwriteCheckBox.IsEnabled = true;
                    DDText1.Visibility = Visibility.Hidden;
                    break;
                default:
                    break;
            }
        }

        private void ExtFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Mode_ComboBox.SelectedIndex != 2)
            {
                extFindFirst = Target_Extension_TextBox.Text;
            }
        }

        private void AfterProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAfterProcessCheckBoxState();
        }

        private void UpdateAfterProcessCheckBoxState()
        {
            var cBox = Operation_After_Encoded;
            if (cBox == null) return;
            if (cBox.SelectedIndex == 1)
            {
                Quit_AUEnc_CheckBox.IsEnabled = false;
                Quit_AviUtl_CheckBox.IsEnabled = false;
            }
            else if (Quit_AUEnc_CheckBox != null)
            {
                Quit_AUEnc_CheckBox.IsEnabled = true;
                if (Mode_ComboBox.SelectedIndex != 3) Quit_AviUtl_CheckBox.IsEnabled = true;
                else Quit_AviUtl_CheckBox.IsEnabled = false;
            }
        }

        private void PluginNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            var text = Profile_Number_TextBox.Text + e.Text;
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

        private void Interrupt_button_Click(object sender, RoutedEventArgs e)
        {
            if (Mode_ComboBox.SelectedIndex != 3)
            {
                if (MessageBox.Show("中断した後にAviUtl側で「Esc」を押して出力を中断してください。", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    return;
                }
                IsRunning = false;
                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                }
                OutputLog("ユーザー操作によって出力が中断されました。");
                interrupt_button.IsEnabled = false;
                status.Text = "中断待機中。AviUtl側で中断操作をしてください。";
            }
            else
            {
                if (MessageBox.Show("中断しますか？", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    return;
                }
                IsRunning = false;
                if (ffmpegProcess != null)
                {
                    ffmpegProcess.StandardInput.Write('q');
                }
            }
            progressBar.IsIndeterminate = true;
            AllProgressText.Text = "中断中";
            OutputLog("ユーザー操作によって出力が中断されました。");
            OutputLog("中断処理中です。しばらくお待ち下さい。");
        }

        /// <summary>
        /// エンコードのログを出力する。必ずUIスレッドで実行すること。
        /// </summary>
        /// <param name="logString">出力するログ</param>
        private void OutputLog(string logString)
        {
            LogStrBuilder.AppendLine();
            LogStrBuilder.Append(logString);
            var item = new ListBoxItem();
            var textBlock = new TextBlock();
            textBlock.Text = logString;
            textBlock.TextWrapping = TextWrapping.Wrap;
            item.Content = textBlock;
            item.Selected += Item_Selected;
            log.Items.Add(item);
            log.ScrollIntoView(item);
            //log.ScrollToBottom();
        }

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(
            string lpApplicationName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedstring,
            int nSize,
            string lpFileName);

        public string GetIniValue(string path, string section, string key)
        {
            var sb = new StringBuilder(256);
            GetPrivateProfileString(section, key, string.Empty, sb, sb.Capacity, path);
            return sb.ToString();
        }

        private void VCodecComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateACodecComboBoxStateForVCodec();
        }

        private void UpdateACodecComboBoxStateForVCodec()
        {
            if (!IsInitialized) return;

            var videoCodecId = ((ComboBoxItem)Video_Codec_ComboBox.SelectedItem).Tag;

            var availableAudioCodecs = videoCodecId switch
            {
                "utvideo" => new List<string> { "pcm_s16le", "aac", "mp3" },
                "h264" or "hevc" or "h264_qsv" or "hevc_qsv" or "h264_nvenc" or "hevc_nvenc" => new List<string> { "vorbis", "aac", "mp3" },
                "av1" => new List<string> { "pcm_s16le", "aac", "mp3", "vorbis", "flac" },
                "vp9" => new List<string> { "vorbis" },
                _ => new List<string> { }
            };

            var extension = videoCodecId switch
            {
                "utvideo" => "avi",
                "h264" or "hevc" or "h264_qsv" or "hevc_qsv" or "h264_nvenc" or "hevc_nvenc" => "mp4",
                "av1" => "mkv",
                "vp9" => "webm",
                _ => ""
            };
            Output_Extension_TextBox.Text = extension;

            foreach (ComboBoxItem item in ACodecComboBox.Items)
            {
                item.IsEnabled = availableAudioCodecs.Contains(item.Tag);
            }

            ACodecComboBox.SelectedItem = ACodecComboBox.Items.OfType<ComboBoxItem>().First((item) => item.IsEnabled);

            Video_Bitrate_TextBox.IsEnabled = videoCodecId switch
            {
                "utvideo" => false,
                _ => true,
            };
        }

        private void ACodecComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateABitrateComboBoxStateForACodec();
            
        }

        private void UpdateABitrateComboBoxStateForACodec()
        {
            if (!IsInitialized) return;
            Trace.WriteLine(ACodecComboBox.SelectedItem);
            Trace.WriteLine(ACodecComboBox.SelectedValue);
            ABitrateComboBox.IsEnabled = ACodecComboBox.SelectedIndex switch
            {
                0 => false,
                _ => true,
            };
        }
    }

    class MediaInfo
    {
        public double Duration;
        public int FrameCount;
        public int Width;
        public int Height;
    }
}

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
        String aucPathText;
        string windowNumber;
        string currentFile;
        int currentNum;
        int allFiles;
        bool IsRunning;

        public MainWindow()
        {
            InitializeComponent();
            
            profileNumber.Text = Properties.Settings.Default.Profile_Number;
            pluginNumber.Text = Properties.Settings.Default.Plugin_Number;
            extFind.Text = Properties.Settings.Default.Find_Ext;
            extOutput.Text = Properties.Settings.Default.Output_Ext;
        }

        private void Window_Closing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.Profile_Number = profileNumber.Text;
                Properties.Settings.Default.Plugin_Number = pluginNumber.Text;
                Properties.Settings.Default.Find_Ext = extFind.Text;
                Properties.Settings.Default.Output_Ext = extOutput.Text;

                Properties.Settings.Default.Save();
            

            }
        }

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
                p.StartInfo.Arguments = $"/c cd /d {aucPathText} & {cmd}";
                p.Start();
            //}

            

            var result = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();

            p.WaitForExit();
            p.Close();

            return result;
        }

        private bool isExtMatch(string input)
        {
            var index = input.LastIndexOf('.');
            var ext = input.Substring(index);
            return ext == extFind.Text.Insert(0, ".");
        }

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

            openFile1.IsEnabled = to;
            openFile2.IsEnabled = to;
        }

        private async void runEncode(string wndNum, string[] inputFiles)
        {
            progressBar.Maximum = inputFiles.Length + 1;

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
                        currentFile = inputFilePath.Substring(inputFilePath.LastIndexOf('\\'));
                        currentNum = i + 1;
                        allFiles = inputFiles.Length;
                        progress.Text = $"{currentFile} を出力中・・・({currentNum}/{allFiles})";
                        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                        TaskbarManager.Instance.SetProgressValue(i + 1, inputFiles.Length + 1);

                        profNum = profileNumber.Text;
                        plugNum = pluginNumber.Text;
                        forFolder = toFolder.Text;
                        outputExt = extOutput.Text.Insert(0, ".");
                        
                        progressBar.Value = i + 1;
                        pb1.Value = 0;
                        progressPercent.Text = "0%";
                    });

                    Dispatcher.Invoke(() =>
                    {
                        pauseButton.IsEnabled = false;
                        log.Text = log.Text + $"\n{inputFilePath}:ファイルの読み込みを開始";
                    });
                    var res1 = RUN_COMMAND($@"auc_open {wndNum} ""{inputFilePath}""");
                    Thread.Sleep(5000);
                    RUN_COMMAND($"auc_setprof {wndNum} {profNum}");
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(() =>
                    {
                        log.Text = log.Text + $"\n{inputFilePath}:ファイルのエンコード開始";
                    });
                    
                    var li = inputFilePath.LastIndexOf('\\');
                    var inputFileName = inputFilePath.Substring(li);
                    var filename = forFolder + inputFileName.Remove(inputFileName.LastIndexOf('.')) + outputExt;
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
                                log.Text = log.Text + $"\n{inputFilePath}:ファイルの出力に失敗\n<エラー>AUCコマンドが見つかりません。AUCのパスが間違っている可能性があります。";
                            } 
                            else
                            {
                                log.Text = log.Text + $"\n{inputFilePath}:ファイルの出力に失敗";
                            }
                            
                        });
                        failure++;
                        continue;
                    } else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            log.Text = log.Text + $"\n{inputFilePath}:ファイルの出力が完了({currentNum}/{allFiles})";
                        });
                        success++;
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    progress.Text = "完了";
                    switchState(true);
                    progressBar.Value = inputFiles.Length + 1;
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                    log.Text = log.Text + $"\n処理がすべて完了しました。({success}個成功/{failure}個失敗)";
                    RUN_COMMAND($"auc_close {wndNum}");
                    pb1.Value = 100;
                    progressPercent.Text = "100%";
                    IsRunning = false;
                });
                
            });
            
            
        }

        private async void GetPercentage()
        {
            await Task.Run(() =>
            {
                while (IsRunning)
                {
                    foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
                    {
                        if (p.MainWindowTitle.Length != 0 && p.MainWindowTitle.StartsWith("出力中"))
                        {
                            string percent;
                            if (p.MainWindowTitle.Substring(4, 2).EndsWith("%"))
                            {
                                percent = p.MainWindowTitle.Substring(4, 1);
                            }
                            else
                            {
                                percent = p.MainWindowTitle.Substring(4, 2);
                            }
                            Dispatcher.Invoke(() =>
                            {
                                progressPercent.Text = percent + "%";
                                pb1.Value = double.Parse(percent);
                            });
                        }
                    }
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
            
            aucPathText = Properties.Settings.Default.AUC_Path;
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
            windowNumber = wndNum;
            runEncode(wndNum, filteredFiles);
            GetPercentage();
            
        }

        private void profileNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            var text = profileNumber.Text + e.Text;
            e.Handled = regex.IsMatch(text);
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
                RUN_COMMAND($"auc_veropen {windowNumber}");
                pauseButton.Content = "再開";
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Paused);
                progress.Text = "出力を一時停止中";
            } else
            {
                RUN_COMMAND($"auc_verclose {windowNumber}");
                pauseButton.Content = "一時停止";
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                progress.Text = $"{currentFile} を出力中・・・({currentNum}/{allFiles})";
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var PrefWindow = new PreferenceWindow();
            PrefWindow.Show();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad", @"""readme.txt""");
        }

        private void fromFolder_Drop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files.Count() != 1 || files == null)
            {
                MessageBox.Show("複数のフォルダーを指定することはできません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            };
            fromFolder.Text = files[0];
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
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files.Count() != 1 || files == null)
            {
                MessageBox.Show("複数のフォルダーを指定することはできません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            };
            toFolder.Text = files[0];
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            var version = File.ReadAllText("../../Resources/version.json");
            //var text = version.Replace(Environment.NewLine, "");
            dynamic obj = JObject.Parse(version);
            var client = new HttpClient();
            var text = client.GetStringAsync("https://script.google.com/macros/s/AKfycbyOj4z6rWTAUJWLJrOwCbF0g9s3ZoQGPF8xSIMTu3geOmVJ6fA/exec");
            var latestVersionInfo = JObject.Parse(text.Result);
            if (latestVersionInfo["num"] > obj["versionCode"])
            {
                var result = MessageBox.Show($"現在のバージョン:{obj["version"]}\n\nアップデートがあります。最新バージョン:{latestVersionInfo["version"]}\n\n☆アップデート内容\n{latestVersionInfo["content"]}\n\nダウンロードしますか？", "バージョン情報", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(latestVersionInfo["url"].ToString());
                }
            } else
            {
                MessageBox.Show($"現在のバージョン:{obj["version"].ToString()}", "バージョン情報", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}

﻿using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using Label = System.Windows.Forms.Label;
using System.IO.Compression;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;
using System.Linq;
using FileMode = System.IO.FileMode;
using System.Threading;

namespace SimpleLauncher
{

    partial class Launcher
    {
       
        private System.ComponentModel.IContainer components = null;
        private static ProgressBar _ProgressBar = new ProgressBar();
        static string InstallPath = "C:\\INSTALL TO ME";
        static string _RepoUrl = "https://github.com/MeduzaEd/Game_LoadfromLauncher";
        #region _Labels
        static Label _PathLabel = new Label();
        #endregion

        #region Buttons
        Button _SelectPathButton = new Button();
        Button _InstallToPathButton = new Button();
        #endregion



        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Init
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
           // this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 600);   

            this.Text = "Simple-Launcher";

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; // Запрещает максимизацию
            this.MinimizeBox = false; // Запрещает минимизацию
            LablesInit();
            ButtonsInit();
        }
        public void InitVars(string Path)
        {
           
        }
        #endregion

        #region Logic

        #region Select Folder
        private void SelectInstallPath(object sender, EventArgs e)
        {
            FolderBrowserDialog _Dialog = new FolderBrowserDialog();
            
            _Dialog.Description = "Select Installing Folder";
            if (_Dialog.ShowDialog() == DialogResult.OK)
            {
                InstallPath = (_Dialog.SelectedPath).ToString();
                _PathLabel.Text = $"Install Path:{InstallPath}";
               
            }
        }
        #endregion
        #region Install
        private async void Install(object sender, EventArgs e)
        {
            _ProgressBar.Value = 0;
            await Task.WhenAll(DownloadRepositoryFiles("/"));
             _PathLabel.Text = "Готово.";
            _ProgressBar.Maximum =1;
             _ProgressBar.Value = 1;
        }
        #endregion

        static async Task DownloadRepositoryFiles(string path)
        {
            try
            {
                var uri = new Uri(_RepoUrl);
                var user = uri.Segments[1].TrimEnd('/');
                var repo = uri.Segments[2].TrimEnd('/');

                var githubClient = new GitHubClient(new ProductHeaderValue("LauncherClient"));
                githubClient.Credentials = new Credentials("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");
                var repoContent = await githubClient.Repository.Content.GetAllContents(user, repo, path);

                _ProgressBar.Maximum = repoContent.Count(); // Устанавливаем максимальное значение прогресс-бара

                var tasks = repoContent
                    .Select(item =>
                    {
                        if (item.Type == ContentType.File)
                        {
                            return DownloadFile(item.DownloadUrl, item.Path,_ProgressBar, _PathLabel);
                        }
                        else if (item.Type == ContentType.Dir)
                        {
                            return DownloadRepositoryFiles(item.Path);
                        }

                        return Task.CompletedTask;
                    });

                await Task.WhenAll(tasks);

                _PathLabel.Text = "Успешно.";
               
            }
            catch (Exception ex)
            {
                //_ProgressBar.Visible = false;
                _PathLabel.Text = $"Ошибка: {ex.Message}";
            }
        }


        static async Task DownloadFile(string downloadUrl, string filePath, ProgressBar progressBar, Label pathLabel)
        {
            try
            {
                var fullFilePath = Path.Combine(InstallPath, filePath.Replace('/', '\\'));

                // Проверка, существует ли файл
                if (File.Exists(fullFilePath))
                {
                    pathLabel.Text = $"Уже существует: {fullFilePath}";
                    return;
                }

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        pathLabel.Text = $"Установка: {fullFilePath}";

                        if (!Directory.Exists(Path.GetDirectoryName(fullFilePath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fullFilePath));
                        }

                        var totalBytes = response.Content.Headers.ContentLength ?? -1;

                        using (var fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                        {
                            using (var contentStream = await response.Content.ReadAsStreamAsync())
                            {
                                var buffer = new byte[4096];
                                var isMoreToRead = true;
                                var totalRead = 0L;

                                do
                                {
                                    var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                                    if (read == 0)
                                    {
                                        isMoreToRead = false;
                                    }
                                    else
                                    {
                                        await fileStream.WriteAsync(buffer, 0, read);
                                        totalRead += read;
                                        UpdateProgressBar(progressBar, totalRead, totalBytes);
                                    }
                                } while (isMoreToRead);
                            }
                        }

                        pathLabel.Text = $"Успешно Установлен: {fullFilePath}";
                    }
                }
            }
            catch (Exception ex)
            {
                pathLabel.Text = $"Ошибка {filePath}: {ex.Message}";
            }
        }

        static void UpdateProgressBar(ProgressBar progressBar, long bytesRead, long totalBytes)
        {
            if (totalBytes > 0)
            {
                var percentage = (int)((bytesRead * 100) / totalBytes);
                progressBar.Value = percentage;
            }
        }


        static async Task WriteAllBytesAsync(string path, byte[] bytes)
        {
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await fileStream.WriteAsync(bytes, 0, bytes.Length);
            }
        }
   

        #endregion

        #region Labels
        private void LablesInit()
        {

            _PathLabel.Text = $"Install Path:{InstallPath}";
            _PathLabel.BackColor = Color.Aquamarine;
            _PathLabel.ForeColor = System.Drawing.Color.DarkGray;
            _PathLabel.Font = new System.Drawing.Font("Calibri", 12);
            _PathLabel.AutoSize = true;
            _PathLabel.Location = new System.Drawing.Point(215, 560);
            Controls.Add(_PathLabel);
            #region Progress Bar

            _ProgressBar.Location = new Point(210, 550);
            _ProgressBar.Size = new Size(600, 40);
            _ProgressBar.ForeColor = Color.DarkCyan;
            _ProgressBar.Minimum = 0;
            _ProgressBar.Maximum = 100;
            _ProgressBar.Value=100;
            _ProgressBar.Step = 1;
            //_ProgressBar.Visible = false;
            Controls.Add(_ProgressBar);
            #endregion
        }

        #endregion

        #region UI
        private void ButtonsInit()
        {
            #region Select Path
            _SelectPathButton.Location = new Point(5, 545);
            _SelectPathButton.Font= new System.Drawing.Font("Calibri", 14);
            _SelectPathButton.BackColor = Color.AliceBlue;
            _SelectPathButton.ForeColor = Color.DarkCyan;
            _SelectPathButton.Name = "ExitButton";
            _SelectPathButton.Size = new Size(200, 50);
            _SelectPathButton.TabIndex = 3;
            _SelectPathButton.Text = "Change Path";
            _SelectPathButton.UseVisualStyleBackColor = true;
            _SelectPathButton.Click += new EventHandler(SelectInstallPath);
            Controls.Add(_SelectPathButton);
            #endregion

            #region Install 

            _InstallToPathButton.Location = new Point(816, 545);
            _InstallToPathButton.Font = new System.Drawing.Font("Calibri", 14);
            _InstallToPathButton.BackColor = Color.AliceBlue;
            _InstallToPathButton.ForeColor = Color.DarkCyan;
            _InstallToPathButton.Name = "ExitButton";
            _InstallToPathButton.Size = new Size(200, 50);
            _InstallToPathButton.TabIndex = 3;
            _InstallToPathButton.Text = "Install";
            _InstallToPathButton.UseVisualStyleBackColor = true;
            _InstallToPathButton.Click += new EventHandler(Install);
            Controls.Add(_InstallToPathButton);
            #endregion
        }


        #endregion
    }
    
    
}


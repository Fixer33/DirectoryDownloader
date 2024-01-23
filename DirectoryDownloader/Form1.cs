using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DirectoryDownloader
{
    public partial class Main : Form
    {
        private const string URL = "https://fixer33.github.io/DirectoryDownloader/mcd.json";
        private const string REG_KEY = "last_selected_folder";
        private readonly CommonOpenFileDialog _openFolderDialog = new CommonOpenFileDialog();

        private string _dirPath;

        public Main()
        {
            InitializeComponent();
        }

        private async void StartBtn_Click(object sender, EventArgs e)
        {
            StartBtn.Enabled = false;

            if (Directory.Exists(_dirPath))
            {
                await Proceed(_dirPath);
            }
            else
            {
                MessageBox.Show("Directory does not exist");
            }

            StartBtn.Enabled = true;
        }

        private async Task Proceed(string path)
        {
            HttpClient httpClient = new HttpClient();
            string html = await httpClient.GetStringAsync(URL);

            DirectoryContent content = JsonConvert.DeserializeObject<DirectoryContent>(html);

            string filePath;
            for (int i = 0; i < content.ToDelete.Count; i++)
            {
                filePath = Path.Combine(path, content.ToDelete[i].NameToDelete);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            List<string> failed = new List<string>();
            byte[] bytes = null;
            using (var client = new WebClient())
            {
                allFilesPB.Maximum = content.ToDownload.Count;
                for (int i = 0; i < content.ToDownload.Count; i++)
                {
                    allFilesPB.Value = i;

                    filePath = Path.Combine(path, content.ToDownload[i].NameToSet);

                    if (File.Exists(filePath))
                    {
                        if (content.ToDownload[i].Sum.Equals(Hash(filePath, ref bytes)))
                        {
                            continue;
                        }
                        File.Delete(filePath);
                    }

                    client.DownloadFile(content.ToDownload[i].Url, filePath);

                    await Task.Delay(100);

                    allFilesPB.Value = i + 1;

                    if (content.ToDownload[i].Sum.Equals(Hash(filePath, ref bytes)) == false)
                    {
                        failed.Add(content.ToDownload[i].NameToSet);
                    }
                }
            }
            allFilesPB.Value = allFilesPB.Maximum;

            string conc = "";
            for (int i = 0; i < failed.Count; i++)
            {
                conc += failed[i] + "\n";
            }

            if (failed.Count > 0)
                MessageBox.Show("Files failed to validate hash:\n" + conc);

            MessageBox.Show("Done. You can close the window");
        }

        private static string Hash(string path, ref byte[] bytes)
        {
            bytes = File.ReadAllBytes(path);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashValue = sha256.ComputeHash(bytes);

                string res = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
                return res;
            }
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            _dirPath = Registry.CurrentUser.GetValue(REG_KEY, "") as string;
            StartBtn.Enabled = Directory.Exists(_dirPath);
            if (_dirPath == null)
            {
                _dirPath = "";
            }
            textBox1.Text = _dirPath;
        }

        #region Moving form
        private bool _isMoving;
        private Point _lastLocation;

        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            _isMoving = true;
            _lastLocation = e.Location;
        }

        private void Main_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMoving)
            {
                this.Location = new Point
                    ( (this.Location.X - _lastLocation.X) + e.X, (this.Location.Y - _lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void Main_MouseUp(object sender, MouseEventArgs e)
        {
            _isMoving = false;
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            _openFolderDialog.DefaultDirectory = _dirPath;
            _openFolderDialog.IsFolderPicker = true;
            if (_openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (Directory.Exists(_openFolderDialog.FileName))
                {
                    _dirPath = _openFolderDialog.FileName;
                    textBox1.Text = _dirPath;
                    Registry.CurrentUser.SetValue(REG_KEY, _dirPath);
                    StartBtn.Enabled = true;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

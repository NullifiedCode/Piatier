using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Newtonsoft.Json;
using Piatier.Utils;
using Piatier.Formatting;
using System.IO;
using static System.Windows.Forms.LinkLabel;
using System.Diagnostics;

namespace Piatier
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            guna2TextBox1.AcceptsReturn = true;
            if (!Directory.Exists("./piatier-cache"))
            {
                Directory.CreateDirectory("./piatier-cache");
                richTextBox3.AppendText(LogUtils.FormatLog("Creating cache directory."));
            }
        }


        private static List<Torrent> Torrents = new List<Torrent>();
        private static List<string> Trackers = new List<string>();
        public static int indexCounter = 0;

        private void DoSearch()
        {
            Torrents.Clear();
            Column1.DataGridView.Rows.Clear();
            guna2Button1.Enabled = false;
            guna2Button3.Enabled = false;

            #region funny search

            new Thread(() =>
            {
                if (guna2ToggleSwitch3.Checked)
                {
                    var tor = Sources.PirateBay.GetTorrents(guna2TextBox1.Text);
                    if (tor.Count() < 0) return; 
                    foreach (var t in tor)
                        Torrents.Add(t);
                }
                if (guna2ToggleSwitch4.Checked)
                {
                    var tor = Sources._1337x.GetTorrents(guna2TextBox1.Text);
                    if (tor.Count() < 0) return;
                    foreach (var t in tor)
                        Torrents.Add(t);
                }
                if (guna2ToggleSwitch5.Checked)
                {
                    var tor = Sources.Kickass.GetTorrents(guna2TextBox1.Text);
                    if (tor.Count() < 0) return;
                    foreach (var t in tor)
                        Torrents.Add(t);
                }

                this.Invoke(new Action(() =>
                {
                    Sort();
                    guna2Button1.Enabled = true;
                    guna2Button3.Enabled = true;
                    guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                    guna2MessageDialog1.Show("Done loading ;)", "Information");
                    richTextBox3.AppendText(LogUtils.FormatLog("Done loading results for \"" + guna2TextBox1.Text + "\""));

                    if (!Directory.Exists("./piatier-cache"))
                        Directory.CreateDirectory("./piatier-cache");

                    var fileName = guna2TextBox1.Text.ToLower();
                    fileName = fileName.Replace(" ", "-");
                    fileName = fileName.Replace(":", "");
                    fileName = fileName.Replace(";", "");
                    fileName = fileName.Replace(",", "");
                    fileName = fileName.Replace("/", "");
                    fileName = fileName.Replace("\'", "");
                    fileName = fileName.Replace("\"", "");

                    if (!File.Exists($"./piatier-cache/{fileName}.json"))
                    {
                        richTextBox3.AppendText(LogUtils.FormatLog("Saving cache for result \"" + guna2TextBox1.Text + "\""));
                        File.WriteAllText($"./piatier-cache/{fileName}.json", JsonConvert.SerializeObject(Torrents, Newtonsoft.Json.Formatting.Indented));
                        guna2TextBox1.Enabled = true;
                    }
                    else
                    {
                        if (guna2ToggleSwitch2.Checked) 
                        {
                            richTextBox3.AppendText(LogUtils.FormatLog("Updating cache for result \"" + guna2TextBox1.Text + "\""));
                            File.WriteAllText($"./piatier-cache/{fileName}.json", JsonConvert.SerializeObject(Torrents, Newtonsoft.Json.Formatting.Indented));
                            guna2TextBox1.Enabled = true;
                        }
                        else
                        {
                            var result = guna2MessageDialog2.Show("Old cache found. Do you wanna update the cache?", "Information");
                            if (result == DialogResult.Yes)
                            {
                                richTextBox3.AppendText(LogUtils.FormatLog("Updating cache for result \""+guna2TextBox1.Text+"\""));
                                File.WriteAllText($"./piatier-cache/{fileName}.json", JsonConvert.SerializeObject(Torrents, Newtonsoft.Json.Formatting.Indented));
                                guna2TextBox1.Enabled = true;
                            }
                        }
                    }
                   
                }));
            }).Start();
            #endregion

            Torrents.Clear();
            guna2MessageDialog1.Icon = MessageDialogIcon.Information;
            guna2MessageDialog1.Show("Loading torrents... Please be patient..", "Information");
            richTextBox3.AppendText(LogUtils.FormatLog("Loading results for \"" + guna2TextBox1.Text+"\""));
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(guna2TextBox1.Text))
            {
                guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                guna2MessageDialog1.Show("Search arg is null or nothing. Please enter something.", "Error");
                richTextBox3.AppendText(LogUtils.FormatLog("I dont think you can search nothing."));
                return;
            }


            if(!guna2ToggleSwitch5.Checked && !guna2ToggleSwitch4.Checked && !guna2ToggleSwitch3.Checked)
            {
                guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                guna2MessageDialog1.Show("I dropped the damn website.... uh oh...", "Error");
                richTextBox3.AppendText(LogUtils.FormatLog("No sources checked. No actions taken"));
                return;
            }

            guna2TextBox1.Enabled = false;

            if (!Directory.Exists("./piatier-cache"))
                Directory.CreateDirectory("./piatier-cache");


            var fileName = guna2TextBox1.Text.ToLower();
            fileName = fileName.Replace(" ", "-");
            fileName = fileName.Replace(":", "");
            fileName = fileName.Replace(";", "");
            fileName = fileName.Replace(",", "");
            fileName = fileName.Replace("/", "");
            fileName = fileName.Replace("\'", "");
            fileName = fileName.Replace("\"", "");

            if (File.Exists($"./piatier-cache/{fileName}.json") && !guna2ToggleSwitch1.Checked)
            {
                var result = guna2MessageDialog2.Show("Old cache for search term found. Do you want to load the old results?", "Information");
                if (result == DialogResult.Yes)
                {
                    richTextBox3.AppendText(LogUtils.FormatLog("Loading old cache for \""+guna2TextBox1.Text+"\""));
                    try
                    {
                        Torrents = JsonConvert.DeserializeObject<List<Torrent>>(File.ReadAllText($"./piatier-cache/{fileName}.json"));

                        Sort();
                        guna2Button1.Enabled = true;
                        guna2Button3.Enabled = true;
                        guna2TextBox1.Enabled = true;
                        guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                        guna2MessageDialog1.Show("Done loading ;)", "Information");
                        richTextBox3.AppendText(LogUtils.FormatLog("Done loading cache for \"" + guna2TextBox1.Text + "\""));
                    }
                    catch
                    {
                        guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                        guna2MessageDialog1.Show("Cannot load cache for current search result. ", "Error");
                        richTextBox3.AppendText(LogUtils.FormatLog("Cannot load cache for \""+guna2TextBox1.Text+"\""));
                        guna2TextBox1.Enabled = true;
                    }
                }
                else
                    DoSearch();
            }
            else
            {
                DoSearch();
            }
        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int val = (int)Column1.DataGridView.Rows[e.RowIndex].Cells[0].Value;
                var find = Torrents.Where(x => x.id == val).Count() > 1;
                if (find)
                {
                    var j = Torrents.Where(x => x.id == val).First();
                    guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                    guna2MessageDialog1.Show("Multiple index numbers found. ("+j.id+") please find the other one and delete it.", "Error");
                }
                else
                {
                    var j = Torrents.Where(x => x.id == val).First();
                    Clipboard.SetText(j.link);
                    guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                    guna2MessageDialog1.Show("Copied torrent to clipboard.", "Information");
                }
            }
            catch
            {

                if (string.IsNullOrEmpty(e.RowIndex.ToString()))
                {
                    guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                    guna2MessageDialog1.Show("Failed copying torrent link.", "Error");
                }
            }
        }

        private void Sort()
        {
            List<string> g = new List<string>();
            Column1.DataGridView.Rows.Clear();
            Torrents.Sort((c, y) => int.Parse(c.seeders) - int.Parse(y.seeders));
            Torrents.Reverse();
            foreach (Torrent tor in Torrents)
            {
                var data = $"{tor.category}|{tor.name}|{tor.size}|{tor.seeders}";
               
                if (!g.Contains(data))
                {
                    Column1.DataGridView.Rows.Add(tor.id, tor.source, tor.category, tor.name, tor.size, int.Parse(tor.seeders));
                    g.Add(data);
                }
            }
            g.Clear();
        }
            
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            Sort();
        }

        private void guna2TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
                guna2Button1_Click(sender, e);
        }

        private void guna2TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            Torrents.Clear();
            Column1.DataGridView.Rows.Clear();
            guna2Button3.Enabled = false;
        }



        private void GetTrackers()
        {
            Trackers.Clear();
            richTextBox1.Text = "";
            guna2MessageDialog1.Icon = MessageDialogIcon.Information;
            guna2MessageDialog1.Show("Scraping for Trackers... Please wait.", "Information");
            richTextBox3.AppendText(LogUtils.FormatLog("Scraping for Trackers... Please wait."));
            var trackers = richTextBox2.Lines; // Dumb fix haha
            new Thread(() =>
            {
                var t = Sources.Trackers.GetTrackers(trackers);

                foreach (var track in t)
                    Trackers.Add(track);

                Trackers = Trackers.Distinct().ToList();

                this.Invoke(new Action(() =>
                {
                  
                    if (Trackers.Count > 0)
                    {
                        guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                        guna2MessageDialog1.Show("Found " + Trackers.Count() + " trackers.", "Information");
                        richTextBox1.Lines = Trackers.ToArray();
                        richTextBox3.AppendText(LogUtils.FormatLog("Found " + Trackers.Count() + " trackers."));
                    }
                    else
                    {
                        guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                        guna2MessageDialog1.Show("Sadly, I didnt find any trackers.", "Error");
                        richTextBox3.AppendText(LogUtils.FormatLog("Sadly, I didnt find any trackers."));
                    }
                    guna2Button2.Enabled = true;
                }));
            }).Start();

        }

        private void guna2Button2_Click_1(object sender, EventArgs e)
        {
            guna2Button2.Enabled = false;
            GetTrackers();
        }

        private void guna2TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2TabControl1.SelectedTab.Name == "tabPage1")
                guna2Button3.Visible = true;
            else
                guna2Button3.Visible = false;

            if (guna2TabControl1.SelectedTab.Name == "tabPage3")
                guna2Button4.Visible = true;
            else
                guna2Button4.Visible = false;

        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/NullifiedCode/Piatier");
            guna2MessageDialog1.Icon = MessageDialogIcon.Warning;
            guna2MessageDialog1.Show("You found the easter-egg :) <3", "hi");
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            richTextBox3.AppendText(LogUtils.FormatLog("Attempting to parse old cache files to new format."));
            guna2Button4.Enabled = false;
            new Thread(() =>
            {
                if (Directory.Exists("./piatier-cache"))
                {
                    foreach (var file in Directory.GetFiles("./piatier-cache"))
                    {
                        try
                        {
                            var h = File.ReadAllText(file);
                            List<Torrent> j = JsonConvert.DeserializeObject<List<Torrent>>(h);
                            File.WriteAllText($"{file}", JsonConvert.SerializeObject(j, Newtonsoft.Json.Formatting.Indented));
                        }
                        catch
                        {
                            LogUtils.FormatLog("Failed formatting file " + file);
                        }
                    }
                }

                Thread.Sleep(2000);
                this.Invoke(new Action(() =>
                {
                    guna2Button4.Enabled = true;
                }));
            }).Start();
            guna2MessageDialog1.Icon = MessageDialogIcon.Information;
            guna2MessageDialog1.Show("Attempting to parse old cache files to new format.", "Information");
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            richTextBox3.Text = "";
        }
    }
}

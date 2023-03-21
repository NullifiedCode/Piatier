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
        private static bool isGettingMagnet = false;

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            guna2TabControl1.Enabled = false;
            if (string.IsNullOrEmpty(guna2TextBox1.Text))
            {
                guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                guna2MessageDialog1.Show("Search arg is null or nothing. Please enter something.", "Error");
                richTextBox3.AppendText(LogUtils.FormatLog("I dont think you can search nothing."));
                return;
            }

            if(!guna2ToggleSwitch5.Checked && !guna2ToggleSwitch4.Checked && !guna2ToggleSwitch3.Checked && !guna2ToggleSwitch6.Checked)
            {
                guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                guna2MessageDialog1.Show("I dropped the damn website.... uh oh...", "Error");
                richTextBox3.AppendText(LogUtils.FormatLog("No sources checked. No actions taken"));
                return;
            }

            guna2TextBox1.Enabled = false;

            if (!Directory.Exists("./piatier-cache"))
                Directory.CreateDirectory("./piatier-cache");

            var fileName = CleanName(guna2TextBox1.Text.ToLower());

            if (File.Exists($"./piatier-cache/{fileName}.json") && !guna2ToggleSwitch1.Checked)
            {
                var result = guna2MessageDialog2.Show("Old cache for search term found. Do you want to load the old results?", "Information");
                if (result == DialogResult.Yes)
                {
                    richTextBox3.AppendText(LogUtils.FormatLog("Loading old cache for \""+guna2TextBox1.Text+"\""));
                    try
                    {
                        Torrents = CacheUtils.GetCache(fileName, 0);

                        Sort();
                        guna2Button1.Enabled = true;
                        guna2Button3.Enabled = true;
                        guna2TextBox1.Enabled = true;
                        guna2TabControl1.Enabled = true;
                        guna2MessageDialog1.Icon = MessageDialogIcon.Information;

                        if (Column1.DataGridView.Rows.Count < 1)
                        {
                            guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                            guna2MessageDialog1.Show("If you cannot see any torrents. Please go and disable or lower your min seeders.", "Information");
                        }

                        guna2MessageDialog1.Show("Done loading ;)", "Information");
                        richTextBox3.AppendText(LogUtils.FormatLog("Done loading cache for \"" + guna2TextBox1.Text + "\""));
                    }
                    catch
                    {
                        guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                        guna2MessageDialog1.Show("Cannot load cache for current search result. ", "Error");
                        richTextBox3.AppendText(LogUtils.FormatLog("Cannot load cache for \""+guna2TextBox1.Text+"\""));
                        guna2TextBox1.Enabled = true;
                        guna2TabControl1.Enabled = true;
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
            if (isGettingMagnet) return;
            isGettingMagnet = true;
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

                    if(j.source == "PirateBay")
                    {
                        Clipboard.SetText(j.link);
                        guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                        guna2MessageDialog1.Show("Copied link to clipboard.", "Information");
                        isGettingMagnet = false;
                        return;
                    }

                    guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                    guna2MessageDialog1.Show("Attempting to grab magnet", "Information");

                    new Thread(() =>
                    {
                        var foundMagnet = false;
                        var magnet = "";
                        var h = HTTPUtils.Get(j.link);
                        if (h != null)
                        {
                            if (h.IsOK)
                            {
                                var data = h.ToString().Split('\n');

                                switch (j.source)
                                {
                                    case "1337x":
                                        for (int line = 0; line < data.Length; line++)
                                        {
                                                if (data[line].Contains("href=\"magnet"))
                                                {
                                                    if (!foundMagnet)
                                                    {
                                                        magnet = Regex.Match(data[line], "magnet:[?=a-zA-Z\\-:0-9&;%.+]+").ToString();
                                                        foundMagnet = true;
                                                    }
                                                }
                                        }
                                        break;
                                    case "Kickass":
                                        for (int line = 0; line < data.Length; line++)
                                        {
                                            if (data[line].Contains("siteButton giantButton"))
                                            {
                                                if (!foundMagnet)
                                                {
                                                    magnet = Regex.Match(data[line], "magnet:[?=a-zA-Z\\-:0-9&;%.+]+").ToString();
                                                    foundMagnet = true;
                                                }
                                            }
                                        }
                                        break;
                                    case "YTS":
                                        if (!foundMagnet)
                                        {
                                            magnet = j.link;
                                            foundMagnet = true;
                                        }
                                        break;
                                }

                                this.Invoke(new Action(() =>
                                {
                                    // Fail over if magnet is empty
                                    if (string.IsNullOrEmpty(magnet))
                                    {
                                        Clipboard.SetText(j.link);
                                        guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                                        guna2MessageDialog1.Show("Failed finding magnet, Copied link to clipboard instead", "Error");
                                        isGettingMagnet = false;
                                        return;
                                    }

                                    Clipboard.SetText(magnet);
                                    guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                                    guna2MessageDialog1.Show("Copied magnet to clipboard.", "Information");
                                    isGettingMagnet = false;
                                }));
                            }
                        }
                        else
                        {
                            // Fail over if request is null.
                            this.Invoke(new Action(() =>
                            {
                                Clipboard.SetText(j.link);
                                guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                                guna2MessageDialog1.Show("Copied link to clipboard.", "Information");
                                isGettingMagnet = false;
                            }));
                        }

                    }).Start();
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

        private void guna2TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
                guna2Button1_Click(sender, e);
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            Torrents.Clear();
            Column1.DataGridView.Rows.Clear();
            guna2Button3.Enabled = false;
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
        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/NullifiedCode/Piatier");
            guna2MessageDialog1.Icon = MessageDialogIcon.Warning;
            guna2MessageDialog1.Show("You found the easter-egg :) <3", "hi");
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            richTextBox3.Text = "";
        }

        private void guna2NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if(guna2NumericUpDown1.Value == 0)
            {
                guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                guna2MessageDialog1.Show("Seeders cannot be zero. Resetting to 1", "Information");
                guna2NumericUpDown1.Value = 1;
            }
            else
            {
                Sort();
            }
        }

        private void guna2HtmlLabel8_Click(object sender, EventArgs e)
        {
            Process.Start("https://mullvad.net/en/");
        }

        private void guna2HtmlLabel9_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/c0re100/qBittorrent-Enhanced-Edition");
        }

        private void guna2HtmlLabel10_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.proxifier.com/");
        }

        private void guna2HtmlLabel11_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.henrypp.org/product/simplewall");
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            Sort();
        }

        private void guna2ToggleSwitch8_CheckedChanged(object sender, EventArgs e)
        {
            Sort();
        }

        private void guna2ToggleSwitch4_CheckedChanged(object sender, EventArgs e)
        {
            Sort();
        }

        private void guna2ToggleSwitch3_CheckedChanged(object sender, EventArgs e)
        {
            Sort();
        }

        private void guna2ToggleSwitch5_CheckedChanged(object sender, EventArgs e)
        {
            Sort();
        }

        private void guna2ToggleSwitch6_CheckedChanged(object sender, EventArgs e)
        {
            Sort();
        }
        private string CleanName(string text)
        {
            var fileName = text.ToLower();
            fileName = fileName.Replace(" ", "-");
            fileName = fileName.Replace(":", "");
            fileName = fileName.Replace(";", "");
            fileName = fileName.Replace(",", "");
            fileName = fileName.Replace("/", "");
            fileName = fileName.Replace("\'", "");
            fileName = fileName.Replace("\\", "");
            fileName = fileName.Replace("\"", "");
            fileName = fileName.Replace("\0", "");
            return fileName;
        }

        private void DoSearch()
        {
            Torrents.Clear();
            Column1.DataGridView.Rows.Clear();
            guna2Button1.Enabled = false;
            guna2Button3.Enabled = false;

            #region funny search

            new Thread(() =>
            {
                var tor = new List<Torrent>();
                tor = Sources.PirateBay.GetTorrents(guna2TextBox1.Text);
                if (tor.Count() < 0) return;
                foreach (var t in tor)
                    Torrents.Add(t);

                tor = Sources._1337x.GetTorrents(guna2TextBox1.Text);
                if (tor.Count() < 0) return;
                foreach (var t in tor)
                    Torrents.Add(t);

                tor = Sources.Kickass.GetTorrents(guna2TextBox1.Text);
                if (tor.Count() < 0) return;
                foreach (var t in tor)
                    Torrents.Add(t);

                tor = Sources.YTS.GetTorrents(guna2TextBox1.Text);
                if (tor.Count() < 0) return;
                foreach (var t in tor)
                    Torrents.Add(t);

                this.Invoke(new Action(() =>
                {
                    
                    if (Torrents.Count() > 0)
                    {
                        Sort();
                        guna2Button1.Enabled = true;
                        guna2Button3.Enabled = true;

                        if (!Directory.Exists("./piatier-cache"))
                            Directory.CreateDirectory("./piatier-cache");

                        var fileName = CleanName(guna2TextBox1.Text.ToLower());


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
                                    richTextBox3.AppendText(LogUtils.FormatLog("Updating cache for result \"" + guna2TextBox1.Text + "\""));
                                    File.WriteAllText($"./piatier-cache/{fileName}.json", JsonConvert.SerializeObject(Torrents, Newtonsoft.Json.Formatting.Indented));
                                    guna2TextBox1.Enabled = true;
                                }
                            }
                        }

                        if(Column1.DataGridView.Rows.Count < 1)
                        {
                            guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                            guna2MessageDialog1.Show("If you cannot see any torrents. Please go and disable or lower your min seeders.", "Information");
                        }
                        guna2TabControl1.Enabled = true;
                        guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                        guna2MessageDialog1.Show("Done loading ;)", "Information");
                        richTextBox3.AppendText(LogUtils.FormatLog("Done loading results for \"" + guna2TextBox1.Text + "\""));
                    }
                    else
                    {
                        guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                        guna2MessageDialog1.Show("No torrrents found.", "Error");
                        guna2TextBox1.Enabled = true;
                        guna2Button1.Enabled = true;
                        guna2Button3.Enabled = true;
                        guna2TabControl1.Enabled = true;
                    }
                }));
            }).Start();
            #endregion

            Torrents.Clear();
            guna2MessageDialog1.Icon = MessageDialogIcon.Information;
            guna2MessageDialog1.Show("Loading torrents... Please be patient..", "Information");
            richTextBox3.AppendText(LogUtils.FormatLog("Loading results for \"" + guna2TextBox1.Text + "\""));
        }

        private void GetTrackers()
        {
            Trackers.Clear();
            richTextBox1.Text = "";
            List<string> trackers = richTextBox2.Lines.ToList(); // Dumb fix haha
            if (Torrents.Count() > 0)
            {
                guna2MessageDialog2.Icon = MessageDialogIcon.Question;
                var result = guna2MessageDialog2.Show("Do you wanna scrape all trackers from the torrents aswell? (UNSTABLE & WIP)", "Question");
                if (result == DialogResult.Yes)
                {
                    foreach (var t in Torrents)
                        trackers.Add(t.link);
                }
            }

            guna2MessageDialog1.Icon = MessageDialogIcon.Information;
            guna2MessageDialog1.Show($"Scraping for Trackers... Please wait. ({trackers.Count()})", "Information");
            richTextBox3.AppendText(LogUtils.FormatLog("Scraping for Trackers... Please wait."));
            new Thread(() =>
            {
                var t = Sources.Trackers.GetTrackers(trackers.ToArray());

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

        private void Sort()
        {
            Column1.DataGridView.Rows.Clear();
            Torrents.Sort((c, y) => int.Parse(c.seeders) - int.Parse(y.seeders));
            Torrents.Reverse();
            Torrents = Torrents.Distinct().ToList();

            var temp = Torrents;

            if (guna2ToggleSwitch8.Checked)
                temp = temp.Where(t => int.Parse(t.seeders) >= guna2NumericUpDown1.Value).ToList();

            if (!guna2ToggleSwitch3.Checked)
                temp = temp.Where(e => e.source != "PirateBay").ToList();

            if (!guna2ToggleSwitch4.Checked)
                temp = temp.Where(e => e.source != "1337x").ToList();

            if (!guna2ToggleSwitch5.Checked)
                temp = temp.Where(e => e.source != "Kickass").ToList();

            if (!guna2ToggleSwitch6.Checked)
                temp = temp.Where(e => e.source != "YTS").ToList();

            foreach (Torrent tor in temp)
                Column1.DataGridView.Rows.Add(tor.id, tor.source, tor.category, " "+tor.name, tor.uploader, tor.size, int.Parse(tor.seeders));
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            if(Directory.Exists(Directory.GetCurrentDirectory() + "\\piatier-cache"))
                Process.Start("explorer.exe", Directory.GetCurrentDirectory()+"\\piatier-cache");
            else
            {
                guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                guna2MessageDialog1.Show("Could not find cache directory.", "Error");
            }
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            guna2Button7.Enabled = false;
            new Thread(() =>
            {
                var ip = HTTPUtils.GetIP();
                this.Invoke(new Action(() =>
                {
                    guna2Button7.Enabled = true;
                    guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                    if (string.IsNullOrEmpty(ip))
                    {
                        guna2MessageDialog1.Icon = MessageDialogIcon.Error;
                        guna2MessageDialog1.Show("Unable to find ip. Something is wrong.", "Error");
                    }
                    guna2MessageDialog1.Show("IP: "+ip, "Information");
                }));
            }).Start();
        }
    }
}

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
                Directory.CreateDirectory("./piatier-cache");
        }


        private static List<Torrent> Torrents = new List<Torrent>();
        private static List<string> Trackers = new List<string>();

        private void DoSearch()
        {
            var bb = 0;
            Torrents.Clear();
            Column1.DataGridView.Rows.Clear();
            guna2Button1.Enabled = false;
            guna2Button3.Enabled = false;

            #region funny search

            new Thread(() =>
            {
                var h = HTTPUtils.Get($"https://apibay.org/q.php?q=" + guna2TextBox1.Text.Replace(" ", "+") + "&cat=0");
                var data = h.ToString();

                var o = JsonConvert.DeserializeObject<List<PirateBayItem>>(data);
                if (o.Count() > 1)
                    foreach (var z in o)
                    {
                        bb++;
                        if (string.IsNullOrEmpty(z.name)) return;
                        var category = "";
                        switch (z.category)
                        {
                            case "207":
                                category = "HD Movies";
                                break;
                            case "201":
                                category = "Movies";
                                break;
                            case "208":
                                category = "HD TV-Show";
                                break;
                            case "205":
                                category = "TV-Shows";
                                break;
                            case "101":
                                category = "Music";
                                break;
                            case "601":
                                category = "E-Book";
                                break;
                            case "504":
                                category = "Games";
                                break;
                            case "401":
                                category = "PC";
                                break;
                            case "404":
                                category = "XBOX360";
                                break;
                            case "403":
                                category = "PSx";
                                break;
                            case "203":
                                category = "Music Video";
                                break;
                            case "104":
                                category = "FLAC";
                                break;
                            case "506":
                                category = "Movie Clips";
                                break;
                            case "602":
                                category = "Comic";
                                break;
                            case "599":
                                category = "Other";
                                break;
                            case "699":
                                category = "Other";
                                break;
                            case "199":
                                category = "Other";
                                break;
                            case "501":
                                category = "Porn";
                                break;
                            case "505":
                                category = "Porn";
                                break;
                            case "301":
                                category = "Windows";
                                break;
                            case "302":
                                category = "Mac/Apple";
                                break;
                            case "399":
                                category = "Other OS";
                                break;
                            case "408":
                                category = "Android";
                                break;
                            case "103":
                                category = "Sound Clips";
                                break;
                            case "605":
                                category = "Physibles";
                                break;
                            case "503":
                                category = "Pictures";
                                break;
                            case "206":
                                category = "Handheld";
                                break;
                            case "102":
                                category = "Audio Books";
                                break;
                        }

                        Torrent tor = new Torrent();
                        tor.id = bb;
                        tor.link = "https://thepiratebay.org/description.php?id=" + z.id;
                        tor.seeders = z.seeders;
                        tor.category = category;
                        tor.size = MiscUtils.BytesToString(long.Parse(z.size));
                        tor.name = z.name;
                        tor.source = "PirateBay";
                        if (!Torrents.Contains(tor))
                            Torrents.Add(tor);
                    }

                for (int x = 0; x < 15; x++)
                {
                    var x2 = HTTPUtils.Get("https://www.1377x.to/sort-search/" + guna2TextBox1.Text.Replace(" ", " ") + "/seeders/desc/" + x + "/");
                    if (x2 != null)
                    {
                        if (x2.IsOK)
                        {
                            var xx = x2.ToString().Split('\n');
                            for (int i = 0; i < xx.Length; i++)
                            {
                                bb++;
                                var line = xx[i];
                                var category = "";
                                if (line.Contains("/torrent/") && line.Contains("href"))
                                {
                                    category = Regex.Replace(xx[i - 1], "\\/sub\\/[a-zA-Z]+\\/", "").Replace("/1/", "").Replace("<td class=\"coll-1 name\"><a href=\"", "").Replace("\" class=\"icon\">", "");
                                    var seeders = xx[i + 1].Replace("<td class=\"coll-2 seeds\">", "").Replace("</td>", "");
                                    var size = xx[i + 4].Replace("<td class=\"coll-4 size mob-uploader\">", "").Replace("</td>", "");
                                    var link = Regex.Replace(line, "<i class=\"flaticon-[a-zA-Z0-9]+\"><\\/i><\\/a><a href=\"", "");
                                    link = Regex.Match(line, "\\/torrent\\/\\d+\\/[a-zA-Z0-9\\-_\\s]+\\/\"").Value.Replace("/\"", "");
                                    //link = Regex.Replace(link, "\\/\\\">[a-zA-Z0-9-_\\\\-\\\\s.\\s+]+", "").Replace("</a></td>", "");


                                    line = Regex.Replace(line, "<a href=\"\\/torrent\\/[0-9]+\\/[a-zA-Z0-9-_]+\\/\">", "").Replace("</a>", "");
                                    line = Regex.Replace(line, "<i class=\\\"flaticon-[a-zA-Z0-9-]+\"><\\/i>", "").Replace("</td>", "");


                                    Torrent tor = new Torrent();
                                    tor.id = bb;
                                    tor.link = "https://www.1377x.to" + link;
                                    tor.seeders = seeders;
                                    tor.category = category;
                                    tor.size = size;
                                    tor.name = line;
                                    tor.source = "1337x";
                                    if (!Torrents.Contains(tor))
                                        Torrents.Add(tor);
                                }
                            }
                        }
                    }

                }

                for (int x = 0; x < 15; x++)
                {
                    var h1 = HTTPUtils.Get($"https://kickasstorrents.to/search/" + guna2TextBox1.Text.Replace(" ", " ") + $"/{x}");
                    if (h1 != null)
                    {
                        if (h1.IsOK)
                        {
                            var data3 = h1.ToString();
                            var lines = data3.Split('\n');
                            for (int k = 0; k < lines.Length; k++)
                            {
                                bb++;
                                Torrent tor = new Torrent();
                                tor.id = bb;
                                tor.source = "Kickass";


                                if (lines[k].Contains("markeredBlock torType filmType"))
                                    tor.category = "TV / Movie";
                                else if (lines[k].Contains("markeredBlock torType musicType"))
                                    tor.category = "Music";
                                else if (lines[k].Contains("markeredBlock torType exeType"))
                                    tor.category = "Program";
                                else if (lines[k].Contains("markeredBlock torType Type"))
                                    tor.category = "File";


                                if (!string.IsNullOrWhiteSpace(tor.category))
                                {
                                    if (lines[k + 1].Contains("cellMainLink"))
                                    {
                                        tor.link = "https://kickasstorrents.to" + Regex.Match(lines[k + 1], "\\/[a-zA-Z0-9-_]+.html").ToString();
                                        tor.name = Regex.Replace(lines[k + 2], "<a href=\"\\/[a-zA-Z0-9-_]+.html\" class=\"cellMainLink\">", "");
                                        tor.name = tor.name.Replace("</a>", "");
                                        tor.name = tor.name.Replace("<strong class=\"red\">", "");
                                        tor.name = tor.name.Replace("</strong>", "");
                                    }
                                    if (lines[k + 22].Contains("nobr center"))
                                    {
                                        if (!string.IsNullOrEmpty(Regex.Match(lines[k + 22 + 1], "\\d+(.\\d+|)\\s(GB|MB|KB|B)").ToString()))
                                            tor.size = Regex.Match(lines[k + 22 + 1], "\\d+(.\\d+|)\\s(GB|MB|KB|B)").ToString();
                                    }

                                    if (lines[k + 28].Contains("green center"))
                                    {
                                        tor.seeders = Regex.Match(lines[k + 28 + 1], "\\d+").ToString();
                                    }

                                    if (!Torrents.Contains(tor))
                                        Torrents.Add(tor);
                                }
                            }
                        }
                    }
                }

                this.Invoke(new Action(() =>
                {
                    Sort();
                    guna2Button1.Enabled = true;
                    guna2Button3.Enabled = true;
                    guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                    guna2MessageDialog1.Show("Done loading ;)", "Information");

                    if (!Directory.Exists("./piatier-cache"))
                        Directory.CreateDirectory("./piatier-cache");

                    if (!File.Exists($"./piatier-cache/{guna2TextBox1.Text.ToLower().Replace(" ", "-")}.json"))
                    {
                        File.WriteAllText($"./piatier-cache/{guna2TextBox1.Text.ToLower().Replace(" ", "-")}.json", JsonConvert.SerializeObject(Torrents));
                        guna2TextBox1.Enabled = true;
                    }
                    else
                    {
                        var result = guna2MessageDialog2.Show("Old cache found. Do you wanna update the cache?", "Information");
                        if (result == DialogResult.Yes)
                        {
                            File.WriteAllText($"./piatier-cache/{guna2TextBox1.Text.ToLower().Replace(" ", "-")}.json", JsonConvert.SerializeObject(Torrents));
                            guna2TextBox1.Enabled = true;
                        }
                    }
                   
                }));
            }).Start();
            #endregion

            Torrents.Clear();
            guna2MessageDialog1.Icon = MessageDialogIcon.Information;
            guna2MessageDialog1.Show("Loading torrents... Please be patient..", "Information");
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            guna2TextBox1.Enabled = false;

            if (!Directory.Exists("./piatier-cache"))
                Directory.CreateDirectory("./piatier-cache");

            if (File.Exists($"./piatier-cache/{guna2TextBox1.Text.ToLower().Replace(" ", "-")}.json"))
            {
                var result = guna2MessageDialog2.Show("Old cache for search term found. Do you want to load the old results?", "Information");
                if (result == DialogResult.Yes)
                {
                    Torrents = JsonConvert.DeserializeObject<List<Torrent>>(File.ReadAllText($"./searches/{guna2TextBox1.Text.ToLower().Replace(" ", "-")}.json"));

                    Sort();
                    guna2Button1.Enabled = true;
                    guna2Button3.Enabled = true;
                    guna2TextBox1.Enabled = true;
                    guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                    guna2MessageDialog1.Show("Done loading ;)", "Information");
                }
                else
                {
                    DoSearch();
                }
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
                var find = Torrents.Where(x => x.id == val).First();
                Clipboard.SetText(find.link);
                guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                guna2MessageDialog1.Show("Copied torrent to clipboard.", "Information");
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


        private static string[] trackers =
        {
            "https://www.theunfolder.com/torrent-trackers-list/",
            "https://techspree.net/torrent-tracker-list-updated/",
            "https://fossbytes.com/torrent-trackers-list/",
            "https://www.techlazy.com/torrent-tracker-list/",
            "https://www.startupopinions.com/torrent-tracker/",
            "https://www.thetechbasket.com/torrent-tracker-list/",
            "https://beencrypted.com/privacy/torrenting/torrent-tracker-list/",
            "https://techcult.com/list-of-best-torrent-trackers/",
            "https://www.thetechbasket.com/torrent-tracker-list/",
            "https://www.techwhoop.com/torrent-trackers/",
            "https://gist.github.com/shashank-p/a9700f6b688259e097c2d1de9a03e502",
            "https://github.com/XIU2/TrackersListCollection/blob/master/all.txt",
            "https://github.com/ngosang/trackerslist/blob/master/trackers_all.txt",
            "https://gist.github.com/niranjanshr13/61e1c670b2dcb6dcf321610d99563f08",
            "https://kickasstorrents.to/usearch/searchquery/",
            "https://raw.githubusercontent.com/ngosang/trackerslist/master/trackers_best.txt",
            "https://raw.githubusercontent.com/XIU2/TrackersListCollection/master/best.txt",
        };

        private void GetTrackers()
        {
            guna2MessageDialog1.Icon = MessageDialogIcon.Information;
            guna2MessageDialog1.Show("Scraping for Trackers... Please wait.", "Information");
            new Thread(() =>
            {
                for (int i = 0; i < trackers.Length; i++)
                {
                    if (string.IsNullOrEmpty(trackers[i])) return;

                    var r = HTTPUtils.Get(trackers[i]);
                    if(r != null)
                    {
                        if (r.IsOK)
                        {
                            var data = r.ToString();

                            MatchCollection collection = Regex.Matches(data, @"(udp|http|https):\/\/([a-zA-Z]+\.|)[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+.\d+\/announce");

                            foreach(var match in collection)
                            {
                                if (!Trackers.Contains(match.ToString()))
                                    Trackers.Add(match.ToString());
                            }
                        }
                    }
                }

                Trackers = Trackers.Distinct().ToList();

                this.Invoke(new Action(() =>
                {
                    guna2MessageDialog1.Icon = MessageDialogIcon.Information;
                    guna2MessageDialog1.Show("Found "+Trackers.Count()+" trackers.", "Information");
                    richTextBox1.Lines = Trackers.ToArray();
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
            // I dont trust em either

            if(guna2TabControl1.SelectedTab.Name == "tabPage1")
            {
                guna2Button3.Visible = true;
            }
            else
            {
                guna2Button3.Visible = false;
            }
        }
    }
}

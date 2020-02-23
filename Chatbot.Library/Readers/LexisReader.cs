using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Chatbot.Library.Models;
using HtmlAgilityPack;

namespace Chatbot.Library.Readers
{
    public class LexisReader
    {
        private string m_syllableUrlTemplate = "https://humanum.arts.cuhk.edu.hk/Lexis/lexi-can/";

        private string m_soundUrlTemplate = "https://humanum.arts.cuhk.edu.hk/Lexis/lexi-can/sound/{syllable}.wav";

        private List<Syllable> m_syllables;
        private string m_folder;


        public LexisReader(string folder)
        {
            m_folder = folder;
            m_syllables = new List<Syllable>();
        }

        public List<Syllable> ReadSyllables()
        {
            string url = "https://humanum.arts.cuhk.edu.hk/Lexis/lexi-can/final.php";

            WebClient wc = new WebClient();
            wc.Encoding = Encoding.GetEncoding("big5");
            string data = wc.DownloadString(url);
            string pattern = @"pho-rel.php\?s2=(\w)";
            MatchCollection matches = Regex.Matches(data, pattern);

            foreach (Match match in matches)
            {
                string link = Path.Combine(match.Value);
                ReadSyllableLink(wc, link);

            }

            return m_syllables;

        }

        public void ReadSyllableLink(WebClient wc, string link)
        {
            string data = wc.DownloadString(link);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//tr");

            foreach (HtmlNode node in nodes)
            {
                HtmlNode syllableNode = node.SelectSingleNode("./td[1]");
                HtmlNode soundNode = node.SelectSingleNode("./td[2]/a");
                HtmlNodeCollection symbolNodes = node.SelectNodes("./td[3]/a");

                if (syllableNode!=null && soundNode !=null &&
                    symbolNodes.Count > 0)
                {
                    string soundPattern = @"sound.php\?s=(\w+)";
                    Match match = Regex.Match(soundNode.Attributes["href"].Value, soundPattern);
                    string syllableName = match.Groups[1].Value;
                    string soundUrl = m_soundUrlTemplate.Replace("{syllable}", syllableName);
                    string filename = Path.GetFileName(soundUrl);
                    string soundPath = Path.Combine(m_folder,"Sound", filename);
                    soundPath = Path.GetFullPath(soundPath);
                    if (!File.Exists(soundPath))
                    {
                        wc.DownloadFile(soundUrl, soundPath);
                    }
                    Syllable syllable = new Syllable();
                    syllable.Name = syllableName;
                    syllable.SoundPath = soundPath;
                    foreach (HtmlNode symbolNode in symbolNodes)
                    {
                        syllable.Symbols.Add(symbolNode.InnerText);
                    }
                    m_syllables.Add(syllable);
                }

            }
        }
    }
}

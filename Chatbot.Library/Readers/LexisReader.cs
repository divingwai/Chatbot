using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Chatbot.Database;
using Chatbot.Library.Models;
using HtmlAgilityPack;
using System.Linq;

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
            string pattern = @"pho-rel.php\?s2=(\w+)";
            MatchCollection matches = Regex.Matches(data, pattern);

            foreach (Match match in matches)
            {
                string link = Path.Combine(m_syllableUrlTemplate, match.Value);
                ReadSyllableLink(wc, link);

            }

            return m_syllables;

        }

        public void SetSyllables(List<Syllable> syllables)
        {
            m_syllables = syllables;

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
                    string soundPath = Path.Combine(m_folder,"Sound", Source.cuhk.ToString(), filename);
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

        public void SaveSyllables(List<Syllable> syllables)
        {

            using (ChatbotEntities dc = ChatbotEntities.GetDataContext())
            {
                //1) get symbol list from syllables

                List<string> currentSymbolIds = dc.chatbot_symbol.Select(p => p.id).ToList();

                List<string> symbolIds = syllables.SelectMany(p => p.Symbols).GroupBy(p => p).Select(p => p.First()).ToList();

                foreach (string symbolId in symbolIds)
                {
                    if (!currentSymbolIds.Contains(symbolId))
                    {
                        chatbot_symbol dbSymbol = new chatbot_symbol();
                        dbSymbol.id = symbolId;
                        dc.chatbot_symbol.Add(dbSymbol);
                    }
                }
                dc.SaveChanges();


                List<string> currentSyllableIds = dc.chatbot_syllable.Select(p => p.id).ToList();

                List<string> syllableIds = syllables.GroupBy(p => p.Name).Select(p => p.Key).ToList();

                //2) create syllables
                foreach (string syllableId in syllableIds)
                {
                    if (!currentSyllableIds.Contains(syllableId))
                    {
                        chatbot_syllable dbSyllable = new chatbot_syllable();
                        dbSyllable.id = syllableId;
                        dc.chatbot_syllable.Add(dbSyllable);
                        dc.SaveChanges();
                    }
                }


                //3) create sounds for the data source
                string sourceId = Source.cuhk.ToString();
                chatbot_source dbSource = dc.chatbot_source.Where(p => p.id == sourceId).FirstOrDefault();
                if (dbSource == null)
                {
                    dbSource = new chatbot_source { id = sourceId };
                    dc.chatbot_source.Add(dbSource);
                    dc.SaveChanges();
                }

                List<string> currentSoundIds = dc.chatbot_sound.Where(p => p.source_id == sourceId).Select(p => p.syllable_id).ToList();

                foreach (Syllable syllable in syllables)
                {
                    if (!currentSoundIds.Contains(syllable.Name))
                    {
                        chatbot_sound dbSound = new chatbot_sound
                        {
                            sound_path = syllable.SoundPath,
                            source_id = sourceId,
                            syllable_id = syllable.Name
                        };
                        dc.chatbot_sound.Add(dbSound);
                        currentSoundIds.Add(syllable.Name);
                    }

                }
                dc.SaveChanges();

                //4) create words
                foreach (Syllable syllable in syllables)
                {
                    foreach (string symbol in syllable.Symbols)
                    {
                        chatbot_word dbWord = dc.chatbot_word.Where(p => p.syllable_id == syllable.Name && p.symbol_id == symbol).FirstOrDefault();

                        if (dbWord == null)
                        {
                            dbWord = new chatbot_word();
                            dbWord.syllable_id = syllable.Name;
                            dbWord.symbol_id = symbol;
                            dc.chatbot_word.Add(dbWord);
                            dc.SaveChanges();
                        }
                    }


                }
            }
        }
    }
}

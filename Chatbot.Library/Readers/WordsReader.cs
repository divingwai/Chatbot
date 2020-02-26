using System;
using System.Collections.Generic;
using System.Net;
using Chatbot.Library.Models;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;
using Chatbot.Database;
using System.IO;

namespace Chatbot.Library.Readers
{
    public class WordsReader
    {


        private string m_urlTemplate = "https://ykyi.net/hongkong/dict/index.php?char={word}";

        private string m_urlSoundTemplate = "https://ykyi.net/mp3_cantonese/{syllable}.mp3";

        private string m_path;

        private List<Word> m_words;

        public WordsReader(string path)
        {
            m_path = path;
            m_words = new List<Word>();
        }

        public void SetWords(List<Word> words)
        {
            m_words = words;
        }

        public List<Syllable> ReadSyllables()
        {
            List<string> syllableIds;

            using (var dc = ChatbotEntities.GetDataContext())
            {
                syllableIds = dc.chatbot_syllable.Select(p => p.id).ToList();

            }

            WebClient webClient = new WebClient();

            List<Syllable> syllables = new List<Syllable>();

            foreach (string syllableId in syllableIds)
            {
                Syllable syllable = new Syllable();
                string url = m_urlSoundTemplate.Replace("{syllable}", syllableId);
                string filename = Path.GetFileName(url);
                string filepath = Path.Combine(m_path, "Sound", Source.ykyi.ToString(), filename);
                filepath = Path.GetFullPath(filepath);
                syllable.Name = syllableId;
                syllable.SoundPath = filepath;

                if (!File.Exists(filepath))
                {
                    webClient.DownloadFile(url, filepath);
                }
                syllables.Add(syllable);

            }
            return syllables;

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
                string sourceId = Source.ykyi.ToString();
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

        public void SaveWords(List<Word> words)
        {
            using (var dc = ChatbotEntities.GetDataContext())
            {

                foreach (Word word in words)
                {
                    chatbot_syllable syllable = dc.chatbot_syllable.Where(p => p.id == word.Syllable).FirstOrDefault();


                    if (syllable != null)
                    {

                        if (!string.IsNullOrEmpty(word.Symbol))
                        {
                            chatbot_symbol dbSymbol = dc.chatbot_symbol.Where(p => p.id == word.Symbol).FirstOrDefault();

                            if (dbSymbol == null)
                            {
                                dbSymbol = new chatbot_symbol();
                                dbSymbol.id = word.Symbol;
                                dc.chatbot_symbol.Add(dbSymbol);
                                dc.SaveChanges();
                            }


                            chatbot_word dbWord = dc.chatbot_word.Where(p => p.syllable_id == word.Syllable && p.symbol_id == word.Symbol).FirstOrDefault();


                            if (dbWord == null)
                            {
                                dbWord = new chatbot_word();
                                dbWord.syllable_id = word.Syllable;
                                dbWord.symbol_id = word.Symbol;
                                dbWord.explanation = word.Explanation;
                                dc.chatbot_word.Add(dbWord);
                                dc.SaveChanges();

                            }
                            else
                            {
                                dbWord.syllable_id = word.Syllable;
                                dbWord.symbol_id = word.Symbol;
                                dbWord.explanation = word.Explanation;
                                dc.Entry(dbWord).State = System.Data.Entity.EntityState.Modified;
                                dc.SaveChanges();
                            }

                        }
                        if (word.Phrases != null)
                        {
                            foreach (string phrase in word.Phrases)
                            {
                                if (!dc.chatbot_phrase.Any(p => p.id == phrase))
                                {
                                    chatbot_phrase dbPhrase = new chatbot_phrase();
                                    dbPhrase.id = phrase;
                                    dc.chatbot_phrase.Add(dbPhrase);
                                    dc.SaveChanges();
                                }

                                if (!dc.chatbot_wordphrase.Any(p => p.phrase_id == phrase && p.syllable_id == word.Syllable && p.symbol_id == word.Symbol))
                                {
                                    chatbot_wordphrase dbWordPhrase = new chatbot_wordphrase();
                                    dbWordPhrase = new chatbot_wordphrase();
                                    dbWordPhrase.phrase_id = phrase;
                                    dbWordPhrase.syllable_id = word.Syllable;
                                    dbWordPhrase.symbol_id = word.Symbol;
                                    dbWordPhrase.location = phrase.IndexOf(word.Symbol);
                                    dc.chatbot_wordphrase.Add(dbWordPhrase);
                                    dc.SaveChanges();
                                }
                            }
                        }
                    }
                }
                dc.SaveChanges();
            }
        }


        public List<Word> ReadWords(List<string> symbolNames)
        {
            WebClient webClient = new WebClient();

            Dictionary<string, string> errors = new Dictionary<string, string>();

            foreach (string symbolName in symbolNames)
            {
                Console.Write(symbolName + ",");
                //try
                //{
         
                    List<Word> words = ReadWordLink(webClient, symbolName);
                    SaveWords(words);
          

                //}
                //catch (Exception ex)
                //{
                //    errors.Add(symbolName, ex.Message);
                //}

            }
            return m_words;
        }

        public List<Word> ReadWordLink(WebClient webClient, string symbolName)
        {
            List<Word> words = new List<Word>();
             webClient = new WebClient();

            string url = m_urlTemplate.Replace("{word}", symbolName);

            string data = webClient.DownloadString(url);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);

            HtmlNode dictsNode = doc.DocumentNode.SelectSingleNode("//*[@id='dicts']");
            if (dictsNode != null)
            {
                HtmlNodeCollection wordNodes = dictsNode.SelectNodes("./ul");

                foreach (HtmlNode wordNode in wordNodes)
                {

                    HtmlNode phoneticNode = wordNode.SelectSingleNode(".//*[@class='phonetic']");
                    string syllable = phoneticNode.InnerText;
                    string wordInnerHtml = wordNode.InnerHtml;

   

                    Word word = new Word();
                    word.Symbol = symbolName;
                    word.Syllable = syllable;
                    word.Phrases = new List<string>();

                    HtmlNode preExplanNode = wordNode.SelectSingleNode(".//text()[contains(., '解釋')]");
                    HtmlNodeCollection explanNodes = preExplanNode.NextSibling.SelectNodes("./li");


                    foreach (HtmlNode explanNode in explanNodes)
                    {
                        string explanText = explanNode.InnerText.Trim();

                        string bracketPattern = @"\(.+\)";

                        explanText = Regex.Replace(explanText, bracketPattern, "");

                       


                        MatchCollection matches = Regex.Matches(explanText, @"[\p{Lo}]+");

                        List<string> phrases = new List<string>();
                        foreach (Match match in matches)
                        {
                            phrases.Add(match.Value);
                        }
                        foreach (string phrase in phrases)
                        {

                            string badPattern = @"[「」a-zA-Z0-9！│~]";

                            if (!Regex.IsMatch(phrase, badPattern))
                            {

                                if (phrase.Contains(symbolName) && phrase.Count()>1 && phrase.Count() <= 8)
                                {
                                    word.Phrases.Add(phrase);
                                }
                            }
                        }
                    }
                    words.Add(word);
                }

            }


            HtmlNode basicNode = doc.DocumentNode.SelectSingleNode("./*[@id='basic2']");

            if (basicNode != null)
            {

            }

            return words;

        }
    }
}


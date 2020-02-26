using System;
using Chatbot.Library.Readers;
using Chatbot.Library.Models;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using Chatbot.Database;

namespace Chatbot.Library.Services
{
    public class ChatbotService
    {
        public ChatbotService()
        {
        }



        public List<Syllable> UpdateSyllables(Source source)
        {

            string folder = "../../../../";
            string filepath = Path.Combine(folder, source.ToString() + ".json");
            List<Syllable> syllables;

            if (source == Source.cuhk)
            {

                LexisReader reader = new LexisReader(folder);

                if (!File.Exists(filepath))
                {
                    syllables = reader.ReadSyllables();

                    string data = JsonConvert.SerializeObject(syllables);

                    using (StreamWriter sw = new StreamWriter(filepath))
                    {
                        sw.Write(data);
                    }
                }
                else
                {
                    using (StreamReader sr = new StreamReader(filepath))
                    {
                        string data = sr.ReadToEnd();
                        syllables = JsonConvert.DeserializeObject<List<Syllable>>(data);
                        reader.SetSyllables(syllables);
                    }
                }

            }
            else
            {
                WordsReader reader = new WordsReader(folder);

                if (!File.Exists(filepath))
                {
                    syllables = reader.ReadSyllables();

                    string data = JsonConvert.SerializeObject(syllables);

                    using (StreamWriter sw = new StreamWriter(filepath))
                    {
                        sw.Write(data);
                    }
                }

                else
                {
                    using (StreamReader sr = new StreamReader(filepath))
                    {
                        string data = sr.ReadToEnd();
                        syllables = JsonConvert.DeserializeObject<List<Syllable>>(data);
                    }
                }
            }
            return syllables;

        }

        public List<Word> UpdateWords(Source source)
        {
            List<string> syllableNames;

            using (ChatbotEntities dc = ChatbotEntities.GetDataContext())
            {
                string sql = "SELECT DISTINCT * FROM `chatbot_symbol` s left join `chatbot_wordphrase` w on s.id = w.symbol_id where w.symbol_id is null";
                List<chatbot_symbol> dbSymbols = dc.Database.SqlQuery<chatbot_symbol>(sql).ToList();
                syllableNames = dbSymbols.Select(p => p.id).ToList();
                syllableNames = dc.chatbot_symbol.Select(p => p.id).ToList();
               // syllableNames = new List<string> { "不" };
            }

            string folder = "../../../../";
            string filepath = Path.Combine(folder, "word.json");
            List<Word> words;


            WordsReader reader = new WordsReader(folder);

            if (!File.Exists(filepath))
            {

                words = reader.ReadWords(syllableNames);

                string data = JsonConvert.SerializeObject(words);

                using (StreamWriter sw = new StreamWriter(filepath))
                {
                    sw.Write(data);
                }
            }
            else
            {
                using (StreamReader sr = new StreamReader(filepath))
                {
                    string data = sr.ReadToEnd();
                    words = JsonConvert.DeserializeObject<List<Word>>(data);
                    reader.SetWords(words);
                }
            }



            return words;
        }

        public void SaveWords(Source source, List<Word> words)
        {


        }

        public void SaveSyllables(Source source, List<Syllable> syllables)
        {
            string folder = "../../../../";

            if (source == Source.cuhk)
            {

                LexisReader reader = new LexisReader(folder);
                reader.SaveSyllables(syllables);
            }
            else
            {
                WordsReader reader = new WordsReader(folder);
                reader.SaveSyllables(syllables);

            }


        }


    }
}
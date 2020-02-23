using System;
using Chatbot.Database.DataContext;
using Chatbot.Library.Readers;
using Chatbot.Library.Models;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using Chatbot.Database.Entities;

namespace Chatbot.Library.Services
{
    public class ChatbotService
    {
        public ChatbotService()
        {
        }


        public void UpdateData()
        {
            string folder = "../../../../";
            string filepath = Path.Combine(folder, "data.json");
            List<Syllable> syllables;

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
                string sourceId = "cuhk";
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
using System;
using Chatbot.Database.DataContext;
using Chatbot.Library.Readers;
using Chatbot.Library.Models;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

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

            if (!File.Exists(filepath))
            {

                LexisReader reader = new LexisReader(folder);

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


            using (ChatbotEntities entities = ChatbotEntities.GetDataContext())
            {
                entities.Database.CreateIfNotExists();

            }
        }
    }
}

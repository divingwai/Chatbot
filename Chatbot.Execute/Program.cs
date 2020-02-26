using System;
using System.Collections.Generic;
using Chatbot.Library.Models;
using Chatbot.Library.Services;
using System.Linq;

namespace Chatbot.Execute
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            ChatbotService chatbotService = new ChatbotService();
            //List<Syllable> syllables = chatbotService.UpdateSyllables(Source.cuhk);
            //chatbotService.SaveSyllables(Source.cuhk, syllables);
            List<Syllable> syllables = chatbotService.UpdateSyllables(Source.ykyi);
            chatbotService.SaveSyllables(Source.ykyi, syllables);
            List<Word> words = chatbotService.UpdateWords(Source.ykyi);
            chatbotService.SaveWords(Source.ykyi, words);
        }
    }
}

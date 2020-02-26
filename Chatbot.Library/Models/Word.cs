using System;
using System.Collections.Generic;

namespace Chatbot.Library.Models
{
    public class Word
    {
        public string Symbol { get; set; }


        public string Syllable { get; set; }

        public string Explanation { get; set; }

        public List<string> Phrases { get; set; }
 
        public Word()
        {
        }
    }
}

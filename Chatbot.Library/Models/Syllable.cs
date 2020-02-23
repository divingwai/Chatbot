using System;
using System.Collections.Generic;

namespace Chatbot.Library.Models
{
    public class Syllable
    {
        public string Name { get; set; }

        public string SoundPath { get; set; }

        public List<string> Symbols { get; set; }

        public Syllable()
        {
            Symbols = new List<string>();
        }
    }
}

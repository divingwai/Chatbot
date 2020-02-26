using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Chatbot.Database
{
    public class chatbot_wordphrase
    {
        [Key]
        public string symbol_id { get; set; }

        [Key]
        public string syllable_id { get; set; }

        [Key]
        public string phrase_id { get; set; }

        public int location { get; set; }

        public virtual chatbot_syllable syllable  { get; set; }

        public virtual chatbot_symbol symbol { get; set; }

        public virtual chatbot_word word { get; set; }

        public virtual chatbot_phrase phrase { get; set; }


        public chatbot_wordphrase()
        {

        }
    }
}

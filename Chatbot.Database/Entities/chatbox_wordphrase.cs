using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Chatbot.Database.Entities
{
    public class chatbot_wordphrase
    {
        [Key]
        public string word_id { get; set; }

        [Key]
        public string phrase_id { get; set; }

        public int location { get; set; }

        public chatbot_word word { get; set; }

        public chatbot_phrase phrase { get; set; }


        public chatbot_wordphrase()
        {

        }
    }
}

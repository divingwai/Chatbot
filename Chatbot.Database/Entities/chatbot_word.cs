using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Chatbot.Database.Entities
{
    public class chatbot_word
    {
        [Key]
        public string symbol_id { get; set; }

        [Key]
        public string syllable_id { get; set; }

        public string explanation { get; set; }

        public virtual ICollection<chatbot_wordphrase> wordphrases { get; set; }

        public chatbot_syllable syllable { get; set; }

        public chatbot_symbol symbol { get; set; }

        public chatbot_word()
        {
            wordphrases = new HashSet<chatbot_wordphrase>();
        }
    }
}

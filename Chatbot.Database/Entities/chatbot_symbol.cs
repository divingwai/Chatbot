using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Chatbot.Database
{
    public class chatbot_symbol
    {
        [Key]
        public string id { get; set; }

        public virtual ICollection<chatbot_wordphrase> wordphrases { get; set; }

        public virtual ICollection<chatbot_word> words { get; set; }

        public chatbot_symbol()
        {
            wordphrases = new HashSet<chatbot_wordphrase>();
            words = new HashSet<chatbot_word>();
        }
    }
}

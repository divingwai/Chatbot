using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Chatbot.Database
{
    public class chatbot_syllable
    {
        [Key]
        public string id { get; set; }
        public virtual ICollection<chatbot_sound> sounds { get; set; }

        public virtual ICollection<chatbot_word> words { get; set; }

        public virtual ICollection<chatbot_wordphrase> wordphrases { get; set; }

        public chatbot_syllable()
        {
            sounds = new HashSet<chatbot_sound>();
            words = new HashSet<chatbot_word>();
            wordphrases = new HashSet<chatbot_wordphrase>();
        }
    }
}

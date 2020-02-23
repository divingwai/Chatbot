using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chatbot.Database.Entities
{
    public class chatbot_phrase
    {
        [Key]
        public string id { get; set; }

        public string explanation { get; set; }

        public virtual ICollection<chatbot_wordphrase> wordphrases { get; set; }
 
        public chatbot_phrase()
        {
            wordphrases = new HashSet<chatbot_wordphrase>();
        }
    }
}

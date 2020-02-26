using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Chatbot.Database
{
    public class chatbot_source
    {
        [Key]
        public string id { get; set; }

        public virtual ICollection<chatbot_sound> sounds { get; set; }


        public chatbot_source()
        {
            sounds = new HashSet<chatbot_sound>();
        }
    }
}

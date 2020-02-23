using System;
using System.ComponentModel.DataAnnotations;

namespace Chatbot.Database.Entities
{
    public class chatbot_sound
    {

        [Key]
        public string syllable_id { get; set; }

        [Key]
        public string source_id  { get; set; }


        public virtual chatbot_syllable syllable { get; set; }

        public virtual chatbot_source source { get; set; }

        public string sound_path { get; set; }
        public chatbot_sound()
        {
        }
    }
}

using System;
using System.Data.Entity;
using Chatbot.Database.Entities;

namespace Chatbot.Database.DataContext
{
    public class ChatbotEntities : DbContext
    {

        public static ChatbotEntities GetDataContext()
        {
            ChatbotEntities entities = new ChatbotEntities();

            entities.Database.CreateIfNotExists();
            return entities;

        }


        public ChatbotEntities()
        {

        }

        //   protected override void OnConfiguring(DbModelBuilder optionsBuilder)
        // {
        //   //optionsBuilder.UseSqlServer(@"Server=.\;Database=EFTutorial;integrated security=True;");
        //   base.OnModelCreating(optionsBuilder);
        //}

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<chatbot_word>()
                .HasKey(x => new { x.syllable_id, x.symbol_id });

            modelBuilder.Entity<chatbot_word>()
                .HasRequired(x => x.syllable)
                .WithMany(p => p.words)
                .HasForeignKey(x => x.syllable_id);

            modelBuilder.Entity<chatbot_syllable>()
                .HasKey(x => x.id);

            modelBuilder.Entity<chatbot_sound>()
                .HasKey(k => new { k.syllable_id, k.source_id });

            modelBuilder.Entity<chatbot_sound>()
               .HasRequired(x => x.syllable)
               .WithMany(x => x.sounds)
               .HasForeignKey(x => x.syllable_id);

            modelBuilder.Entity<chatbot_sound>()
                .HasRequired(x => x.source)
                .WithMany(x => x.sounds)
                .HasForeignKey(x => x.source_id);


            modelBuilder.Entity<chatbot_wordphrase>()
                .HasKey(k => new { k.symbol_id, k.syllable_id , k.phrase_id });

            modelBuilder.Entity<chatbot_wordphrase>()
                .HasRequired(x => x.word)
                .WithMany(x => x.wordphrases)
                .HasForeignKey(x => new { x.syllable_id, x.symbol_id });

            modelBuilder.Entity<chatbot_wordphrase>()
                .HasRequired(x => x.phrase)
                .WithMany(x => x.wordphrases)
                .HasForeignKey(x => x.phrase_id);

            modelBuilder.Entity<chatbot_wordphrase>()
               .HasRequired(x => x.symbol)
               .WithMany(x => x.wordphrases)
               .HasForeignKey(x => x.symbol_id);


            modelBuilder.Entity<chatbot_wordphrase>()
               .HasRequired(x => x.syllable)
               .WithMany(x => x.wordphrases)
               .HasForeignKey(x => x.syllable_id);


            base.OnModelCreating(modelBuilder);
        }


    }
}

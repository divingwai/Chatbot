using System;
using Chatbot.Database.DataContext;

namespace Chatbot.Library.Services
{
    public class ChatbotService
    {
        public ChatbotService()
        {
        }


        public void UpdateData()
        {
            using (ChatbotEntities entities = ChatbotEntities.GetDataContext())
            {
                entities.Database.CreateIfNotExists();

            }
        }
    }
}

using System;
using Chatbot.Library.Services;

namespace Chatbot.Execute
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            ChatbotService chatbotService = new ChatbotService();
            chatbotService.UpdateData();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POEChatbot
{
    public class QuizQuestion
    {
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; }

        // ✅ Add this default constructor
        public QuizQuestion() { }

        // Existing constructor
        public QuizQuestion(string text, string[] options, int correctIndex, string explanation)
        {
            Text = text;
            Options = new List<string>(options);
            CorrectIndex = correctIndex;
            Explanation = explanation;
        }
    }
}


// --- Supporting Classes --- 
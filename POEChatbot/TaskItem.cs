using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POEChatbot
{
    public enum Priority
    {
        Low,
        Medium,
        High
    }

    public enum Category
    {
        General,
        Personal,
        Work,
        Security
    }

    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool ReminderTriggered { get; set; } = false;
        public Priority TaskPriority { get; set; }
        public Category TaskCategory { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsCompleted { get; set; } = false;

        



    }
}

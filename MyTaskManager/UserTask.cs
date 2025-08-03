using System.Text.Json.Serialization;

namespace MyTaskManager
{
    public class UserTask
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime Created { get; set; }
        public DateTime Completed { get; set; }
        public Priority TaskPriority { get; set; }
        public bool IsCompleted { get; set; }

        [JsonConstructor]
        public UserTask(string name, string description, DateTime created, Priority taskPriority)
        {
            Name = name;
            Description = description;
            Created = created;
            TaskPriority = taskPriority;
            IsCompleted = false;
        }

        public UserTask(string name, string description, DateTime created, DateTime completed)
        {
            Name = name;
            Description = description;
            Created = created;
            Completed = completed;
            IsCompleted = true;
        }

        public static Priority GetTaskPriority(int priority)
        {
            switch (priority)
            {
                case 0:
                    return Priority.Низкая;
                case 1:
                    return Priority.Средняя;
                case 2:
                    return Priority.Высокая;
                case 3:
                    return Priority.Срочная;
                default:
                    return Priority.Средняя;
            }
        }

        public enum Priority
        {
            Низкая, Средняя, Высокая, Срочная
        }
    }
}

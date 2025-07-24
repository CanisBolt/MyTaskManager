namespace MyTaskManager
{
    public class UserTask
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime Created { get; set; }
        public Priority TaskPriority { get; set; }

        public UserTask(string name, string description, DateTime created, Priority taskPriority)
        {
            Name = name;
            Description = description;
            Created = created;
            TaskPriority = taskPriority;
        }

        public static Priority SetTaskPriority(int priority)
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

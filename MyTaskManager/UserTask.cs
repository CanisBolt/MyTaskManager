namespace MyTaskManager
{
    public class UserTask
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime Created { get; set; }

        public UserTask(string name, string description, DateTime created)
        {
            Name = name;
            Description = description;
            Created = created;
        }
    }
}

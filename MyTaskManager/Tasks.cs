namespace MyTaskManager
{
    public class Tasks
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime Created { get; set; }

        public Tasks(string name, string description, DateTime created)
        {
            Name = name;
            Description = description;
            Created = created;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTaskManager
{
    public class TaskManagerData
    {
        public List<UserTask> ActiveTasks { get; set; } = new List<UserTask>();
        public List<UserTask> ArchiveTasks { get; set; } = new List<UserTask>();

        public TaskManagerData()
        {

        }
    }
}

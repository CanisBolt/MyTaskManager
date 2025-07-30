using MyTaskManager;
using System.Text.Json;
using System.Resources;
using System.Reflection;

ResourceManager resourceManager = new ResourceManager("MyTaskManager.Resource", Assembly.GetExecutingAssembly());
const string SaveDirectory = "Saves";
TaskManagerData taskManagerData = new TaskManagerData();

if (!Directory.Exists(SaveDirectory))
{
    Directory.CreateDirectory(SaveDirectory);
}
int mainChoice = -1;

do
{
    mainChoice = GetUserInput(0, 7, PrintEntryText());
    switch (mainChoice)
    {
        case 1:
            PrintTask();
            break;
        case 2:
            AddTask();
            break;
        case 3:
            DeleteTask();
            break;
        case 4:
            await SaveTasksAsync();
            break;
        case 5:
            await LoadTasksAsync();
            break;
        case 6:
            EditTask();
            break;
        case 7:
            MarkAsCompleted();
            break;
    }
} while(mainChoice != 0);


string PrintEntryText()
{
    return string.Format(resourceManager.GetString("MainMenuPrompt"), "\n");
}

void PrintAllTasks()
{
    if(taskManagerData.ActiveTasks.Count + taskManagerData.ArchiveTasks.Count == 0)
    {
        Console.WriteLine(resourceManager.GetString("NoTasksFound"));
        return;
    }
    PrintActiveTasks();
    PrintArchiveTasks();
}

void PrintActiveTasks()
{
    if (taskManagerData.ActiveTasks.Count == 0)
    {
        Console.WriteLine(resourceManager.GetString("NoActiveTasks"));
        return;
    }
    DisplayTasks(taskManagerData.ActiveTasks);
}

void PrintArchiveTasks()
{
    if (taskManagerData.ArchiveTasks.Count == 0)
    {
        Console.WriteLine(resourceManager.GetString("NoArchiveTasks"));
        return;
    }
    Console.WriteLine(string.Format(resourceManager.GetString("ArchiveHeader"), "\t", "\n"));
    DisplayTasks(taskManagerData.ArchiveTasks);
}

void PrintTask()
{
    Console.Clear();
    int choice = GetUserInput(0, 4, string.Format(resourceManager.GetString("ChooseTaskView"), "\n"));
    switch(choice)
    {
        case 1:
            PrintAllTasks();
            break;
        case 2:
            PrintActiveTasks();
            break;
        case 3:
            PrintArchiveTasks();
            break;
        case 4:
            PrintTaskByPriority();
            break;
        case 0: 
            return;
    }
}

void PrintTaskByPriority()
{
    Console.Clear();
    int taskPriority = GetTaskPriorityInput() - 1;
    DisplayTasksByPriority(taskManagerData.ActiveTasks, taskPriority);
}

bool CheckTasks()
{
    Console.Clear();
    if(taskManagerData.ActiveTasks.Count + taskManagerData.ArchiveTasks.Count == 0)
    {
        Console.WriteLine(resourceManager.GetString("NoTasksFound"));
        return true;
    }
    return false;
}

void AddTask()
{
    Console.Clear();
    string name = GetTaskNameInput();
    string description = GetTaskDescriptionInput();
    int choice = GetTaskPriorityInput() - 1;

    taskManagerData.ActiveTasks.Add(new UserTask(name, description, DateTime.Now, UserTask.SetTaskPriority(choice)));
    Console.Clear();
    Console.WriteLine(resourceManager.GetString("TaskSuccessfullyAdded"));
    Console.WriteLine();
}

void DeleteTask()
{
    if (CheckTasks())
    {
        return;
    }
    int choice = GetUserInput(1, 2, resourceManager.GetString("DeleteActiveOrArchive"));
    switch(choice)
    {
        case 1:
            PrintActiveTasks();
            if (taskManagerData.ActiveTasks.Count > 0)
            {
                DeleteTasks(taskManagerData.ActiveTasks);
            }
            break;
        case 2:
            PrintArchiveTasks();
            if (taskManagerData.ArchiveTasks.Count > 0)
            {
                DeleteTasks(taskManagerData.ArchiveTasks);
            }
            break;
    }
}

void DeleteTasks(List<UserTask> taskList)
{
    int choice = GetUserInput(0, taskList.Count, resourceManager.GetString("ChooseTaskToDelete"));
    if (choice == 0) return;
    choice--;
    taskList.RemoveAt(choice);
    Console.Clear();
    Console.WriteLine(resourceManager.GetString("TaskSuccessfullyDeleted"));
    Console.WriteLine();
}

void EditTask()
{
    if (CheckTasks())
    {
        return;
    }
    PrintActiveTasks();
    int choice = GetUserInput(0, taskManagerData.ActiveTasks.Count, resourceManager.GetString("ChooseTaskToEdit"));
    if (choice == 0)
    {
        return;
    }
    bool isFileChanged = false;

    choice--;
    string name = GetTaskNameInput();

    if (!string.IsNullOrEmpty(name))
    {
        taskManagerData.ActiveTasks[choice].Name = name;
        isFileChanged = true;
    }

    string description = GetTaskDescriptionInput();
    if (!string.IsNullOrEmpty(description))
    {
        taskManagerData.ActiveTasks[choice].Description = description;
        isFileChanged = true;
    }


    int taskChoice = GetTaskPriorityInput() - 1;
    if (!taskManagerData.ActiveTasks[choice].TaskPriority.Equals(UserTask.SetTaskPriority(taskChoice)))
    {
        taskManagerData.ActiveTasks[choice].TaskPriority = UserTask.SetTaskPriority(taskChoice);
        isFileChanged = true;
    }
    if (isFileChanged)
    {
        taskManagerData.ActiveTasks[choice].Created = DateTime.Now;
    }
    Console.Clear();
    Console.WriteLine(resourceManager.GetString("TaskSuccessfullyUpdated"));
}

void MarkAsCompleted()
{
    if (CheckTasks())
    {
        return;
    }
    PrintActiveTasks();
    int choice = GetUserInput(0, taskManagerData.ActiveTasks.Count, resourceManager.GetString("ChooseTaskToEdit"));
    if (choice == 0)
    {
        return;
    }

    choice--;
    Console.WriteLine(resourceManager.GetString("MarkAsCompletedPrompt"));
    string answer = GetUserInputYesNo();
    if (answer.ToLower().Equals("да"))
    {
        taskManagerData.ActiveTasks[choice].IsCompleted = true;
        Console.WriteLine(resourceManager.GetString("MoveToArchivePrompt"));
        answer = GetUserInputYesNo();
        if (answer.ToLower().Equals("да"))
        {
            taskManagerData.ArchiveTasks.Add(new UserTask(taskManagerData.ActiveTasks[choice].Name, taskManagerData.ActiveTasks[choice].Description, taskManagerData.ActiveTasks[choice].Created, DateTime.Now));
            taskManagerData.ActiveTasks.Remove(taskManagerData.ActiveTasks[choice]);
            Console.WriteLine(resourceManager.GetString("TaskCompletedAndArchived"));
        }
        else
        {
            Console.WriteLine(resourceManager.GetString("DeleteAfterCompletionPrompt"));
            answer = GetUserInputYesNo();
            if (answer.ToLower().Equals("да"))
            {
                taskManagerData.ActiveTasks.Remove(taskManagerData.ActiveTasks[choice]);
            }
        }
    }
}

async Task SaveTasksAsync()
{
    Console.Clear();
    string saveName;
    if (taskManagerData.ActiveTasks.Count == 0)
    {
        Console.WriteLine(resourceManager.GetString("NoTasksFound"));
        return;
    }

    Console.WriteLine(resourceManager.GetString("EnterSaveFileName"));
    saveName = Console.ReadLine();
    while (saveName.Length < 3)
    {
        Console.WriteLine(resourceManager.GetString("SaveFileNameTooShort"));
        saveName = Console.ReadLine();
    }
    saveName += ".json";

    string fullSavePath = Path.Combine(SaveDirectory, saveName);
    if (File.Exists(fullSavePath))
    {
        Console.WriteLine(resourceManager.GetString("FileExistsOverwriteWarning"));
        string answer = GetUserInputYesNo();
        if (answer.ToLower().Equals("да"))
        {
            await Saving(fullSavePath);
        }
        else if (answer.ToLower().Equals("нет"))
        {
            Console.WriteLine(resourceManager.GetString("SaveCancelled"));
            return;
        }
    }
    else
    {
        await Saving(fullSavePath);
    }
    Console.Clear();
    Console.WriteLine(resourceManager.GetString("TasksSuccessfullySaved"));
    Console.WriteLine();
}

async Task Saving(string fullSavePath)
{
    try
    {
        string json = JsonSerializer.Serialize(taskManagerData, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(fullSavePath, json);
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine(string.Format(resourceManager.GetString("ErrorFileNotFound"), Path.GetFileName(fullSavePath)));
    }
    catch (JsonException ex)
    {
        Console.WriteLine(resourceManager.GetString("ErrorCorruptedJson"));
        Console.WriteLine(resourceManager.GetString("ErrorDetails"));
        taskManagerData = new TaskManagerData();
    }
    catch (Exception ex)
    {
        Console.WriteLine(resourceManager.GetString("ErrorGeneric"));
    }
}

async Task LoadTasksAsync()
{
    Console.Clear();
    string dirName = SaveDirectory;
    if (Directory.Exists(dirName))
    {
        string[] files = Directory.GetFiles(dirName, "*.json");
        if(files.Length == 0)
        {
            Console.WriteLine(resourceManager.GetString("NoFilesToLoad"));
            return;
        }

        Console.WriteLine(resourceManager.GetString("FilesToLoadList"));
        for (int i = 0; i < files.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {Path.GetFileName(files[i])}");
        }

        int choice = GetUserInput(0, files.Length, resourceManager.GetString("ChooseFileToLoad"));
        if (choice == 0)
        {
            return;
        }

        string saveName = files[choice-1].ToString();

        if (taskManagerData.ActiveTasks.Count > 0 || taskManagerData.ArchiveTasks.Count > 0)
        {
            Console.WriteLine(resourceManager.GetString("CurrentDataWillBeErased"));
            string answer = GetUserInputYesNo();
            if (answer.ToLower().Equals("нет"))
            {
                Console.WriteLine(resourceManager.GetString("LoadCancelled"));
                return;
            }
        }
        try
        {
            string json = await File.ReadAllTextAsync(saveName);
            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine(resourceManager.GetString("LoadErrorFileEmpty"));
                taskManagerData = new TaskManagerData();
                return;
            }
            var loadedData = JsonSerializer.Deserialize<TaskManagerData>(json);
            if (loadedData == null)
            {
                Console.WriteLine(resourceManager.GetString("LoadFailed"));
                return;
            }
            taskManagerData = loadedData;
            Console.Clear();
            Console.WriteLine(resourceManager.GetString("DataLoadedSuccessfully"));
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine(resourceManager.GetString("ErrorFileNotFound"));
        }
        catch (JsonException ex)
        {
            Console.WriteLine(resourceManager.GetString("ErrorCorruptedJson"));
            Console.WriteLine(resourceManager.GetString("ErrorDetails"));
            taskManagerData = new TaskManagerData();
        }
        catch (Exception ex)
        {
            Console.WriteLine(resourceManager.GetString("ErrorGeneric"));
        }
        Console.WriteLine();
    }
}

string GetTaskNameInput()
{
    Console.WriteLine(resourceManager.GetString("EnterTaskName"));
    string name = Console.ReadLine();
    while(string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine(resourceManager.GetString("TaskNameCannotBeEmpty"));
        Console.WriteLine(resourceManager.GetString("EnterTaskName"));
        name = Console.ReadLine();
    }
    return name;
}

string GetTaskDescriptionInput()
{
    Console.WriteLine(resourceManager.GetString("EnterTaskDescription"));
    string description = Console.ReadLine();
    while (string.IsNullOrWhiteSpace(description))
    {
        Console.WriteLine(resourceManager.GetString("TaskDescriptionCannotBeEmpty"));
        Console.WriteLine(resourceManager.GetString("EnterTaskDescription"));
        description = Console.ReadLine();
    }
    return description;
}


int GetTaskPriorityInput()
{
    int choice = GetUserInput(1, 4, string.Format(resourceManager.GetString("ChoosePriority"), "\n"));
    return choice;
}


/* User input methods */
int GetUserInput(int minValue, int maxValue, string message)
{
    Console.WriteLine(message);
    int choice = -1;
    while (!int.TryParse(Console.ReadLine(), out choice) || choice <  minValue || choice > maxValue)
    {
        Console.WriteLine(resourceManager.GetString("InvalidInput"));
        Console.WriteLine(message);
    }

    return choice;
}

string GetUserInputYesNo()
{
    string answer = Console.ReadLine();
    while (!answer.ToLower().Equals("да") && !answer.ToLower().Equals("нет"))
    {
        Console.WriteLine(resourceManager.GetString("InvalidInputYesNo"));
        answer = Console.ReadLine();
    }

    return answer;
}

/* Display tasks methods */
void DisplayTasks(List<UserTask> printedList)
{
    int taskCount = 0;
    foreach (UserTask task in printedList)
    {
        taskCount++;
        Console.WriteLine(string.Format(resourceManager.GetString("DisplayTaskByPriority"), taskCount, task.Name, "\n", task.Created.ToString("g"), task.Description, task.TaskPriority));
        Console.WriteLine();
    }
}

void DisplayTasksByPriority(List<UserTask> printedList, int requiredPriority)
{
    int taskCount = 0;
    foreach (UserTask task in printedList)
    {
        if ((int)task.TaskPriority == requiredPriority)
        {
            taskCount++;
            Console.WriteLine(string.Format(resourceManager.GetString("DisplayTaskByPriority"), taskCount, task.Name, "\n", task.Created.ToString("g"), task.Description, task.TaskPriority));
            Console.WriteLine();
        }
    }
    if (taskCount == 0)
    {
        Console.WriteLine(resourceManager.GetString("NoTasksWithPriority"));
    }
}
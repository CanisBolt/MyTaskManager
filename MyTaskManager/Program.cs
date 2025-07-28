using MyTaskManager;
using System.Text.Json;

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
    return "Выберите действие:\n" +
        "1. Просмотр задач\n" +
        "2. Добавление задач\n" +
        "3. Удаление задач\n" +
        "4. Сохранение задач\n" +
        "5. Загрузка задач\n" +
        "6. Редактировать задачу\n" +
        "7. Отметить как выполненную\n" +
        "0. Выйти из программы";
}

void PrintAllTasks()
{
    if(taskManagerData.ActiveTasks.Count + taskManagerData.ArchiveTasks.Count == 0)
    {
        Console.WriteLine("Список задач пуст!");
        return;
    }
    PrintActiveTasks();
    PrintArchiveTasks();
}

void PrintActiveTasks()
{
    if (taskManagerData.ActiveTasks.Count == 0)
    {
        Console.WriteLine("Нет активных задач.");
        return;
    }
    DisplayTasks(taskManagerData.ActiveTasks);
}

void PrintArchiveTasks()
{
    if (taskManagerData.ArchiveTasks.Count == 0)
    {
        Console.WriteLine("Нет архивных задач.");
        return;
    }
    Console.WriteLine("-------------------------------------------------\n" +
        "\tАРХИВ:\n");
    DisplayTasks(taskManagerData.ArchiveTasks);
}

void PrintTask()
{
    Console.Clear();
    int choice = GetUserInput(0, 4, "Выберите, какие задачи вы хотели бы посмотреть?\n" +
        "1. Все задачи\n" +
        "2. Все активные задачи\n" +
        "3. Все архивные задачи\n" +
        "4. По срочности\n" +
        "0. Отмена");
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
        Console.WriteLine("Нет задач. Сначала необходимо добавить хотя бы одну задачу.");
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
    Console.WriteLine("Задача успешно  добавлена!");
    Console.WriteLine();
}

void DeleteTask()
{
    if (CheckTasks())
    {
        return;
    }
    int choice = GetUserInput(1, 2, "Введите 1 для удаления активных задач, 2 для удаления архивных задач: ");
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
    int choice = GetUserInput(0, taskList.Count, "Введите порядковый номер задачи, которую хотите удалить. (Для отмены введите 0): ");
    if (choice == 0) return;
    choice--;
    taskList.RemoveAt(choice);
    Console.Clear();
    Console.WriteLine("Задача успешно удалена!");
    Console.WriteLine();
}

void EditTask()
{
    if (CheckTasks())
    {
        return;
    }
    PrintActiveTasks();
    int choice = GetUserInput(0, taskManagerData.ActiveTasks.Count, "Введите порядковый номер файла для редактирования. 0 для отмены редактирования: ");
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
    Console.WriteLine("Задача успешно обновлена!");
}

void MarkAsCompleted()
{
    if (CheckTasks())
    {
        return;
    }
    PrintActiveTasks();
    int choice = GetUserInput(0, taskManagerData.ActiveTasks.Count, "Введите порядковый номер файла для редактирования. 0 для отмены редактирования: ");
    if (choice == 0)
    {
        return;
    }

    choice--;
    Console.WriteLine("Отметить задачу как выполненную?(да/нет)");
    string answer = GetUserInputYesNo();
    if (answer.ToLower().Equals("да"))
    {
        taskManagerData.ActiveTasks[choice].IsCompleted = true;
        Console.WriteLine("Желаете переместить данную задачу в архив?(да/нет)");
        answer = GetUserInputYesNo();
        if (answer.ToLower().Equals("да"))
        {
            taskManagerData.ArchiveTasks.Add(new UserTask(taskManagerData.ActiveTasks[choice].Name, taskManagerData.ActiveTasks[choice].Description, taskManagerData.ActiveTasks[choice].Created, DateTime.Now));
            taskManagerData.ActiveTasks.Remove(taskManagerData.ActiveTasks[choice]);
            Console.WriteLine("Задача выполнена и добавлена в архив!");
        }
        else
        {
            Console.WriteLine("Желаете удалить данную задачу из списка?(да/нет)");
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
        Console.WriteLine("Нет задач. Сначала необходимо добавить хотя бы одну задачу.");
        return;
    }

    Console.WriteLine("Введите имя файла сохранения. Не менее трех символов.");
    saveName = Console.ReadLine();
    while (saveName.Length < 3)
    {
        Console.WriteLine("Неверный ввод. Введите имя не менее трех символов!");
        saveName = Console.ReadLine();
    }
    saveName += ".json";

    string fullSavePath = Path.Combine(SaveDirectory, saveName);
    if (File.Exists(fullSavePath))
    {
        Console.WriteLine("Внимание, данные будут полностью перезаписаны! Вы уверены, что хотите продолжить? (да/нет): ");
        string answer = GetUserInputYesNo();
        if (answer.ToLower().Equals("да"))
        {
            await Saving(fullSavePath);
        }
        else if (answer.ToLower().Equals("нет"))
        {
            Console.WriteLine("Операция сохранения отменена!");
            return;
        }
    }
    else
    {
        await Saving(fullSavePath);
    }
    Console.Clear();
    Console.WriteLine("Задачи успешно сохранены!");
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
        Console.WriteLine($"Ошибка: Файл '{Path.GetFileName(fullSavePath)}' не найден.");
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Ошибка при обработке файла задач: {Path.GetFileName(fullSavePath)}. Возможно, файл поврежден или имеет неверный формат JSON.");
        Console.WriteLine($"Подробности: {ex.Message}");
        taskManagerData = new TaskManagerData();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Произошла ошибка при загрузке задач: {ex.Message}");
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
            Console.WriteLine("Нет файлов для загрузки:");
            return;
        }

        Console.WriteLine("Список файлов для загрузки:");
        for (int i = 0; i < files.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {Path.GetFileName(files[i])}");
        }

        int choice = GetUserInput(0, files.Length, "Введите порядковый номер файла для загрузки. 0 для отмены загрузки: ");
        if (choice == 0)
        {
            return;
        }

        string saveName = files[choice-1].ToString();

        if (taskManagerData.ActiveTasks.Count > 0 || taskManagerData.ArchiveTasks.Count > 0)
        {
            Console.WriteLine("Внимание! Текущие данные будут стерты! Вы уверены, что хотите продолжить? (да/нет)");
            string answer = GetUserInputYesNo();
            if (answer.ToLower().Equals("нет"))
            {
                Console.WriteLine("Операция загрузки отменена!");
                return;
            }
        }
        try
        {
            string json = await File.ReadAllTextAsync(saveName);
            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine($"Ошибка: Файл '{Path.GetFileName(saveName)}' пуст или содержит некорректные данные.");
                taskManagerData = new TaskManagerData();
                return;
            }
            var loadedData = JsonSerializer.Deserialize<TaskManagerData>(json);
            if (loadedData == null)
            {
                Console.WriteLine("Не удалось загрузить!");
                return;
            }
            taskManagerData = loadedData;
            Console.Clear();
            Console.WriteLine("Данные успешно загружены!");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Файл '{Path.GetFileName(saveName)}' не найден.");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка при обработке файла задач: {Path.GetFileName(saveName)}. Возможно, файл поврежден или имеет неверный формат JSON.");
            Console.WriteLine($"Подробности: {ex.Message}");
            taskManagerData = new TaskManagerData();
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"Произошла ошибка при загрузке задач: {ex.Message}");
        }
        Console.WriteLine();
    }
}

static string GetTaskNameInput()
{
    Console.WriteLine("Введите название задачи: ");
    string name = Console.ReadLine();
    while(string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Название задачи не может быть пустым!");
        Console.WriteLine("Введите название задачи: ");
        name = Console.ReadLine();
    }
    return name;
}

static string GetTaskDescriptionInput()
{
    Console.WriteLine("Введите текст задачи: ");
    string description = Console.ReadLine();
    while (string.IsNullOrWhiteSpace(description))
    {
        Console.WriteLine("Текст задачи не может быть пустым!");
        Console.WriteLine("Введите текст задачи: ");
        description = Console.ReadLine();
    }
    return description;
}


static int GetTaskPriorityInput()
{
    int choice = GetUserInput(1, 4, "Выберите степень важности задачи:\n" +
        "1. Низкая\n" +
        "2. Средняя\n" +
        "3. Высокая\n" +
        "4. Срочная");
    return choice;
}


/* User input methods */
static int GetUserInput(int minValue, int maxValue, string message)
{
    Console.WriteLine(message);
    int choice = -1;
    while (!int.TryParse(Console.ReadLine(), out choice) || choice <  minValue || choice > maxValue)
    {
        Console.WriteLine("Неверный ввод.");
        Console.WriteLine(message);
    }

    return choice;
}

static string GetUserInputYesNo()
{
    string answer = Console.ReadLine();
    while (!answer.ToLower().Equals("да") && !answer.ToLower().Equals("нет"))
    {
        Console.WriteLine("Неверный ввод. Введите да или нет.");
        answer = Console.ReadLine();
    }

    return answer;
}

/* Display tasks methods */
static void DisplayTasks(List<UserTask> printedList)
{
    int taskCount = 0;
    foreach (UserTask task in printedList)
    {
        taskCount++;
        Console.WriteLine($"{taskCount}. {task.Name}\n" +
                $"Создана: {task.Created.ToString("g")}\n" +
                $"{task.Description}\n" +
                $"Приоритет: {task.TaskPriority}");
        Console.WriteLine();
    }
}

static void DisplayTasksByPriority(List<UserTask> printedList, int requiredPriority)
{
    int taskCount = 0;
    foreach (UserTask task in printedList)
    {
        if ((int)task.TaskPriority == requiredPriority)
        {
            taskCount++;
            Console.WriteLine($"{taskCount}. {task.Name}\n" +
                $"Создана: {task.Created.ToString("g")}\n" +
                $"{task.Description}\n" +
                $"Приоритет: {task.TaskPriority}");
            Console.WriteLine();
        }
    }
    if (taskCount == 0)
    {
        Console.WriteLine("Нет задач с указанным приоритетом!");
    }
}
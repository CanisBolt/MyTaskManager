using MyTaskManager;
using System.Text.Json;
using System.Xml.Linq;

const string SaveDirectory = "Saves";
List<UserTask> AllTasks = new List<UserTask>();

int choice = -1;

if (!Directory.Exists(SaveDirectory))
{
    Directory.CreateDirectory(SaveDirectory);
}

do
{
    PrintEntryText();
    while(!int.TryParse(Console.ReadLine(), out choice) || choice < 0 || choice > 6)
    {
        Console.WriteLine("Неверный ввод.");
        PrintEntryText();
    }
    switch (choice)
    {
        case 1:
            CheckTasks();
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
    }
} while(choice != 0);


void PrintEntryText()
{
    Console.WriteLine("Выберите действие:\n" +
        "1. Просмотр задач\n" +
        "2. Добавление задач\n" +
        "3. Удаление задач\n" +
        "4. Сохранение задач\n" +
        "5. Загрузка задач\n" +
        "6. Редактировать задачу\n" +
        "0. Выйти из программы");
    Console.WriteLine();
}

void PrintTasks()
{
    int taskCount = 1;
    foreach (UserTask task in AllTasks)
    {
        Console.WriteLine($"{taskCount}. {task.Name}\n" +
            $"Создана: {task.Created}\n" +
            $"{task.Description}\n" +
            $"Приоритет: {task.TaskPriority}");
        taskCount++;
        Console.WriteLine();
    }
}

bool CheckTasks()
{
    Console.Clear();
    if (AllTasks.Count == 0)
    {
        Console.WriteLine("Нет задач. Сначала необходимо добавить хотя бы одну задачу.");
        return true;
    }
    PrintTasks();
    return false;
}

void AddTask()
{
    Console.Clear();
    string name = GetTaskNameInput();
    string description = GetTaskDescriptionInput();
    int choice = GetTaskPriorityInput() - 1;

    AllTasks.Add(new UserTask(name, description, DateTime.Now, UserTask.SetTaskPriority(choice)));
    Console.Clear();
    Console.WriteLine("Задача успешно  добавлена!");
    Console.WriteLine();
}

void DeleteTask()
{
    if(CheckTasks())
    {
        return;
    }

    Console.WriteLine("Введите порядковый номер задачи, которую хотите удалить. (Для отмены введите 0): ");
    int choice = -1;
    while (!int.TryParse(Console.ReadLine(), out choice) || choice < 0 || choice > AllTasks.Count)
    {
        Console.WriteLine("Неверный ввод.");
        Console.WriteLine("Введите порядковый номер задачи, которую хотите удалить. (Для отмены введите 0): ");
    }
    if (choice == 0) return;
    choice--;
    AllTasks.RemoveAt(choice);
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

    Console.WriteLine("Выберите задачу для редактирования: ");
    int choice = -1;
    Console.WriteLine("Введите порядковый номер файла для редактирования. 0 для отмены редактирования: ");
    while (!int.TryParse(Console.ReadLine(), out choice) || choice < 0 || choice > AllTasks.Count)
    {
        Console.WriteLine("Неверный ввод.");
    }
    if (choice == 0)
    {
        return;
    }
    UserTask editTask = new UserTask(AllTasks[choice - 1].Name, AllTasks[choice - 1].Description, AllTasks[choice - 1].Created, AllTasks[choice - 1].TaskPriority);
    bool isFileChanged = false;

    string name = GetTaskNameInput();

    if (!string.IsNullOrEmpty(name))
    {
        editTask.Name = name;
        isFileChanged = true;
    }

    string description = GetTaskDescriptionInput();
    if (!string.IsNullOrEmpty(description))
    {
        editTask.Description = description;
        isFileChanged = true;
    }


    int taskChoice = GetTaskPriorityInput() - 1;
    if (!editTask.TaskPriority.Equals(UserTask.SetTaskPriority(taskChoice)))
    {
        editTask.TaskPriority = UserTask.SetTaskPriority(taskChoice);
        isFileChanged |= true;
    }

    choice--;
    if (isFileChanged)
    {
        AllTasks[choice].Name = editTask.Name;
        AllTasks[choice].Description = editTask.Description;
        AllTasks[choice].Created = DateTime.Now;
        AllTasks[choice].TaskPriority = editTask.TaskPriority;
    }
    Console.Clear();
    Console.WriteLine("Задача успешно обновлена!");
}

async Task SaveTasksAsync()
{
    Console.Clear();
    string saveName;
    if (AllTasks.Count == 0)
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
        string answer = Console.ReadLine();
        while (!answer.ToLower().Equals("да") && !answer.ToLower().Equals("нет"))
        {
            Console.WriteLine("Неверный ввод. Введите да или нет.");
            answer = Console.ReadLine();
        }
        if (answer.ToLower().Equals("да"))
        {
            await Saving(fullSavePath);
            Console.WriteLine("Задачи успешно сохранены!");
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
        Console.Clear();
        Console.WriteLine("Задачи успешно сохранены!");
        Console.WriteLine();
    }
}

async Task LoadTasksAsync()
{
    Console.Clear();
    string dirName = "Saves";
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

        int choice = -1;
        Console.WriteLine("Введите порядковый номер файла для загрузки. 0 для отмены загрузки: ");
        while (!int.TryParse(Console.ReadLine(), out choice) || choice < 0 || choice > files.Length)
        {
            Console.WriteLine("Неверный ввод.");
        }
        if (choice == 0)
        {
            return;
        }

        string saveName = files[choice-1].ToString();

        if (AllTasks.Count > 0)
        {
            Console.WriteLine("Внимание! Текущие данные будут стерты! Вы уверены, что хотите продолжить? (да/нет)");
            string answer = Console.ReadLine();
            while (!answer.ToLower().Equals("да") && !answer.ToLower().Equals("нет"))
            {
                Console.WriteLine("Неверный ввод. Введите да или нет.");
                answer = Console.ReadLine();
            }
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
                AllTasks = new List<UserTask>();
                return;
            }
            AllTasks = JsonSerializer.Deserialize<List<UserTask>>(json);
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
            AllTasks = new List<UserTask>();
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"Произошла ошибка при загрузке задач: {ex.Message}");
        }
        Console.WriteLine();
    }
}

async Task Saving(string fullSavePath)
{
    try
    {
        string json = JsonSerializer.Serialize(AllTasks);
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
        AllTasks = new List<UserTask>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Произошла ошибка при загрузке задач: {ex.Message}");
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
    Console.WriteLine("Выберите степень важности задачи:\n" +
            "1. Низкая\n" +
            "2. Средняя\n" +
            "3. Высокая\n" +
            "4. Срочная");
    int choice = -1;
    while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 4)
    {
        Console.WriteLine("Неверный ввод.");
        Console.WriteLine("Выберите степень важности задачи:\n" +
        "1. Низкая\n" +
        "2. Средняя\n" +
        "3. Высокая\n" +
        "4. Срочная");
    }

    return choice;
}
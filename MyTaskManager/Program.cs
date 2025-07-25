﻿using MyTaskManager;
using System.Text.Json;
using System.Xml.Linq;

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

bool PrintActiveTasks()
{
    if(taskManagerData.AllTasks.Count == 0)
    {
        Console.WriteLine("Нет активных задач. Сначала необходимо добавить хотя бы одну задачу.");
        return true;
    }
    int taskCount = 1;
    foreach (UserTask task in taskManagerData.AllTasks)
    {
        Console.WriteLine($"{taskCount}. {task.Name}\n" +
            $"Создана: {task.Created}\n" +
            $"{task.Description}\n" +
            $"Приоритет: {task.TaskPriority}");
        taskCount++;
        Console.WriteLine();
    }
    return false;
}

bool PrintArchiveTasks()
{
    if (taskManagerData.ArchiveTasks.Count == 0)
    {
        Console.WriteLine("Нет архивных задач. Сначала необходимо добавить хотя бы одну задачу.");
        return true;
    }
    Console.WriteLine("-------------------------------------------------\n" +
        "\tАРХИВ:\n");
    int taskCount = 1;
    foreach (UserTask task in taskManagerData.ArchiveTasks)
    {
        Console.WriteLine($"{taskCount}. {task.Name}\n" +
            $"Создана: {task.Created}\n" +
            $"{task.Description}\n" +
            $"Выполнена: {task.Completed}");
        taskCount++;
        Console.WriteLine();
    }
    return false;
}

bool CheckTasks()
{
    Console.Clear();
    if(taskManagerData.AllTasks.Count == 0 && taskManagerData.ArchiveTasks.Count == 0)
    {
        Console.WriteLine("Нет задач. Сначала необходимо добавить хотя бы одну задачу.");
        return true;
    }
    PrintActiveTasks();
    PrintArchiveTasks();
    return false;
}

void AddTask()
{
    Console.Clear();
    string name = GetTaskNameInput();
    string description = GetTaskDescriptionInput();
    int choice = GetTaskPriorityInput() - 1;

    taskManagerData.AllTasks.Add(new UserTask(name, description, DateTime.Now, UserTask.SetTaskPriority(choice)));
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
            if(PrintActiveTasks())
            {
                DeleteTasks(taskManagerData.AllTasks);
            }
            break;
        case 2:
            if (PrintArchiveTasks())
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
    int choice = GetUserInput(0, taskManagerData.AllTasks.Count, "Введите порядковый номер файла для редактирования. 0 для отмены редактирования: ");
    if (choice == 0)
    {
        return;
    }
    bool isFileChanged = false;

    string name = GetTaskNameInput();

    if (!string.IsNullOrEmpty(name))
    {
        taskManagerData.AllTasks[choice].Name = name;
        isFileChanged = true;
    }

    string description = GetTaskDescriptionInput();
    if (!string.IsNullOrEmpty(description))
    {
        taskManagerData.AllTasks[choice].Description = description;
        isFileChanged = true;
    }


    int taskChoice = GetTaskPriorityInput() - 1;
    if (!taskManagerData.AllTasks[choice].TaskPriority.Equals(UserTask.SetTaskPriority(taskChoice)))
    {
        taskManagerData.AllTasks[choice].TaskPriority = UserTask.SetTaskPriority(taskChoice);
        isFileChanged = true;
    }

    choice--;
    if (isFileChanged)
    {
        taskManagerData.AllTasks[choice].Created = DateTime.Now;
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
    int choice = GetUserInput(0, taskManagerData.AllTasks.Count, "Введите порядковый номер файла для редактирования. 0 для отмены редактирования: ");
    if (choice == 0)
    {
        return;
    }

    choice--;
    Console.WriteLine("Отметить задачу как выполненную?(да/нет)");
    string answer = GetUserInputYesNo();
    if (answer.ToLower().Equals("да"))
    {
        taskManagerData.AllTasks[choice].IsCompleted = true;
        Console.WriteLine("Желаете переместить данную задачу в архив?(да/нет)");
        answer = GetUserInputYesNo();
        if (answer.ToLower().Equals("да"))
        {
            taskManagerData.ArchiveTasks.Add(new UserTask(taskManagerData.AllTasks[choice].Name, taskManagerData.AllTasks[choice].Description, taskManagerData.AllTasks[choice].Created, DateTime.Now));
            taskManagerData.AllTasks.Remove(taskManagerData.AllTasks[choice]);
            Console.WriteLine("Задача выполнена и добавлена в архив!");
        }
        else
        {
            Console.WriteLine("Желаете удалить данную задачу из списка?(да/нет)");
            answer = GetUserInputYesNo();
            if (answer.ToLower().Equals("да"))
            {
                taskManagerData.AllTasks.Remove(taskManagerData.AllTasks[choice]);
            }
        }
    }
}

async Task SaveTasksAsync()
{
    Console.Clear();
    string saveName;
    if (taskManagerData.AllTasks.Count == 0)
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

        if (taskManagerData.AllTasks.Count > 0 || taskManagerData.ArchiveTasks.Count > 0)
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
    int choice = GetUserInput(1, 4, "Выберите степень важности задачи:\\n\" +\r\n        \"1. Низкая\\n\" +\r\n        \"2. Средняя\\n\" +\r\n        \"3. Высокая\\n\" +\r\n        \"4. Срочная\"");
    return choice;
}

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
using MyTaskManager;
using System;
using System.Text.Json;

List<Tasks> AllTasks = new List<Tasks>();

int choice = -1;

do
{
    PrintEntryText();
    while(!int.TryParse(Console.ReadLine(), out choice) || choice < 0 || choice > 5)
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
            SaveTasksAsync();
            break;
        case 5:
            LoadTasks();
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
        "0. Выйти из программы");
}

void PrintTasks()
{
    int taskCount = 1;
    foreach (Tasks task in AllTasks)
    {
        Console.WriteLine($"{taskCount}. {task.Name}\n" +
            $"Создана: {task.Created}\n" +
            $"{task.Description}");
        taskCount++;
        Console.WriteLine();
    }
}

void CheckTasks()
{
    if (AllTasks.Count == 0)
    {
        Console.WriteLine("Нет задач. Сначала необходимо добавить хотя бы одну задачу.");
        return;
    }

    PrintTasks();
}

void AddTask()
{
    Console.WriteLine("Введите название задачи: ");
    string name = Console.ReadLine();

    Console.WriteLine("Введите текст задачи: ");
    string description = Console.ReadLine();

    AllTasks.Add(new Tasks(name, description, DateTime.Now));
}

void DeleteTask()
{
    if (AllTasks.Count == 0)
    {
        Console.WriteLine("Нет задач. Сначала необходимо добавить хотя бы одну задачу.");
        return;
    }

    PrintTasks();

    Console.WriteLine("Введите порядковый номер задачи, которую хотите удалить. (Для отмены введите 0)");
    while (!int.TryParse(Console.ReadLine(), out choice) || choice < 0 || choice > AllTasks.Count)
    {
        Console.WriteLine("Неверный ввод.");
        Console.WriteLine("Введите порядковый номер задачи, которую хотите удалить. (Для отмены введите 0)");
    }
    if (choice == 0) return;
    AllTasks.RemoveAt(choice - 1);
}

async Task SaveTasksAsync()
{
    if (AllTasks.Count == 0)
    {
        Console.WriteLine("Нет задач. Сначала необходимо добавить хотя бы одну задачу.");
        return;
    }

    Console.WriteLine("Внимание, данные будут полностью перезаписаны! Вы уверены, что хотите продолжить? (да/нет)");
    string answer = Console.ReadLine();
    while (!answer.ToLower().Equals("да") && !answer.ToLower().Equals("нет"))
    {
        Console.WriteLine("Неверный ввод. Введите да или нет.");
    }
    if(answer.ToLower().Equals("да"))
    {
        string json = JsonSerializer.Serialize(AllTasks);
        await File.WriteAllTextAsync("AllTasks.json", json);
        Console.WriteLine("Задачи успешно сохранены!");
    }
    else if(answer.ToLower().Equals("нет"))
    {
        Console.WriteLine("Операция сохранения отменена!");
        return;
    }
}

async Task LoadTasks()
{
    if (!File.Exists("AllTasks.json"))
    {
        Console.WriteLine("Нет задач. Сначала необходимо добавить хотя бы одну задачу.");
        return;
    }

    Console.WriteLine("Внимание, данные будут полностью перезаписаны! Вы уверены, что хотите продолжить? (да/нет)");
    string answer = Console.ReadLine();
    while (!answer.ToLower().Equals("да") && !answer.ToLower().Equals("нет"))
    {
        Console.WriteLine("Неверный ввод. Введите да или нет.");
    }
    if (answer.ToLower().Equals("да"))
    {
        AllTasks = new List<Tasks>();
        string json = await File.ReadAllTextAsync("AllTasks.json");

        AllTasks = JsonSerializer.Deserialize<List<Tasks>>(json);
        Console.WriteLine("Данные успешно загружены!");
    }
    else if (answer.ToLower().Equals("нет"))
    {
        Console.WriteLine("Операция загрузки отменена!");
        return;
    }
}
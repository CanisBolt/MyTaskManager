using MyTaskManager;
using System.Text.Json;

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
            await SaveTasksAsync();
            break;
        case 5:
            await LoadTasks();
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
    Console.WriteLine();
}

void PrintTasks()
{
    int taskCount = 1;
    foreach (UserTask task in AllTasks)
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

    AllTasks.Add(new UserTask(name, description, DateTime.Now));
}

void DeleteTask()
{
    if (AllTasks.Count == 0)
    {
        Console.WriteLine("Нет задач. Сначала необходимо добавить хотя бы одну задачу.");
        return;
    }

    PrintTasks();

    Console.WriteLine("Введите порядковый номер задачи, которую хотите удалить. (Для отмены введите 0): ");
    int choice = -1;
    while (!int.TryParse(Console.ReadLine(), out choice) || choice < 0 || choice > AllTasks.Count)
    {
        Console.WriteLine("Неверный ввод.");
        Console.WriteLine("Введите порядковый номер задачи, которую хотите удалить. (Для отмены введите 0): ");
    }
    if (choice == 0) return;
    AllTasks.RemoveAt(choice - 1);
}

async Task SaveTasksAsync()
{
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
            string json = JsonSerializer.Serialize(AllTasks);
            await File.WriteAllTextAsync(fullSavePath, json);
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
        string json = JsonSerializer.Serialize(AllTasks);
        await File.WriteAllTextAsync(fullSavePath, json);
        Console.WriteLine("Задачи успешно сохранены!");
    }
}

async Task LoadTasks()
{
    Console.WriteLine();
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
            Console.WriteLine($"{i + 1}. {files[i]}");
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

        AllTasks = new List<UserTask>();
        string json = await File.ReadAllTextAsync(saveName);

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

        AllTasks = JsonSerializer.Deserialize<List<UserTask>>(json);
        Console.WriteLine("Данные успешно загружены!");
    }
    Console.WriteLine();
}
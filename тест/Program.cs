using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

// Тип данных для пользователя
[Serializable]
public class UserData
{
    public string Name { get; set; }
    public int CharactersPerMinute { get; set; }
    public int CharactersPerSecond { get; set; }
}

// Статический класс для работы с таблицей рекордов
public static class Leaderboard
{
    private const string LeaderboardFilePath = "leaderboard.json";
    private static List<UserData> leaderboard;

    // Инициализация таблицы рекордов
    static Leaderboard()
    {
        LoadLeaderboard();
    }

    // Загрузка таблицы рекордов из файла
    private static void LoadLeaderboard()
    {
        if (File.Exists(LeaderboardFilePath))
        {
            string json = File.ReadAllText(LeaderboardFilePath);
            leaderboard = JsonConvert.DeserializeObject<List<UserData>>(json);
        }
        else
        {
            leaderboard = new List<UserData>();
        }
    }

    // Сохранение таблицы рекордов в файл
    private static void SaveLeaderboard()
    {
        string json = JsonConvert.SerializeObject(leaderboard, Formatting.Indented);
        File.WriteAllText(LeaderboardFilePath, json);
    }

    // Добавление пользователя в таблицу рекордов
    public static void AddUser(UserData user)
    {
        leaderboard.Add(user);
        SaveLeaderboard();
    }

    // Вывод таблицы рекордов
    public static void DisplayLeaderboard()
    {
        Console.WriteLine("Leaderboard:");
        foreach (var user in leaderboard)
        {
            Console.WriteLine($"{user.Name} - {user.CharactersPerMinute} CPM, {user.CharactersPerSecond} CPS");
        }
    }
}

// Класс для набора текста
public class TypingTest
{
    private static readonly string TestText = "Я в Москве, ха-ха, я в Москве Ты лох ебаный, ты не можешь тут быть Ну как там Европа, как в Дубае? Кайфуешь?";
    private static readonly object ConsoleLock = new object();
    private static string userInput = string.Empty;

    public void StartTest()
    {
        Console.Write("Введите ваше имя: ");
        string userName = Console.ReadLine();

        DisplayInstructions();
        StartTimer();

        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            lock (ConsoleLock)
            {
                userInput += key.KeyChar;
                DisplayInput();
            }
        } while (key.Key != ConsoleKey.Enter && userInput.Length < TestText.Length);

        CalculateSpeed();

        UserData user = new UserData
        {
            Name = userName,
            CharactersPerMinute = (int)((userInput.Length / 5.0) / (stopwatch.Elapsed.TotalMinutes)),
            CharactersPerSecond = (int)(userInput.Length / stopwatch.Elapsed.TotalSeconds)
        };

        Leaderboard.AddUser(user);
        Leaderboard.DisplayLeaderboard();
    }

    private void DisplayInstructions()
    {
        Console.WriteLine("Текст для набора:");
        Console.WriteLine(TestText);
        Console.WriteLine("Нажмите Enter, чтобы начать тест.");
    }

    private void StartTimer()
    {
        new Thread(() =>
        {
            Thread.Sleep(60000); // Таймер на минуту
            lock (ConsoleLock)
            {
                userInput = userInput.Substring(0, Math.Min(userInput.Length, TestText.Length));
                Console.Clear();
                DisplayInput();
                CalculateSpeed();
            }
        }).Start();
    }

    private void DisplayInput()
    {
        Console.Clear();
        Console.WriteLine("Текст для набора:");
        Console.WriteLine(TestText);
        Console.WriteLine("Ваш ввод:");
        Console.Write(userInput);
    }

    private void CalculateSpeed()
    {
        stopwatch.Stop();
        Console.WriteLine($"\nВремя: {stopwatch.Elapsed.TotalSeconds} секунд");
        Console.WriteLine($"Символов в минуту: {(int)((userInput.Length / 5.0) / (stopwatch.Elapsed.TotalMinutes))}");
        Console.WriteLine($"Символов в секунду: {(int)(userInput.Length / stopwatch.Elapsed.TotalSeconds)}");
    }

    private readonly Stopwatch stopwatch = new Stopwatch();
}

class Program
{
    static void Main(string[] args)
    {
        TypingTest typingTest = new TypingTest();

        while (true)
        {
            typingTest.StartTest();
            Console.Write("Желаете пройти тест еще раз? (y/n): ");
            if (Console.ReadKey().Key != ConsoleKey.Y)
                break;
        }
    }
}

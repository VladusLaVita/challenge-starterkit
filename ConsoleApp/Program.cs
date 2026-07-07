using System;
using System.IO;
using Challenge;
using Challenge.DataContracts;
using ConsoleApp;
using Task = System.Threading.Tasks.Task;
using System.Text.Json;


var path = Path.Combine(AppContext.BaseDirectory, "secrets.json");
var json = File.ReadAllText(path);

using var doc = JsonDocument.Parse(json);

var teamSecret = doc.RootElement
    .GetProperty("TeamSecret")
    .GetString();

if (string.IsNullOrEmpty(teamSecret))
{
    Console.WriteLine("Задай секрет своей команды, чтобы можно было делать запросы от ее имени");
    Console.ReadLine();
    return;
}

var challengeClient = new ChallengeClient(teamSecret);
const string challengeId = "git-course";

Console.WriteLine($"Получение информации о соревновании {challengeId}...");
var challenge = await challengeClient.GetChallengeAsync(challengeId);
Console.WriteLine(challenge.Description);
Console.WriteLine("----------------\n");

const string taskType = "polynomial-root";

var utcNow = DateTime.UtcNow;
string currentRound = "1";
foreach (var round in challenge.Rounds)
{
    if (round.StartTimestamp < utcNow && utcNow < round.EndTimestamp)
        currentRound = round.Id;
}

Console.WriteLine($"Бот запущен в пошаговом режиме для задач [{taskType}] (раунд {currentRound}).");
Console.WriteLine("Нажимайте [ENTER] для подтверждения отправки ответов.\n");

int successCount = 0;
int failCount = 0;

while (true)
{
    try
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Запрашиваю новую задачу у сервера...");
        Console.ResetColor();

        var newTask = await challengeClient.AskNewTaskAsync(currentRound, taskType);

        Console.WriteLine($"[Новое задание] Статус: {newTask.Status}");
        Console.WriteLine($"Вопрос: {newTask.Question}");

        // Вычисляем ответ с помощью Solver
        var answer = Solver.Solve(newTask, taskType);

        // Подсвечиваем ответ желтым и ждем подтверждения от пользователя
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n-> Робот посчитал ответ: {answer}");
        Console.Write("Нажмите [ENTER], чтобы отправить этот ответ на сервер... ");
        Console.ResetColor();

        Console.ReadLine(); // Пауза для контроля человеком

        Console.WriteLine("Отправка ответа на проверку...");
        var updatedTask = await challengeClient.CheckTaskAnswerAsync(newTask.Id, answer);

        if (updatedTask.Status == TaskStatus.Success)
        {
            successCount++;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Ура! Ответ принят системой! (Решено: {successCount})\n");
            Console.ResetColor();
        }
        else
        {
            failCount++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Похоже, ответ не подошел. (Ошибок: {failCount})");
            Console.WriteLine($"Сервер ожидал другой ответ на: {updatedTask.Question}\n");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"Произошла ошибка при обмене данными с сервером: {ex.Message}");
        Console.Write("Нажмите [ENTER], чтобы повторить попытку запроса... ");
        Console.ResetColor();
        Console.ReadLine();
    }

    Console.WriteLine("====================================\n");
}

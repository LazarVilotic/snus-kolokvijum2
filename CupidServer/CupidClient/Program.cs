using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

Console.WriteLine("=== Cupid Client ===");

string username;
do
{
    Console.Write("Username: ");
    username = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(username))
        Console.WriteLine("Username cannot be empty.");
} while (string.IsNullOrWhiteSpace(username));

string city;
do
{
    Console.Write("City: ");
    city = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(city))
        Console.WriteLine("City cannot be empty.");
} while (string.IsNullOrWhiteSpace(city));

int age;
while (true)
{
    Console.Write("Age: ");
    string ageInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(ageInput))
    {
        Console.WriteLine("Age cannot be empty.");
        continue;
    }
    if (!int.TryParse(ageInput, out age))
    {
        Console.WriteLine("Age must be a number, not characters.");
        continue;
    }
    if (age <= 0)
    {
        Console.WriteLine("Age must be a positive number.");
        continue;
    }
    break;
}

string phone;
while (true)
{
    Console.Write("Phone: ");
    string phoneInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(phoneInput))
    {
        Console.WriteLine("Phone cannot be empty.");
        continue;
    }
    if (!phoneInput.All(char.IsDigit))
    {
        Console.WriteLine("Phone must contain only digits.");
        continue;
    }
    phone = phoneInput;
    break;
}

var person = new { Username = username, City = city, Age = age, PhoneNumber = phone };
var json = JsonSerializer.Serialize(person);
using var httpClient = new HttpClient();
var response = await httpClient.PostAsync(
    "https://localhost:7054/Cupid/register",
    new StringContent(json, System.Text.Encoding.UTF8, "application/json")
);
Console.WriteLine(await response.Content.ReadAsStringAsync());

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7054/cupidHub")
    .WithAutomaticReconnect()
    .Build();

connection.On<string, string, string>("ReceiveLetter", async (from, message, phoneNumber) =>
{
    Console.WriteLine("\n=== New Letter ===");
    Console.WriteLine($"From: {from}");
    Console.WriteLine($"Message: {message}");
    if (!string.IsNullOrEmpty(phoneNumber))
        Console.WriteLine($"Phone: {phoneNumber}");

    string cmd;
    while (true)
    {
        Console.WriteLine("Type /ok to confirm or /block <username> to block:");
        cmd = Console.ReadLine()?.Trim();

        if (cmd == "/ok")
        {
            await httpClient.PostAsync($"https://localhost:7054/Cupid/confirm?username={username}", null);
            break;
        }
        else if (cmd.StartsWith("/block "))
        {
            var parts = cmd.Split(' ');
            if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]))
            {
                await httpClient.PostAsync($"https://localhost:7054/Cupid/block?username={username}&blockUser={parts[1]}", null);
                await httpClient.PostAsync($"https://localhost:7054/Cupid/confirm?username={username}", null);
                break;
            }
            else
            {
                Console.WriteLine("Invalid format. Use: /block <username>");
            }
        }
        else
        {
            Console.WriteLine("Unknown command. Please type /ok or /block <username>.");
        }
    }
});
await connection.StartAsync();
await connection.InvokeAsync("RegisterUser", username);
Console.WriteLine("Waiting for letters...");
await Task.Delay(-1);
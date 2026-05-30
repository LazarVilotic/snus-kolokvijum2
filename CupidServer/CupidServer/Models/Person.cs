namespace CupidServer.Models;

public class Person
{
    public string Username { get; set; }
    public string City { get; set; }
    public int Age { get; set; }
    public string PhoneNumber { get; set; }

    public bool WaitingForConfirmation { get; set; } = false;

    public List<string> BlockedUsers { get; set; } = new();
}
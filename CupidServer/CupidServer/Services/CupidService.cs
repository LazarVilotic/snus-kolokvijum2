using CupidServer.Models;
using CupidServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Cryptography;

namespace CupidServer.Services
{
    public class CupidService
    {
        private readonly List<Person> _persons = new();
        private readonly IHubContext<CupidHub> _hubContext;
        private Timer _timer;

        public CupidService(IHubContext<CupidHub> hubContext)
        {
            _hubContext = hubContext;
            _timer = new Timer(SendLetters, null, 10000, 60000);
        }

        public void RegisterPerson(Person person)
        {
            _persons.Add(person);
        }

        public List<Person> GetAllPersons() => _persons;

        private void SendLetters(object state)
        {
            foreach (var receiver in _persons)
            {
                if (receiver.WaitingForConfirmation) continue;

                var possibleSenders = _persons
                    .Where(s => s.Username != receiver.Username
                             && !receiver.BlockedUsers.Contains(s.Username))
                    .ToList();

                if (!possibleSenders.Any()) continue;

                Person bestSender = null;
                int bestScore = -1;

                foreach (var sender in possibleSenders)
                {
                    int score = CalculateScore(sender, receiver);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestSender = sender;
                    }
                }

                if (bestSender == null) continue;

                string message = GenerateRandomMessage();
                string phone = message == "Nisam zainteresovan/a za upoznavanje."
                    ? ""
                    : bestSender.PhoneNumber;

                receiver.WaitingForConfirmation = true;

                _hubContext.Clients.Group(receiver.Username)
                    .SendAsync("ReceiveLetter", bestSender.Username, message, phone);
            }
        }

        private string GenerateRandomMessage()
        {
            string[] messages =
            {
                "Radujem se našem susretu!",
                "Želim da se upoznamo.",
                "Nisam zainteresovan/a za upoznavanje."
            };
            return messages[GetSecureRandom(0, messages.Length)];
        }

        private int CalculateScore(Person a, Person b)
        {
            int score = 0;
            if (a.City.ToLower() == b.City.ToLower()) score += 30;
            if (Math.Abs(a.Age - b.Age) <= 2) score += 20;
            score += GetSecureRandom(0, 101);
            return score;
        }

        // Helper metoda koja koristi RNGCryptoServiceProvider
        private int GetSecureRandom(int minInclusive, int maxExclusive)
        {
            using var rng = RandomNumberGenerator.Create(); 
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            int value = Math.Abs(BitConverter.ToInt32(bytes, 0));
            return minInclusive + (value % (maxExclusive - minInclusive));
        }

        public void ConfirmLetter(string username)
        {
            var user = _persons.FirstOrDefault(x => x.Username == username);
            if (user != null)
                user.WaitingForConfirmation = false;
        }

        public void BlockUser(string username, string toBlock)
        {
            var user = _persons.FirstOrDefault(x => x.Username == username);
            if (user != null && !user.BlockedUsers.Contains(toBlock))
            {
                user.BlockedUsers.Add(toBlock);
                user.WaitingForConfirmation = false; 
            }
        }
    }
}
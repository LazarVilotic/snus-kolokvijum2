# 💘 Chaotic Cupid

A real-time PubSub matchmaking simulation built with **ASP.NET Core** + **SignalR**. Players register and wait for Cupid to send them love letters — matched by location, age, and a dash of chaos.

---

## 📐 Architecture

```
┌─────────────────┐        SignalR         ┌──────────────────────┐
│   CupidClient   │ ◄──────────────────── │    CupidServer       │
│  (Console App)  │                        │  ASP.NET Core + Hub  │
│                 │ ──── REST (register,   │                      │
│                 │       confirm, block)─►│  CupidService        │
└─────────────────┘                        │  (Timer + Scoring)   │
                                           └──────────────────────┘
```

- **Server** — ASP.NET Core Web API + SignalR Hub. Manages registered players, runs Cupid's timer, and pushes letters in real time.
- **Client** — Console app. Registers the player, listens for incoming letters via SignalR, and handles user commands.

---

## ✨ Features

- 👤 **Player registration** — enter username, city, age, and phone number with full input validation
- 💌 **Automatic letters** — Cupid sends one letter per minute to every registered player
- 🧮 **Smart matching algorithm:**
  - Same city → +30 points
  - Similar age (±2 years) → +20 points
  - Cryptographic random factor → +0–100 points (`RNGCryptoServiceProvider`)
  - The player with the **highest score** gets the letter
- 🎲 **Random messages** — each letter includes one of three randomly picked messages
- 📵 **Phone number hidden** when the message is *"Nisam zainteresovan/a za upoznavanje."*
- ⏸️ **Confirmation gate** — a player cannot receive a new letter until they confirm the previous one
- 🚫 **Block command** — players can block specific senders

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Run the Server

```bash
cd CupidServer
dotnet run
```

Server starts on `https://localhost:7054`.

### Run the Client (one per player)

```bash
cd CupidClient
dotnet run
```

Open multiple terminal windows to simulate multiple players.

---

## 🕹️ Client Usage

On startup you will be prompted for:

```
Username: alice
City: Belgrade
Age: 25
Phone: 0641234567
```

**Validation rules:**
- No field can be empty
- Age must be a positive integer (no characters, no negative numbers)
- Phone must contain digits only

Once registered, the client waits for incoming letters:

```
=== New Letter ===
From: bob
Message: Radujem se našem susretu!
Phone: 0649876543
Type /ok to confirm or /block <username> to block:
```

**Available commands:**

| Command | Description |
|---|---|
| `/ok` | Confirm you received the letter and allow new ones |
| `/block <username>` | Block that user and allow new letters |

Any invalid command will prompt you to try again — the app will not freeze.

---

## 📁 Project Structure

```
CupidServer/
├── Controllers/
│   └── CupidController.cs   # REST endpoints: register, confirm, block
├── Hubs/
│   └── CupidHub.cs          # SignalR hub
├── Models/
│   ├── Person.cs            # Player model
│   └── Letter.cs            # Letter model
├── Services/
│   └── CupidService.cs      # Matching logic, timer, RNG scoring
└── Program.cs

CupidClient/
└── Program.cs               # Console client with SignalR listener
```

---

## 🔌 REST API

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/Cupid/register` | Register a new player |
| `POST` | `/Cupid/confirm?username=` | Confirm letter received |
| `POST` | `/Cupid/block?username=&blockUser=` | Block a specific user |
| `GET` | `/Cupid/all` | List all registered players |

---

## 🔒 Notes on Randomness

The random scoring factor uses `RandomNumberGenerator.Create()` (the modern .NET equivalent of `RNGCryptoServiceProvider`) to ensure cryptographically secure randomness — not `System.Random`.

```csharp
private int GetSecureRandom(int minInclusive, int maxExclusive)
{
    using var rng = RandomNumberGenerator.Create();
    byte[] bytes = new byte[4];
    rng.GetBytes(bytes);
    int value = Math.Abs(BitConverter.ToInt32(bytes, 0));
    return minInclusive + (value % (maxExclusive - minInclusive));
}
```

---

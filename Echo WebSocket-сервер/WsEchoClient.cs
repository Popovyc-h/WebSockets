using System.Net.WebSockets;
using System.Text;

using var ws = new ClientWebSocket();

// Підключаємось — тут відбувається Handshake
await ws.ConnectAsync(new Uri("ws://localhost:8181/"), CancellationToken.None);
Console.WriteLine($"З'єднано! Стан: {ws.State}\n");

var buffer = new byte[4096];
string[] messages = ["Привіт, сервере!", "WebSockets — це двосторонній канал", "До побачення!"];

foreach (var msg in messages)
{
    // Надіслати текстове повідомлення
    var bytes = Encoding.UTF8.GetBytes(msg);
    await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    Console.WriteLine($"→ Надіслано:  {msg}");

    // Отримати відповідь
    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    var reply = Encoding.UTF8.GetString(buffer, 0, result.Count);
    Console.WriteLine($"← Отримано:   {reply}");
    Console.WriteLine();
}

// Надіслати Close Frame — коректне закриття
await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
Console.WriteLine($"З'єднання закрито. Стан: {ws.State}");

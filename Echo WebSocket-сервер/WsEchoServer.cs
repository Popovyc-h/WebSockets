using System.Net;
using System.Net.WebSockets;
using System.Text;

var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:8181/");
listener.Start();
Console.WriteLine("WebSocket сервер запущено: ws://localhost:8181/");
Console.WriteLine("Очікуємо клієнтів...\n");

while (true)
{
    // Чекаємо HTTP-запит
    var httpContext = await listener.GetContextAsync();

    // Якщо це не WebSocket-запит — відхиляємо
    if (!httpContext.Request.IsWebSocketRequest)
    {
        httpContext.Response.StatusCode = 400;
        httpContext.Response.Close();
        Console.WriteLine("Відхилено: не WebSocket-запит");
        continue;
    }

    Console.WriteLine($"Новий клієнт підключився: {httpContext.Request.RemoteEndPoint}");

    // HttpListener сам виконує WebSocket Handshake (101 Switching Protocols)
    var wsContext = await httpContext.AcceptWebSocketAsync(subProtocol: null);
    var ws = wsContext.WebSocket;

    var buffer = new byte[4096];

    // Обробляємо повідомлення в циклі поки з'єднання відкрите
    while (ws.State == WebSocketState.Open)
    {
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            // Клієнт надіслав Close Frame — відповідаємо і закриваємо
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", CancellationToken.None);
            Console.WriteLine("Клієнт закрив з'єднання\n");
            break;
        }

        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        Console.WriteLine($"  ← Отримано: {message}");

        // Відправляємо echo-відповідь
        var reply = $"[Echo]: {message}";
        var replyBytes = Encoding.UTF8.GetBytes(reply);
        await ws.SendAsync(
            new ArraySegment<byte>(replyBytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            CancellationToken.None
        );
        Console.WriteLine($"  → Відправлено: {reply}");
    }
}

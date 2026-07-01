As part of learning SignalR - Part 3

Project 3 : Realt-Time Stock Price Monitor


Difficulty : Advanced


I built a financial dashboard where three user roles receive fundamentally different live data streams over authenticated WebSocket connections. 

Admins control the feed. 

Traders get full real-time prices. 

Viewers get a 15-second delayed feed. 

The Hub enforces these boundaries at the server — clients cannot circumvent them.


WHAT'S NEW ?


Project 3 introduces the three things:

Authentication: WebSocket connections carry a JWT bearer token. The server validates it before any Hub method runs. Unauthenticated connection attempts are rejected at the transport layer before they reach any Hub code.

Authorization: Hub methods and the Hub class itself are decorated with [Authorize] and [Authorize(Roles = "...")] attributes. 
A trader calling an admin-only method receives a forbidden response — not broken behavior or silent failure.

Streaming: Instead of fire-and-forget broadcasts, the Hub uses IAsyncEnumerable<T> — a server-to-client stream that pushes data continuously until the client cancels it. 

Load Balancing: "Channel-T" a thread-safe, high-performance producer-consumer queue,
Making the Hub and IAsyncEnumerabke  threads to communicate safely .

The server produces at a rate it controls and the client consumes at its own pace.

![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/1fab0c5847084e712d226ac2d6cb93100c07ef84/Screenshot%202026-06-25%20111520.png)
![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/405e8d9dd4fdcd05887d8c68c7b584a05ba5b0e5/Screenshot%20(69).png)
![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/3757b302bf02866edfe03457e31ebcd8f281c050/Screenshot%202026-06-25%20111552.png)
![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/af84dcd3ddf52888011972ed2cf7da11702a80b3/Screenshot%202026-06-25%20111628.png)


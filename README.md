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

HOW TO TEST

Requirements
Visual Studio
Visual Studion Code + .NET 8 SDK
A web browser


Clone the project, and "dotnet run" in VS Code terminal



Scenario 1 — Trader Live Feed

Go to http://localhost:5000. Log in as alice / alice123. 

You're redirected to trader.html. Header shows "Alice Chen [Trader]" and the green dot.

Select "AAPL" from the dropdown. Click Start Live Feed. 

A price card appears immediately with bid, ask, volume, and change data updating every 500ms. 

The feed log shows ticks every 2.5 seconds.

Open a second browser window, log in as bob / bob123. Select "AAPL" and start stream. 

Both traders now see the same live prices simultaneously — different connections, same channel output.

![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/1fab0c5847084e712d226ac2d6cb93100c07ef84/Screenshot%202026-06-25%20111520.png)
![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/405e8d9dd4fdcd05887d8c68c7b584a05ba5b0e5/Screenshot%20(69).png)
![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/3757b302bf02866edfe03457e31ebcd8f281c050/Screenshot%202026-06-25%20111552.png)
![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/af84dcd3ddf52888011972ed2cf7da11702a80b3/Screenshot%202026-06-25%20111628.png)


Scenario 2 — Viewer Delay Enforcement

Open a third window. Log in as viewer / viewer123. 

You're redirected to viewer.html. Header shows yellow dot and "Viewer" role.

Select "AAPL" and click Subscribe. 

The display shows " Waiting 15 seconds for first delayed tick..." Nothing appears for 15 seconds. Compare to the trader windows which are updating constantly.


After 15 seconds, a price appears. Note: the "Delay" field in the viewer display shows ~15s. 

Note the Timestamp vs DeliveredAt fields confirm the delay. The viewer only sees Price — no bid/ask/volume — exactly as designed.


![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/579ea3a01a3e02eb5de308cb41060077f9fb4673/Screenshot%202026-06-25%20111729.png)
![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/ceebee4d7e82c1f75a495e3abf8a9d39c68352b2/Screenshot%202026-06-25%20111745.png)
![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/752cbb71bd714729eb0b11550e5a379c94d43db6/Screenshot%20(70).png)


Scenario 4 — Admin Control Broadcast

Open a fourth window. Log in as admin / admin123. 

You're redirected to admin.html. The admin can see both live and delayed streams for monitoring.


On the admin dashboard, find the Feed Control section. Enter command "HALT" and message "System maintenance in 5 minutes." Click Send Control Command.


Watch ALL other windows simultaneously. The "FeedControlEvent" handler fires on every connected client — traders, viewers, and other admin windows all display the alert banner: "[Admin Morgan Hayes] HALT: System maintenance in 5 minutes."


To verify admin-only access: on a trader tab console, type connection.invoke("ControlFeed", "TEST", "test"). It should throw "Authorization failed" — traders cannot call [Authorize(Roles = "Admin")] methods.
![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/c2d0fa6d58509e7d060ce94584bf96fc5208b940/Screenshot%20(71).png)
![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/46a3abde6a7f72d7617164d72939803e4f6e1b6f/Screenshot%20(72).png)
![](https://github.com/Sbomelo/Real-Time-Stock-Price-Monitor/blob/e0e12fea1f5fd519df0ae632a28aea54249cac65/Screenshot%20(73).png)

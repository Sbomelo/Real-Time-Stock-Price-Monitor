As part of learning SignalR - Part 3
I built a financial dashboard where three user roles receive fundamentally different live data streams over authenticated WebSocket connections. 
Admins control the feed. 
Traders get full real-time prices. 
Viewers get a 15-second delayed feed. 
The Hub enforces these boundaries at the server — clients cannot circumvent them.

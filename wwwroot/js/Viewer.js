const jwt = sessionStorage.getItem("jwt");
if (!jwt) location.href = "/";

let sub = null;

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/stockHub", { accessTokenFactory: () => sessionStorage.getItem("jwt") })
    .withAutomaticReconnect().configureLogging(signalR.LogLevel.Warning).build();

connection.on("Connected", (id, name, role) => {
    document.getElementById("dot").classList.add("live");
    document.getElementById("cst").textContent = `${name} [${role}]`;
});

document.getElementById("startBtn").addEventListener("click", () => {
    if (sub) { sub.dispose(); }
    const sym = document.getElementById("sym").value;
    // Viewers call StreamDelayedPrices — NOT StreamPrices
    // If they tried to call StreamPrices, the server would return 403 Forbidden
    sub = connection.stream("StreamDelayedPrices", sym)           
        .subscribe({
            next: (tick) => {
                document.getElementById("display").innerHTML = `
                    <div class="pd-sym">${tick.symbol} — DELAYED FEED</div>
                    <div class="pd-price">$${tick.price.toFixed(2)}</div>
                    <div class="pd-times">
                      Price captured: ${new Date(tick.timestamp).toLocaleTimeString()}<br>
                      Delivered to you: ${new Date(tick.deliveredAt).toLocaleTimeString()}<br>
                      Delay: ~${Math.round((new Date(tick.deliveredAt)-new Date(tick.timestamp))/1000)}s
                    </div>`;
            },
            error: (err) => {
                document.getElementById("display").innerHTML =
                    `<div class="pd-waiting" style="color:#f87171">Error: ${err.message}</div>`;
            }
        });
        
    document.getElementById("display").innerHTML =
        `<div class="pd-waiting">⏱ Waiting 15 seconds for first delayed tick...</div>`;
});

async function start() {
    try { await connection.start(); }
    catch(e) { document.getElementById("cst").textContent="Auth failed"; }
}
start();
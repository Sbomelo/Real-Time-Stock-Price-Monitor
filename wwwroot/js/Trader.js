//Guard: redirect to login if no JWT
const jwt = sessionStorage.getItem("jwt");
if (!jwt) { location.href = "/"; }                         
document.getElementById("uname").textContent =
    sessionStorage.getItem("name") || "Trader";


let subscription = null;
let tickCount = 0;
const cards = new Map();   // symbol → DOM card element

//Build authenticated connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/stockHub", {
        accessTokenFactory: () => sessionStorage.getItem("jwt")
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build();

connection.on("Connected", (id, name, role) => {               
    document.getElementById("dot").classList.add("live");
    document.getElementById("connst").textContent = `Live [${role}]`;
});

connection.on("FeedControlEvent", (cmd, msg, admin) => {         
    const el = document.getElementById("alert");
    el.className = "alert-banner alert-warn show";
    el.textContent = `[Admin ${admin}] ${cmd}: ${msg}`;
});

connection.onreconnected(async () => {                       
    document.getElementById("connst").textContent = "Reconnected";

    // Restart stream if one was active before disconnection
    if (subscription) {
        subscription.dispose();
        subscription = null;
        startStream();                                         
    }
});

async function startStream() {
    const sym = document.getElementById("sym").value;
    subscription = connection.stream("StreamPrice", sym)     
        .subscribe({
            next: (tick) => {
                updateCard(tick);
                logTick(tick);
            },
            error: (err) => {
                const el = document.getElementById("alert");
                el.className = "alert-banner alert-warn show";
                el.textContent = "Stream error: " + err.message;
            },
            complete: () => console.log("Stream completed")
        });
    document.getElementById("startBtn").style.display = "none";
    document.getElementById("stopBtn").style.display  = "inline";
}

document.getElementById("startBtn").addEventListener("click", startStream);

document.getElementById("stopBtn").addEventListener("click", () => {
    if (subscription) { subscription.dispose(); subscription = null; } 
    document.getElementById("startBtn").style.display = "inline";
    document.getElementById("stopBtn").style.display  = "none";
});

function updateCard(tick) {
    let card = cards.get(tick.symbol);
    if (!card) {
        card = document.createElement("div");
        card.className = "ticker-card";
        document.getElementById("grid").appendChild(card);
        cards.set(tick.symbol, card);
    }

    const sign = tick.changePct >= 0 ? "+" : "";
    const cls  = tick.changePct >= 0 ? "up" : "dn";

    card.innerHTML = `
      <div class="tc-symbol">${tick.symbol}</div>
      <div class="tc-price">$${tick.price.toFixed(2)}</div>
      <div class="tc-change ${cls}">${sign}${tick.changePct.toFixed(3)}%</div>
      <div class="tc-detail">
        Bid: $${tick.bid.toFixed(2)} | Ask: $${tick.ask.toFixed(2)}<br>
        Vol: ${tick.volume.toLocaleString()} | Chg: ${sign}$${Math.abs(tick.change).toFixed(2)}
      </div>`;
}

function logTick(tick) {
    tickCount++;
    if (tickCount % 5 !== 0) return;   // Log every 5th tick to keep log readable
    const log = document.getElementById("log");
    const t = new Date(tick.timestamp).toLocaleTimeString();
    const e = document.createElement("div");
    e.className = "log-entry";
    e.innerHTML = `[${t}] <span class="sym">${tick.symbol}</span> <span class="pr">$${tick.price.toFixed(2)}</span>`;
    log.insertBefore(e, log.firstChild);
    if (log.children.length > 50) log.lastChild.remove();
}

async function start() {
    try { 
        await connection.start(); 
    }
    catch (e) {
        document.getElementById("connst").textContent = "Auth failed — "  + e.message;
        setTimeout(start, 5000);
    }
}
start();

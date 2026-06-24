// Guard: redirect to login if no JWT or not Admin
const jwt  = sessionStorage.getItem("jwt");
const role = sessionStorage.getItem("role");
if (!jwt || role !== "Admin") { location.href = "/"; }

document.getElementById("uname").textContent =
    sessionStorage.getItem("name") || "Admin";

let subscription = null;
const cards = new Map();

// Build authenticated connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/stockHub", {
        accessTokenFactory: () => sessionStorage.getItem("jwt")
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build();

// Server confirmed connection
connection.on("Connected", (id, name, role) => {
    document.getElementById("dot").classList.add("live");
    document.getElementById("connst").textContent = `Live [${role}]`;
});

// Listen for broadcast events (including ones this admin sends)
connection.on("FeedControlEvent", (cmd, msg, admin) => {
    appendLog(cmd, msg, admin);
});

connection.onreconnected(async () => {
    document.getElementById("connst").textContent = "Reconnected";
    if (subscription) {
        subscription.dispose();
        subscription = null;
        startStream();
    }
});

//Broadcast
document.getElementById("sendBtn").addEventListener("click", async () => {
    const cmd = document.getElementById("cmd").value;
    const msg = document.getElementById("msg").value.trim();

    if (!msg) {
        showAlert("Enter a message before broadcasting.", "warn");
        return;
    }

    try {
        await connection.invoke("ControlFeed", cmd, msg);
        document.getElementById("msg").value = "";
        showAlert(`Broadcast sent: [${cmd}] ${msg}`, "ok");
    } catch (e) {
        showAlert("Broadcast failed: " + e.message, "warn");
    }
});

//Live price stream (admin gets full trader-level feed)
function startStream() {
    const sym = document.getElementById("sym").value;
    subscription = connection.stream("StreamPrice", sym)
        .subscribe({
            next: (tick) => updateCard(tick),
            error: (err) => showAlert("Stream error: " + err.message, "warn"),
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

//DOM helpers
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

function appendLog(cmd, msg, admin) {
    const log = document.getElementById("log");

    // Remove the "no broadcasts yet" placeholder if it's still there
    const empty = log.querySelector(".log-empty");
    if (empty) empty.remove();

    const t = new Date().toLocaleTimeString();
    const entry = document.createElement("div");
    entry.className = "log-entry";
    entry.innerHTML =
        `<span class="time">[${t}]</span> ` +
        `<span class="cmd">${cmd}</span> ` +
        `"${msg}" — <span class="who">${admin}</span>`;

    log.insertBefore(entry, log.firstChild);
    if (log.children.length > 50) log.lastChild.remove();
}

function showAlert(text, type) {
    const el = document.getElementById("alert");
    el.className = `alert-banner alert-${type} show`;
    el.textContent = text;
    setTimeout(() => { el.className = "alert-banner"; }, 4000);
}

//Start connection 
async function start() {
    try {
        await connection.start();
    } catch (e) {
        document.getElementById("connst").textContent = "Auth failed — " + e.message;
        setTimeout(start, 5000);
    }
}
start();
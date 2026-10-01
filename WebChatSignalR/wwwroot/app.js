// SignalR-klient för chattgränssnittet.
// Servern sätter en cookie efter inloggning, och hubben läser användarens identitet därifrån.
// Text från servern renderas med textContent för att undvika att HTML körs i webbläsaren.

const statusEl = document.getElementById("status");
const messagesEl = document.getElementById("messages");
const loginForm = document.getElementById("login-form");
const loginNameEl = document.getElementById("login-name");
const logoutBtn = document.getElementById("logout");
const messageEl = document.getElementById("message");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Klientkod är alltid synlig i webbläsaren, så servern måste validera alla anrop.
window.connection = connection;

connection.on("ReceiveMessage", (username, message) => addMessage(username, message));
connection.on("ReceiveSystem", (message) => addMessage(null, message, "system"));

connection.onreconnecting((err) => setStatus("Återansluter...", false));
connection.onreconnected(() => setStatus("Ansluten", true));
connection.onclose((err) => {
    setStatus("Anslutningen stängdes", false);
    setChatEnabled(false);
    if (err) addMessage(null, `Anslutningen stängdes: ${err.message}`, "error");
});

// Säker rendering: allt som kommer från servern behandlas som text.
function addMessage(username, message, cssClass) {
    const li = document.createElement("li");
    if (cssClass) li.className = cssClass;

    if (username) {
        const strong = document.createElement("strong");
        strong.textContent = username;
        li.appendChild(strong);
        li.appendChild(document.createTextNode(": "));
    }

    li.appendChild(document.createTextNode(message));
    messagesEl.appendChild(li);
    li.scrollIntoView();
}

function setStatus(text, connected) {
    statusEl.textContent = text;
    statusEl.className = connected ? "status status--on" : "status status--off";
}

function setChatEnabled(enabled) {
    document.querySelectorAll("#send-form")
        .forEach(f => f.querySelectorAll("input, button").forEach(el => el.disabled = !enabled));
    loginNameEl.disabled = enabled;
    loginForm.querySelector("button[type=submit]").disabled = enabled;
    logoutBtn.hidden = !enabled;
}

// Alla anrop går genom samma hjälpfunktion så att HubException-fel visas för användaren.
async function invoke(method, ...args) {
    try {
        await connection.invoke(method, ...args);
        return true;
    } catch (err) {
        addMessage(null, `Fel: ${err.message}`, "error");

        // En HubException bär bara text. Servern skriver antalet sekunder i meddelandet,
        // så vi läser ut det och pausar skickaknapparna så länge. Ett större system skulle
        // skicka en felkod och ett tal i stället för att klienten tolkar text.
        const retry = err.message.match(/Försök igen om (\d+) sekunder/);
        if (retry) pauseSending(Number(retry[1]));

        return false;
    }
}

let resumeTimer;

function pauseSending(seconds) {
    const buttons = document.querySelectorAll("#send-form button");
    buttons.forEach(b => b.disabled = true);

    clearTimeout(resumeTimer);
    resumeTimer = setTimeout(() => {
        // Återaktivera bara om vi fortfarande är anslutna.
        if (connection.state === signalR.HubConnectionState.Connected) {
            buttons.forEach(b => b.disabled = false);
            addMessage(null, "Du kan skicka igen.", "system");
        }
    }, seconds * 1000);
}

loginForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    const response = await fetch("/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name: loginNameEl.value }),
    });

    if (!response.ok) {
        const body = await response.json().catch(() => ({}));
        addMessage(null, `Inloggning misslyckades: ${body.error ?? response.status}`, "error");
        return;
    }

    const user = await response.json();
    setStatus(`Ansluter som ${user.name}...`, false);

    try {
        await connection.start();
        setStatus(`Ansluten som ${user.name}`, true);
        setChatEnabled(true);
        messageEl.focus();
    } catch (err) {
        setStatus("Anslutning misslyckades", false);
        addMessage(null, `Anslutning misslyckades: ${err.message}`, "error");
    }
});

logoutBtn.addEventListener("click", async () => {
    await connection.stop();
    await fetch("/logout", { method: "POST" });
    setStatus("Ej inloggad", false);
    setChatEnabled(false);
});

document.getElementById("send-form").addEventListener("submit", async (event) => {
    event.preventDefault();
    if (await invoke("SendMessage", messageEl.value)) messageEl.value = "";
});

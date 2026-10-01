

const statusEl = document.getElementById("status");
const messagesEl = document.getElementById("messages");
const loginForm = document.getElementById("login-form");
const loginNameEl = document.getElementById("login-name");
const logoutBtn = document.getElementById("logout");
const messageEl = document.getElementById("message");


//Skapa signalR anslutningen.
//Klienten försöker koppla upp sig mot en server-hub på /hubs/chat

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// För debbuging i dev console.
window.connection = connection;

//När servern anropar ReceiveMessage på klienten, kör funktionen addMessage.
connection.on("ReceiveMessage", (username, message) => addMessage(username, message));
//Servern skickar systemmeddelande
connection.on("ReceiveSystem", (message) => addMessage(null, message, "system"));

//Anslutning
connection.onreconnecting((err) => setStatus("Återansluter...", false));
connection.onreconnected(() => setStatus("Ansluten", true));
connection.onclose((err) => {
    setStatus("Anslutningen stängdes", false);
    setChatEnabled(false);
    if (err) addMessage(null, `Anslutningen stängdes: ${err.message}`, "error");
});

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

//Visar status
function setStatus(text, connected) {
    statusEl.textContent = text;
    statusEl.className = connected ? "status status--on" : "status status--off";
}

//Styr när chatten ska vara enabled/disabled
function setChatEnabled(enabled) {
    document.querySelectorAll("#send-form")
        .forEach(f => f.querySelectorAll("input, button").forEach(el => el.disabled = !enabled));
    loginNameEl.disabled = enabled;
    loginForm.querySelector("button[type=submit]").disabled = enabled;
    logoutBtn.hidden = !enabled;
}

//Hjälpfunktion för att anropa metoder på servern.
async function invoke(method, ...args) {
    try {
        await connection.invoke(method, ...args);
        return true;
    } catch (err) {
        //Felet visas i chatten.
        addMessage(null, `Fel: ${err.message}`, "error");

        const retry = err.message.match(/Försök igen om (\d+) sekunder/);
        if (retry) pauseSending(Number(retry[1]));

        return false;
    }
}

let resumeTimer;

//Stänger av skickaknappen under visst antal sekunder
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

//Inloggning

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

//Utloggning
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

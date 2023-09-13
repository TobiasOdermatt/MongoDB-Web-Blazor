function addToLocalStorage(key, value) { localStorage[key] = value; }
function readLocalStorage(key) { return localStorage[key]; }

function setCookie(name, value, days) {
    let expires = "";
    if (days) {
        let date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}

function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

function downloadURI (uri, name) {
    const link = document.createElement("a");
    link.download = name;
    link.href = uri;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

async function startConnection() {
    const connection = new signalR.HubConnectionBuilder().withUrl("/progressHub").build();

    connection.on("ReceiveProgressDatabase", function (totalCollections, processedCollections, progress) {
        const progressBar = document.getElementById("fileProgress");
        if (progressBar) {
            progressBar.value = progress;
        }

        const progressText = document.getElementById("status-text");
        if (progressText) {
            progressText.innerHTML = processedCollections + " / " + totalCollections + " collections";
        }
    });

    connection.on("ReceiveProgressCollection", function (totalDocuments, processedDocuments, progress) {
        const progressBar = document.getElementById("fileProgress");
        if (progressBar) {
            progressBar.value = progress;
        }

        const progressText = document.getElementById("status-text");
        if (progressText) {
            progressText.innerHTML = processedDocuments + " / " + totalDocuments + " documents";
        }
    });

    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(startConnection, 5000);
    }
}

startConnection();


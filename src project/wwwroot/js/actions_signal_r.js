async function startConnection() {
    const connection = new signalR.HubConnectionBuilder().withUrl("/progressHub").build();

    connection.on("ReceiveProgressDatabase", function (totalCollections, processedCollections, progress, guid, actionType) {
        displayStatusCollectionDatabase(totalCollections, processedCollections, progress, guid, "collections", actionType)
    });

    connection.on("ReceiveProgressCollection", function (totalDocuments, processedDocuments, progress, guid, actionType) {
        displayStatusCollectionDatabase(totalDocuments, processedDocuments, progress, guid, "documents", actionType)
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

function changeProgressBar(progress, guid) {
    const progressBar = document.querySelector(`[data-guid="${guid}"][id="ProgressBar"]`);
    progressBar.classList.remove("d-none")
    progressBar.classList.add("d-block")
    progressBar.value = progress;
}

function displayStatusCollectionDatabase(total, processed, progress, guid, type, actionType) {
    const progressText = document.querySelector(`[data-guid="${guid}"][id="status-text"]`);
    progressText.innerHTML = processed + " / " + total + " " + type;
    changeProgressBar(progress, guid);
    if (progress == 100) {
        changeMessage(actionType);
    }
}

function changeMessage(actionType) {
    const text = document.querySelector(`[data-guid="${guid}"][id="text"]`);
    if (actionType == "download") {
        text.innerHTML = "Download started"
        text.classList.add("text-success");
    }
}

async function startConnection() {
    const connection = new signalR.HubConnectionBuilder().withUrl("/progressHub").build();

    connection.on("ReceiveProgressDatabase", function (totalCollections, processedCollections, progress, guid) {
        displayStatus(totalCollections, processedCollections, progress, guid, "collections")
    });

    connection.on("ReceiveProgressCollection", function (totalDocuments, processedDocuments, progress, guid) {
        displayStatus(totalDocuments, processedDocuments, progress, guid, "documents")
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

function displayStatus(total, processed, progress, guid, type) {
    const progressBar = document.querySelector(`[data-guid="${guid}"][id="fileProgress"]`);
    const progressText = document.querySelector(`[data-guid="${guid}"][id="status-text"]`);
    progressBar.value = progress;
    progressText.innerHTML = processed + " / " + total + " " + type;

    if (progress == 100) {
        const text = document.querySelector(`[data-guid="${guid}"][id="text"]`);
        text.innerHTML = "Download started"
        text.classList.add("text-success");
    }
}

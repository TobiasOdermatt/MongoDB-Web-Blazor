async function startConnection() {
    const connection = new signalR.HubConnectionBuilder().withUrl("/progressHub").build();

    connection.on("ReceiveProgressDatabase", function (totalCollections, processedCollections, progress, guid) {

        const progressBar = getProgressElement(guid).progressBar;
        const progressText = getProgressElement(guid).progressTest;
        progressBar.value = progress;
        progressText.innerHTML = processedCollections + " / " + totalCollections + " collections";

    });

    connection.on("ReceiveProgressCollection", function (totalDocuments, processedDocuments, progress, guid) {

        const progressBar = getProgressElement(guid);
        const progressText = getProgressElement(guid);
        progressBar.value = progress;
        progressText.innerHTML = processedDocuments + " / " + totalDocuments + " documents";

    });

    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(startConnection, 5000);
    }
}

function getProgressElement(guid) {
    const progressBar = document.querySelector(`[data-guid="${guid}"][id="fileProgress"]`);
    const progressText = document.querySelector(`[data-guid="${guid}"][id="status-text"]`);
    return { progressBar, progressText };
}
startConnection();

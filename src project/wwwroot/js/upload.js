window.dragDropFunctions = {
    addEventListeners: function(dropAreaId, fileInputId, fileNameId) {
        const dropArea = document.getElementById(dropAreaId);
        const fileInput = document.getElementById(fileInputId);
        const fileName = document.getElementById(fileNameId);

        dropArea.addEventListener('dragover', (e) => {
            e.preventDefault();
            dropArea.classList.add('file-upload-drag');
        });

        dropArea.addEventListener('dragleave', () => {
            dropArea.classList.remove('file-upload-drag');
        });

        dropArea.addEventListener('drop', (e) => {
            e.preventDefault();
            dropArea.classList.remove('file-upload-drag');
            const droppedFiles = e.dataTransfer.files;
            const fileType = droppedFiles[0].name.split('.').pop();

            if (fileType !== 'json') {
                fileName.textContent = 'Not allowed filetype';
            } else {
                fileInput.files = droppedFiles;
                fileName.textContent = droppedFiles[0].name + " (" + (droppedFiles[0].size / (1024 * 1024)).toFixed(2) + " MB)";
            }
        });


        dropArea.addEventListener('click', () => {
            fileInput.click();
        });

        fileInput.addEventListener('change', () => {
            const selectedFile = fileInput.files[0];
            const fileType = selectedFile.name.split('.').pop();

            if (fileType !== 'json') {
                fileName.textContent = 'Not allowed filetype';
            } else {
                fileName.textContent = selectedFile.name + " (" + (selectedFile.size / (1024 * 1024)).toFixed(2) + " MB)";
            }
        });

        dropArea.addEventListener('touchmove', (e) => {
            e.preventDefault();
            dropArea.classList.add('file-upload-drag');
        });

        dropArea.addEventListener('touchend', () => {
            dropArea.classList.remove('file-upload-drag');
        });
    },

    getFile: function(fileInputId) {
        return new Promise((resolve, reject) => {
            const fileInput = document.getElementById(fileInputId);

            if (fileInput.files.length === 0) {
                reject("No file selected.");
                return;
            }

            const file = fileInput.files[0];
            const fileInfo = {
                name: file.name,
                size: file.size,
                type: file.type,
                lastModified: file.lastModified,
                getExtension: () => {
                    const fileNameParts = file.name.split('.');
                    return fileNameParts.length > 1 ? '.' + fileNameParts.pop() : '';
                }
            };

            resolve(fileInfo);
        });
    }
};

async function startChunkUpload(fileInputId, guid) {
    const fileInput = document.getElementById(fileInputId);
    if (fileInput.files.length === 0) {
        console.error("No file selected.");
        return;
    }

    const file = fileInput.files[0];
    const fileName = await uploadChunks(file, guid);
    return fileName;
}

async function uploadChunks(file, guid) {
    const chunkSize = 15 * 1024 * 1024; // 15 MB
    let start = 0;
    let end = chunkSize;
    let chunkIndex = 0;
    const totalChunks = Math.ceil(file.size / chunkSize);

    while (start < file.size) {
        const chunk = file.slice(start, end);
        const formData = new FormData();
        formData.append("file", chunk, file.name);
        formData.append("chunkIndex", chunkIndex.toString());
        formData.append("totalChunks", totalChunks.toString());
        formData.append("guid", guid.toString());

        let progress = Math.round((chunkIndex + 1) / totalChunks * 100);
        displayStatus(totalChunks*15, chunkIndex*15, progress, guid);

        const response = await fetch('/UploadFile', {
            method: 'POST',
            body: formData
        });

        switch (response.status) {
            case 200: // OK
                if (chunkIndex === totalChunks - 1) {
                    console.log("Upload complete");
                    return file.name;
                }
                break;
            case 204: // No Content
                break;
            default:
                console.error(`Error uploading chunk ${chunkIndex}: ${response.statusText}`);
                return;
        }


        start += chunkSize;
        end = Math.min(file.size, start + chunkSize);
        chunkIndex++;
    }
    return file.name;
}

function displayStatus(total, processed, progress, guid) {
    const progressText = document.querySelector(`[data-guid="${guid}"][id="status-text"]`);
    const text = document.querySelector(`[data-guid="${guid}"][id="text"]`);
    progressText.innerHTML = processed+1 + " / " + total + " MB";
    changeProgressBar(progress, guid);
    if (progress == 100) {
        changeMessage(text);
    }
}

function changeProgressBar(progress, guid) {
    const progressBar = document.querySelector(`[data-guid="${guid}"][id="ProgressBar"]`);
    progressBar.classList.remove("d-none")
    progressBar.classList.add("d-block")
    progressBar.value = progress;
}

function changeMessage(text) {
    text.innerHTML = "Upload complete.<br> File will be processed.....";
}
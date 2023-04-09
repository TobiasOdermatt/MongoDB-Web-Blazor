window.dragDropFunctions = {
    addEventListeners: function (dropAreaId, fileInputId, fileNameId) {
        var dropArea = document.getElementById(dropAreaId);
        var fileInput = document.getElementById(fileInputId);
        var fileName = document.getElementById(fileNameId);

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
            fileInput.files = e.dataTransfer.files;
            fileName.textContent = e.dataTransfer.files[0].name;
        });

        dropArea.addEventListener('click', () => {
            fileInput.click();
        });

        fileInput.addEventListener('change', () => {
            fileName.textContent = fileInput.files[0].name;
        });

        dropArea.addEventListener('touchmove', (e) => {
            e.preventDefault();
            dropArea.classList.add('file-upload-drag');
        });

        dropArea.addEventListener('touchend', () => {
            dropArea.classList.remove('file-upload-drag');
        });
    },

        getFile: function (fileInputId) {
            return new Promise((resolve, reject) => {
                var fileInput = document.getElementById(fileInputId);

                if (fileInput.files.length === 0) {
                    reject("No file selected.");
                    return;
                }

                var file = fileInput.files[0];

                var fileInfo = {
                    name: file.name,
                    size: file.size,
                    type: file.type,
                    lastModified: file.lastModified,
                    getExtension: () => {
                        let fileNameParts = file.name.split('.');
                        return fileNameParts.length > 1 ? '.' + fileNameParts.pop() : '';
                    }
                };

                resolve(fileInfo);
            });
    },

    readFileContentsAsByteArray: function (fileInputId) {
        return new Promise((resolve, reject) => {
            var fileInput = document.getElementById(fileInputId);

            if (fileInput.files.length === 0) {
                reject("No file selected.");
                return;
            }

            var file = fileInput.files[0];
            var reader = new FileReader();

            reader.onload = function (e) {
                var arrayBuffer = reader.result;
                var byteArray = new Uint8Array(arrayBuffer);
                resolve(byteArray);
            };

            reader.onerror = function () {
                reject(reader.error);
            };

            reader.readAsArrayBuffer(file);
        });
    }
}
function downloadFile(fileName, fileData, contentType) {
    const blob = new Blob([fileData], { type: contentType });
    const url = window.URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();

    window.URL.revokeObjectURL(url);
}
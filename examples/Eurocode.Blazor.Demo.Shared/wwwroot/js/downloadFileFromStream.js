
    window.downloadFileFromStream = (stream, fileName, contentType) => {
        // Convert the stream (byte array) to a Blob
        const byteArray = new Uint8Array(stream);
    const blob = new Blob([byteArray], {type: contentType });

    // Create a link element to trigger the download
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = fileName;

    // Append the link to the body (necessary for some browsers)
    document.body.appendChild(link);

    // Trigger the click event to download the file
    link.click();

    // Clean up
    document.body.removeChild(link);
    };


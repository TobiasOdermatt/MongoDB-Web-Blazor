function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

function getInputData() {
    const connectForm = document.getElementById('connect');
    let username = connectForm.elements['username'].value;
    let password = connectForm.elements['password'].value;
    return input = username + "@" + password;
}

function randomBinaryString(length) {
    let result = '';
    let characters = '01';
    let charactersLength = characters.length;
    const array = new Uint32Array(length);
    window.crypto.getRandomValues(array);

    for (let i = 0; i < length; i++) {
        if (i % 8 == 0 && i != 0)
            result += ' ';
        result += characters[array[i] % charactersLength];
    }
    return result;
}

//returns UUID if success, false if failed
async function sendDataToServer(authCookieKey, randData) {
    let url = window.location.protocol + '//' + window.location.host + "/api/Auth/CreateOTP";
    let data = { 'AuthCookieKey': authCookieKey, 'RandData': randData };
    try {
        let res = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
        });
        const text = await res.text();
        const dataResult = JSON.parse(text);
        if (dataResult.hasOwnProperty('uuid')) {
            return dataResult["uuid"];
        }
        else { return false; }
    }
    catch (err) {
        return false;
    }
}

function stringToBinary(string) {
    return string.split('').map(function (char) {
        let tempBinaryBlock = char.charCodeAt(0).toString(2);
        return String(tempBinaryBlock).padStart(8, '0');
    }).join(' ');
}

function xoring(a, b, n) {
    let ans = "";
    for (let i = 0; i < n; i++) {
        if (a[i] == " " && b[i] == " ")
            ans += " ";
        
        else if (a[i] == b[i])
            ans += "0";

        else 
            ans += "1";
    }
    return ans;
}

async function Validate(form) {
    let inputString = "Data:" + getInputData();
    let inputBinaryString = stringToBinary(inputString);
    let lengthOfRandomString = parseInt(inputBinaryString.length) - parseInt(inputBinaryString.length / 9);

    let randomDataBinaryString = randomBinaryString(lengthOfRandomString);
    let authCookieBinaryKey = xoring(inputBinaryString, randomDataBinaryString, inputBinaryString.length);

    let successresult = await sendDataToServer(authCookieBinaryKey, randomDataBinaryString);
    if (successresult != false) {
        setCookie("Token", btoa(authCookieBinaryKey), 10);
        setCookie("UUID", successresult, 10);
        document.getElementById('alert-success').style.display = "block"
        document.getElementById('alert-danger').style.display = "none"
        await sleep(200);
        window.location.href = window.location.protocol + '//' + window.location.host + "/Dashboard";
    }
    else {
        document.getElementById('alert-danger').style.display = "block"
        document.getElementById('alert-success').style.display = "none"
    }
}

let qRAuthentication = {};
let qRAuthenticationStatus = {};

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/Hub/QRAuthentication")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("ReceiveDeviceCode", function (deviceCodeResponse) {
    qRAuthentication = deviceCodeResponse;

    generateQrCode(deviceCodeResponse.verificationUriComplete);

    document.getElementById("userCode").innerText = qRAuthentication.userCode;
    document.getElementById("qrCodeWrapper").classList.remove("d-none");
    document.getElementById("qrCodeWrapper").classList.add("d-block");

    const expiration = new Date();
    expiration.setSeconds(expiration.getSeconds() + deviceCodeResponse.expiresIn);

    qRAuthentication.expirationDate = expiration
    qRAuthentication.interval = deviceCodeResponse.interval * 1000;

    checkQRAuthenticationStatus(qRAuthentication.expirationDate);
});

connection.on("ReceiveQRAuthenticationStatus", function (qRAuthenticationStatusResponse) {
    console.log(qRAuthenticationStatusResponse);

    qRAuthenticationStatus = qRAuthenticationStatusResponse;

    if (qRAuthenticationStatus.authorizationSuccessful) {
        document.getElementById("deviceCode").value = qRAuthentication.deviceCode;
        document.getElementById("deviceAuthForm").submit();
    }

    if (qRAuthenticationStatus.authorizationFailed) {
        document.getElementById("qrCodeWrapper").classList.add("d-none");

        document.getElementById("abortMessage").innerText = qRAuthenticationStatus.errorMessage;
        document.getElementById("authAborted").classList.remove("d-none");
        document.getElementById("authAborted").classList.add("d-block");
    }

    if (qRAuthenticationStatus.authorizationPending) {
        setTimeout(checkQRAuthenticationStatus, qRAuthentication.interval);
    }
});

connection.onclose(async () => {
    resetDeviceAuth();
});

async function start() {
    try {
        await connection.start();
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
    connection.invoke("RequestDeviceCode");
}

function resetDeviceAuth() {
    qRAuthentication = {};
    qRAuthenticationStatus = {};

    document.getElementById("qrCodeWrapper").classList.add("d-none");
    document.getElementById("qrCodeWrapper").classList.remove("d-block");
    document.getElementById("authAborted").classList.add("d-none");
    document.getElementById("authAborted").classList.remove("d-block");
}

function retryDeviceAuth() {
    resetDeviceAuth();
    connection.invoke("RequestDeviceCode");
}

function generateQrCode(uri) {
    if (uri == null) {
        return;
    }
    document.getElementById("qrCode").innerHTML = "";

    new QRCode(document.getElementById("qrCode"),
        {
            text: uri,
            width: 200,
            height: 200
        });
}

function checkQRAuthenticationStatus() {
    if (qRAuthentication.expirationDate < new Date()) {
        return;
    }

    connection.invoke("RequestQRAuthenticationStatus", qRAuthentication.deviceCode);
}

window.onload = (event) => {
    start();
};
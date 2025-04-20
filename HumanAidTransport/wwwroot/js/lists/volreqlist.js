setTimeout(function () {
    var successAlert = document.getElementById('success-alert');
    if (successAlert) {
        successAlert.style.display = 'none';
    }

    var errorAlert = document.getElementById('error-alert');
    if (errorAlert) {
        errorAlert.style.display = 'none';
    }
}, 3000);
setTimeout(function () {
    var successAlert = document.querySelector('.alert-success');
    if (successAlert) {
        successAlert.style.transition = "opacity 0.5s";
        successAlert.style.opacity = "0";
        setTimeout(() => successAlert.remove(), 500);
    }
}, 3000);
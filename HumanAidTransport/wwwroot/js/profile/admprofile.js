document.addEventListener("DOMContentLoaded", function () {
    var violationsFilter = document.getElementById("violationsFilter");
    var banFilter = document.getElementById("banFilter");
    var roleFilter = document.getElementById("roleFilter");
    var searchName = document.getElementById("searchName");
    var userCards = document.querySelectorAll(".user-card");

    function filterUsers() {
        var violationsValue = violationsFilter.value;
        var banValue = banFilter.value;
        var roleValue = roleFilter.value;
        var searchValue = searchName.value.trim().toLowerCase();


        userCards.forEach(function (card) {
            var violations = parseInt(card.getAttribute("data-violations"));
            var banned = card.getAttribute("data-banned") === "true";
            var role = card.getAttribute("data-role");
            var name = card.querySelector("h3").textContent.toLowerCase();

            var show = true;

            if (violationsValue === "lt2" && violations >= 2) {
                show = false;
            }
            if (violationsValue === "gte2" && violations < 2) {
                show = false;
            }

            if (banValue === "banned" && !banned) {
                show = false;
            }
            if (banValue === "notbanned" && banned) {
                show = false;
            }

            if (roleValue !== "all" && role !== roleValue) {
                show = false;
            }

            if (searchValue && !name.includes(searchValue)) {
                show = false;
            }

            card.style.display = show ? "block" : "none";
        });
    }

    violationsFilter.addEventListener("change", filterUsers);
    banFilter.addEventListener("change", filterUsers);
    roleFilter.addEventListener("change", filterUsers);
    searchName.addEventListener("input", filterUsers);

    setTimeout(function () {
        var alerts = document.querySelectorAll(".custom-alert");
        alerts.forEach(function (alert) {
            alert.style.transition = "opacity 0.5s";
            alert.style.opacity = "0";
            setTimeout(() => alert.remove(), 500);
        });
    }, 3000);

    filterUsers();
});

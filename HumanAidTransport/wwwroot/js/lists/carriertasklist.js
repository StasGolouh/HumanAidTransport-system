setTimeout(function () {
    document.querySelectorAll('.alert').forEach(alert => {
        alert.style.display = 'none';
    });
}, 3000);

document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("searchTask");
    const statusFilter = document.getElementById("statusFilter");

    // Функція для фільтрації задач
    function filterTasks() {
        const searchText = searchInput.value.toLowerCase(); // Текст для пошуку
        const status = statusFilter.value; // Вибраний статус

        // Перебираємо всі картки замовлень
        document.querySelectorAll(".order-card").forEach(orderCard => {
            const orderName = orderCard.querySelector("h3").innerText.toLowerCase(); // Назва замовлення
            const orderStatus = orderCard.querySelector(".status").innerText; // Статус замовлення

            let showTask = true;

            // Перевірка за текстом пошуку
            if (searchText && !orderName.includes(searchText)) showTask = false;

            // Перевірка за статусом
            if (status && orderStatus !== status) showTask = false;

            // Визначення, чи показати картку замовлення
            orderCard.style.display = showTask ? "block" : "none";
        });
    }

    // Додаємо слухачі подій до елементів фільтра
    searchInput.addEventListener("input", filterTasks);
    statusFilter.addEventListener("change", filterTasks);
});

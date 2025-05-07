document.getElementById("uploadPic").addEventListener("change", function (event) {
    let file = event.target.files[0];

    if (file) {
        let formData = new FormData();
        formData.append("profilePhoto", file);

        fetch("/CarrierProfile/UploadPhoto", {
            method: "POST",
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    document.getElementById("profilePic").src = data.imageUrl;
                } else {
                    alert("Error uploading photo");
                }
            })
            .catch(error => console.error("Error:", error));
    }
});

document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("searchTask");
    const statusFilter = document.getElementById("statusFilter");
    const quantityFilter = document.getElementById("quantityFilter");
    const paymentFilter = document.getElementById("paymentFilter");
    const typeFilter = document.getElementById("typeFilter");

    function filterTasks() {
        const searchText = searchInput.value.toLowerCase();
        const status = statusFilter.value;
        const quantity = quantityFilter.value;
        const payment = paymentFilter.value;
        const selectedType = typeFilter.value.trim();

        document.querySelectorAll(".task-item").forEach(task => {
            const taskName = task.querySelector("strong").innerText.toLowerCase();
            const taskStatus = task.querySelector(".status-label").innerText;
            const taskQuantity = parseInt(task.innerText.match(/Кількість:\s*(\d+)/)?.[1] || "0");
            const taskPayment = parseInt(task.innerText.match(/Ціна:\s*(\d+)/)?.[1] || "0");
            const typeMatch = task.innerHTML.match(/Тип:\s*<strong>\s*(.*?)\s*<\/strong>/);
            const taskType = typeMatch ? typeMatch[1].trim() : "";

            let showTask = true;

            if (searchText && !taskName.includes(searchText)) showTask = false;
            if (status && taskStatus !== status) showTask = false;

            if (quantity) {
                if (quantity === "small" && !(taskQuantity >= 1 && taskQuantity <= 10)) showTask = false;
                if (quantity === "medium" && !(taskQuantity > 10 && taskQuantity <= 50)) showTask = false;
                if (quantity === "large" && taskQuantity <= 50) showTask = false;
            }

            if (payment) {
                if (payment === "low" && taskPayment >= 10000) showTask = false;
                if (payment === "medium" && !(taskPayment >= 10000 && taskPayment <= 100000)) showTask = false;
                if (payment === "high" && taskPayment <= 100000) showTask = false;
            }

            if (selectedType && taskType !== selectedType) showTask = false;

            task.style.display = showTask ? "block" : "none";
        });
    }

    searchInput.addEventListener("input", filterTasks);
    statusFilter.addEventListener("change", filterTasks);
    quantityFilter.addEventListener("change", filterTasks);
    paymentFilter.addEventListener("change", filterTasks);
    typeFilter.addEventListener("change", filterTasks);
});

function sortTasksByPriority() {
    const sortOrder = document.getElementById("taskSort").value;
    const taskList = document.querySelector(".task-list"); 
    const tasks = Array.from(taskList.children);

    const priorityMap = {
        "Високий": 3,
        "Середній": 2,
        "Низький": 1
    };

    tasks.sort((a, b) => {
        const priorityA = priorityMap[a.querySelector('.priority-label')?.innerText.trim()] || 0;
        const priorityB = priorityMap[b.querySelector('.priority-label')?.innerText.trim()] || 0;

        return sortOrder === "asc" ? priorityA - priorityB : priorityB - priorityA;
    });

    tasks.forEach(task => taskList.appendChild(task));
}


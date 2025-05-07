    function openModal() {
        document.getElementById("taskModal").style.display = "flex";
    }

    function closeModal() {
        document.getElementById("taskModal").style.display = "none";
    }


    document.getElementById("uploadPic").addEventListener("change", function(event) {
        let file = event.target.files[0];

    if (file) {
        let formData = new FormData();
    formData.append("profilePhoto", file);

    fetch("/VolunProfile/UploadPhoto", {
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
        setTimeout(function () {
            let alerts = document.querySelectorAll(".custom-alert");
            alerts.forEach(alert => {
                alert.style.transition = "opacity 0.5s";
                alert.style.opacity = "0";
                setTimeout(() => alert.remove(), 500); // Видалення після завершення анімації
            });
        }, 3000);
    });

    function filterTasks() {
        var taskNameSearch = document.getElementById("taskNameSearch").value.toLowerCase();
    var taskStatusSearch = document.getElementById("taskStatusSearch").value;
    var tasks = document.querySelectorAll(".task-item");

    tasks.forEach(function(task) {
            var taskName = task.getAttribute("data-name").toLowerCase();
    var taskStatus = task.getAttribute("data-status");

    var isNameMatch = taskName.includes(taskNameSearch);
    var isStatusMatch = taskStatus.includes(taskStatusSearch) || taskStatusSearch === "";

    if (isNameMatch && isStatusMatch) {
        task.style.display = "block";
            } else {
        task.style.display = "none";
            }
        });
}

function sortTasksByPriority() {
    const sortOrder = document.getElementById("taskSort").value;
    const taskList = document.getElementById("taskList");
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
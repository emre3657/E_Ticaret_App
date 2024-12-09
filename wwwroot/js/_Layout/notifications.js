// Bildirim sayısını güncelleyen fonksiyon
function updateNotificationCount() {
    $.get("/Home/GetUnreadNotificationCount", function (data) {
        $('#notificationBadge').text(data.count); // Badge'deki okunmamış bildirim sayısını güncelle
    });
}


// Bildirimleri dropdown içinde güncelleyen fonksiyon
function loadNotifications() {
    $.get("/Home/GetNotifications", function (data) {
        const notificationMenu = $('#notificationMenu');
        notificationMenu.empty(); // Önce mevcut bildirimi temizle

        if (data.length > 0) {
            data.forEach(notification => {
                notificationMenu.append(`
                    <li>
                        <a href="javascript:void(0);" class="dropdown-item mark-as-read" data-id="${notification.id}">
                            <p>${notification.message}</p>
                            <small>${new Date(notification.createdAt).toLocaleString()}</small>
                        </a>
                    </li>
                `);
            });
        } else {
            notificationMenu.append('<li><a class="dropdown-item">Henüz bildirim yok</a></li>');
        }
    });
}

// Bildirimleri okundu olarak işaretleyen fonksiyon
$(document).on('click', '.mark-as-read', function () {
    const notificationId = $(this).data('id');

    $.post("/Home/MarkAsRead", { notificationId: notificationId }, function (response) {
        if (response.count !== undefined) {
            $('#notificationBadge').text(response.count); // Yeni okunmamış bildirim sayısını güncelle
        }
        loadNotifications(); // Bildirimleri yeniden yükle
    });
});



// Bildirim menüsünü açma ve kapama işlemleri
$(document).ready(function () {
    updateNotificationCount();
    loadNotifications();

    // Her 30 saniyede bir bildirim sayısını ve içeriklerini güncelle
    setInterval(() => {
        updateNotificationCount();
        loadNotifications();
    }, 30000);

    // Fare ikona geldiğinde bildirim menüsünü göster
    $('#notificationDropdown').on('mouseover', function () {
        $('#notificationMenu').addClass('show');
    });

    // Fare menüden veya ikon alanından ayrıldığında menüyü gizle
    $('#notificationDropdown').on('mouseleave', function () {
        $('#notificationMenu').removeClass('show');
    });
});

// Bildirim oluşturma fonksiyonu
function createNotification(userId, message) {
    $.post("@Url.Action("CreateNotification", "Admin")", { userId: userId, message: message }, function (response) {
        if (response.success) {
            alert(response.message);
            updateNotificationCount();
            loadNotifications();
        } else {
            alert("Bildirim oluşturulamadı.");
        }
    });
}

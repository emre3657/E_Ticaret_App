$(document).ready(function () {
    $.ajax({
        url: '/Home/CartItemCount',
        type: 'GET',
        success: function (response) {
            $("#cartCountBadge").text(response); // Sepet miktarını güncelle
        },
        error: function () {
            console.error("Sepet miktarı yüklenirken bir hata oluştu.");
        }
    });
});

function addToCart(productId) {
    $.ajax({
        url: '/Home/AddToCart',
        type: 'POST',
        data: { id: productId },
        success: function (response) {
            if (response.success) {
                // Sepetteki toplam ürün sayısını güncelle
                $("#cartCountBadge").text(response.totalQuantity);
                updateCart(); // Sepet içeriğini güncelle
                alert("Ürün sepete eklendi.");
            } else {
                // Ürün zaten sepette varsa uyarı göster
                alert(response.message);
            }
        },
        error: function () {
            alert("Bir hata oluştu.");
        }
    });
}

function updateCart() {
    $.ajax({
        url: '/Home/UpdateCart', // Sepet içeriğini güncelleyen bir route
        type: 'GET',
        success: function (result) {
            $("#cartItems").html(result); // Sepet içeriğini güncelle
        }
    });
}


function toggleCart() {
    const cartSidebar = document.getElementById("cartSidebar");
    const overlay = document.getElementById("overlay");

    cartSidebar.classList.toggle("open");
    overlay.style.display = cartSidebar.classList.contains("open") ? "block" : "none";
}

// Sepet ikonuna tıklanınca sidebar'ı açmak için etkinlik
document.getElementById("cartIcon").addEventListener("click", function (event) {
    event.stopPropagation(); // Sepet ikonuna tıklamayı yayılmasını engelle
    toggleCart();
});

// Sepet dışında bir yere veya overlay'e tıklandığında sidebar'ı kapatma
window.addEventListener("click", function (event) {
    const cartSidebar = document.getElementById("cartSidebar");
    const overlay = document.getElementById("overlay");

    if (cartSidebar.classList.contains("open") && !cartSidebar.contains(event.target) && event.target !== document.getElementById("cartIcon")) {
        cartSidebar.classList.remove("open");
        overlay.style.display = "none"; // Overlay'i gizle
    }
});

function updateQuantity(productId, change) {
    // AJAX isteğiyle miktarı artır veya azalt
    $.ajax({
        url: '/Home/UpdateQuantity',
        type: 'POST',
        data: { id: productId, change: change },
        success: function (response) {
            if (response.success) {
                updateCart(); // Sepet içeriğini güncelle
                updateCartCount(); // Sepet ikonundaki miktarı güncelle
                updateCheckout(); // Checkout sayfasını güncelle
            } else {
                alert(response.message); // Stok yetersiz uyarısı
            }
        },
        error: function () {
            alert("Miktar güncellenirken bir hata oluştu.");
        }
    });
}

function removeFromCart(productId) {
    // AJAX isteğiyle ürünü sepetten çıkar
    $.ajax({
        url: '/Home/RemoveFromCart',
        type: 'POST',
        data: { id: productId },
        success: function (response) {
            updateCart();
            updateCartCount(); // Sepet ikonundaki miktarı güncelle
            updateCheckout(); // Checkout sayfasını güncelle
        },
        error: function () {
            alert("Ürün kaldırılırken bir hata oluştu.");
        }
    });
}

// Sepet ikonundaki toplam miktarı güncelle
function updateCartCount() {
    $.ajax({
        url: '/Home/CartItemCount',
        type: 'GET',
        success: function (response) {
            $("#cartCountBadge").text(response); // Sepet ikonundaki miktarı güncelle
        },
        error: function () {
            console.error("Sepet miktarı yüklenirken bir hata oluştu.");
        }
    });
}

function saveOldValue(input) {
    // Input’un mevcut değerini veri özelliğine kaydet
    input.setAttribute("data-old-value", input.value);
}

function updateQuantityDirectly(productId, newQuantity) {
    // Girilen değerin geçerli bir sayı olup olmadığını kontrol edin
    newQuantity = parseInt(newQuantity);

    // Input elemanını seç
    const input = document.querySelector(`input.quantity-input[data-product-id='${productId}']`);

    // Eğer 0'dan küçükse hata mesajı ver ve eski değeri geri yükle
    if (isNaN(newQuantity) || newQuantity < 0) {
        alert("Lütfen geçerli bir miktar girin.");
        input.value = input.getAttribute("data-old-value"); // Eski değeri geri yükle
        return;
    }

    // Eğer değer 0 ise ürünü sepetten kaldır
    if (newQuantity === 0) {
        if (confirm("Ürünü sepetten kaldırmak istiyor musunuz?")) {
            removeFromCart(productId); // Sepetten kaldırma fonksiyonunu çağır
        } else {
            input.value = input.getAttribute("data-old-value"); // Eski değere geri dön
        }
        return;
    }

    $.ajax({
        url: '/Home/UpdateQuantityDirectly',
        type: 'POST',
        data: { id: productId, quantity: newQuantity },
        success: function (response) {
            if (response.success) {
                updateCart(); // Sepet içeriğini güncelle
                updateCartCount(); // Sepet ikonundaki miktarı güncelle
                updateCheckout(); // Checkout sayfasını güncelle
            } else {
                alert(response.message); // Stok yetersiz ve mevcut stok bilgisini göster

                // Mevcut stok miktarına geri ayarla
                input.value = response.maxStock;
                updateCart(); // Sepet içeriğini güncelle
                updateCartCount(); // Sepet ikonundaki miktarı güncelle
                updateCheckout(); // Checkout sayfasını güncelle

            }
        },
        error: function () {
            alert("Miktar güncellenirken bir hata oluştu.");
        }
    });
}



function updateCheckout() {
    $.ajax({
        url: '/Home/UpdateCheckout',
        type: 'GET',
        success: function (result) {
            $("#checkoutContainer").html(result); // Checkout içeriğini güncelle
        },
        error: function () {
            alert("Checkout sayfası güncellenirken bir hata oluştu.");
        }
    });
}



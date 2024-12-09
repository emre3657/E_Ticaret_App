// Geçerli dosya uzantıları
const validExtensions = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'avif'];

// Form gönderilmeden önce doğrulama fonksiyonu
function validateForm() {
    var imageFile = document.getElementById("imageFile");
    var imageError = document.getElementById("imageError");

    // Resim dosyasının seçilip seçilmediğini kontrol ediyoruz
    if (imageFile.files.length === 0) {
        imageError.textContent = "Image is required!";
        return false; // Formu durduruyoruz
    }

    // Dosya uzantısını kontrol et
    var fileName = imageFile.files[0].name;
    var fileExtension = fileName.split('.').pop().toLowerCase();

    if (!validExtensions.includes(fileExtension)) {
        imageError.textContent = "Invalid file type! Please upload an image (jpg, jpeg, png, gif, webp, avif).";
        return false; // Formu durduruyoruz
    }

    // Hata mesajını temizle
    imageError.textContent = "";
    return true; // Formu göndermeye devam ediyoruz
}


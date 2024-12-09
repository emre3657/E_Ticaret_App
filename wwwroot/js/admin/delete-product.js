document.addEventListener("DOMContentLoaded", function () {
    // Delete butonlarına tıklama olayı ekle
    const deleteButtons = document.querySelectorAll('.btn-delete');

    deleteButtons.forEach(button => {
        button.addEventListener('click', function () {
            const productId = this.getAttribute('data-product-id'); // data-product-id'den ID'yi al
            const productName = this.getAttribute('data-product-name');
            const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

            // Silme butonunun ID'sini ayarla ve ürün ismini al.
            document.getElementById('productName').textContent = productName;
            confirmDeleteBtn.setAttribute('data-product-id', productId);
        });
    });
});


function deleteProduct() {
    const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
    const productId = confirmDeleteBtn.getAttribute('data-product-id');

    fetch(`/admin/delete-product/${productId}`, {
        method: 'DELETE'
    })
    .then(response => {
        if (response.ok) {
            // Silme işlemi başarılı, sayfayı yenile
            window.location.reload();
        }
        else {
            // Hata durumunda bir mesaj göster
            alert("Ürün silinirken bir hata oluştu.");
        }
    })
    .catch(error => {
        console.error('Silme işlemi sırasında bir hata oluştu:', error);
        alert("Silme işlemi sırasında bir hata oluştu.");
    });
}


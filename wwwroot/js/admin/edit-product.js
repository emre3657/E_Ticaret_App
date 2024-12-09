document.addEventListener('DOMContentLoaded', function () {
    const editProductModal = new bootstrap.Modal(document.getElementById('editProductModal'));
    const editProductForm = document.getElementById('editProductForm');
    const errorMessageContainer = document.getElementById('errorMessageContainer');
    const validExtensions = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'avif']; // Geçerli dosya uzantıları

    document.querySelectorAll('.btn-edit').forEach(button => {
        button.addEventListener('click', function () {
            const productId = this.getAttribute('data-product-id');

            // Fetch product details using Ajax
            fetch(`/admin/get-product/${productId}`)
                .then(response => response.json())
                .then(product => {
                    document.getElementById('editProductId').value = product.id;
                    document.getElementById('editProductName').value = product.name;
                    document.getElementById('editProductBrand').value = product.brand;
                    document.getElementById('editProductModel').value = product.model;
                    document.getElementById('editProductFeatures').value = product.features;
                    document.getElementById('editProductDescription').value = product.description;
                    document.getElementById('editProductPrice').value = product.price;
                    document.getElementById('editProductStock').value = product.stock;
                    document.getElementById('editProductCategory').value = product.categoryId;

                    const currentImageElement = document.getElementById('currentProductImage');
                    currentImageElement.src = product.imagePath || '/images/no-image.png';

                    editProductModal.show();
                });
        });
    });

    // Form submission
    editProductForm.addEventListener('submit', function (e) {
        e.preventDefault();

        // Hata mesajlarını gizle
        document.getElementById('priceError').style.display = 'none';
        document.getElementById('stockError').style.display = 'none';
        errorMessageContainer.style.display = 'none';

        // Girdi değerlerini al
        const price = parseFloat(document.getElementById('editProductPrice').value);
        const stock = parseInt(document.getElementById('editProductStock').value);
        const imageFile = document.getElementById('editProductImage'); // Dosya girdisi
        const imageError = document.getElementById('imageError'); // Hata alanı

        let isValid = true;

        // Fiyat kontrolü
        if (price <= 0) {
            document.getElementById('priceError').style.display = 'block';
            isValid = false;
        }

        // Stok kontrolü
        if (stock < 0) {
            document.getElementById('stockError').style.display = 'block';
            isValid = false;
        }

        // Resim dosyası kontrolü (isteğe bağlıysa `files.length` kısmını kontrol etmeyebilirsiniz)
        if (imageFile.files.length > 0) {
            const fileName = imageFile.files[0].name;
            const fileExtension = fileName.split('.').pop().toLowerCase();
            if (!validExtensions.includes(fileExtension)) {
                imageError.textContent = "Invalid file type! Please upload an image (jpg, jpeg, png, gif, webp, avif).";
                imageError.style.display = 'block';
                isValid = false;
            } else {
                imageError.style.display = 'none';
            }
        }

        if (isValid) {
            const productId = document.getElementById('editProductId').value;
            const formData = new FormData(editProductForm);

            fetch(`/admin/edit-product/${productId}`, {
                method: 'POST',
                body: formData
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        window.location.href = '/admin/product';
                    } else {
                        const toast = new bootstrap.Toast(document.getElementById('errorToast'));
                        document.getElementById('errorToastBody').innerText = data.errorMessage;
                        toast.show();
                    }
                })
                .catch(error => {
                    console.error('There was an error!', error);
                });
        }
    });
});

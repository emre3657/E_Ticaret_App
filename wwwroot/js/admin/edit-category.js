

document.addEventListener('DOMContentLoaded', function () {
    const editCategoryModal = new bootstrap.Modal(document.getElementById('editCategoryModal'));
    const editCategoryForm = document.getElementById('editCategoryForm');
    const errorMessageContainer = document.getElementById('errorMessageContainerEdit');

    
    // Kategori düzenleme butonuna basıldığında
    document.querySelectorAll('.btn-edit-category').forEach(button => {
        button.addEventListener('click', function () {
            const categoryId = this.getAttribute('data-category-id');

            // Kategori detaylarını almak için AJAX isteği
            fetch(`/admin/get-category/${categoryId}`)
                .then(response => response.json())
                .then(category => {
                    document.getElementById('editCategoryId').value = category.id;
                    document.getElementById('editCategoryName').value = category.name;
                    document.getElementById('editCategoryDescription').value = category.description;
                    document.getElementById('editCategoryParent').value = category.parentCategoryId;

                    // Parent category dropdown'ını doldur
                    const parentCategorySelect = document.getElementById('editCategoryParent');
                    parentCategorySelect.innerHTML = '';  // Seçenekleri temizle
                    parentCategorySelect.innerHTML = '<option value="">None</option>';

                    category.allCategories.forEach(parentCategory => {
                        const selected = parentCategory.id === category.parentCategoryId ? 'selected' : '';
                        parentCategorySelect.innerHTML += `<option value="${parentCategory.id}" ${selected}>${parentCategory.name}</option>`;
                    });

                    editCategoryModal.show();
                });
        });
    });

    // Form submission
    editCategoryForm.addEventListener('submit', function (e) {
        e.preventDefault();

        errorMessageContainer.style.display = 'none';
        const categoryId = document.getElementById('editCategoryId').value;
        const formData = new FormData(editCategoryForm);

        fetch(`/admin/edit-category/${categoryId}`, {
            method: 'POST',
            body: formData
        })

        .then(response => response.json())
      
        .then(data => {
            console.log(data);  // Yanıtı konsola yazdırarak kontrol edin
            if (data.success) {
                window.location.href = '/admin/category';
            } else {
                errorMessageContainer.innerHTML = data.errorMessage;
                errorMessageContainer.style.display = 'block';
            }
        })
        .catch(error => {
            console.error('There was an error!', error);
        });
    });
});

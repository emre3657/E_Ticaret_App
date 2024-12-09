

document.addEventListener("DOMContentLoaded", function () {
    // Delete button click event
    const deleteButtons = document.querySelectorAll('.btn-delete');

    deleteButtons.forEach(button => {
        button.addEventListener('click', function () {

            const categoryId = this.getAttribute('data-category-id');
            const categoryName = this.getAttribute('data-category-name');
            const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

            console.log("Category ID:", categoryId); // Log category ID
            console.log("Category Name:", categoryName); // Log category name

            // Update modal with category name and set the category ID to the delete button
            document.getElementById('categoryName').textContent = categoryName;
            confirmDeleteBtn.setAttribute('data-category-id', categoryId);

            //////  Hide the error message when the modal opens
            const errorContainer = document.getElementById('errorMessageContainerDelete');
            errorContainer.style.display = 'none';  // Hide the error message
            errorContainer.textContent = '';        // Clear the previous message

           
        });
    });
});

function deleteCategory() {
    const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
    const categoryId = confirmDeleteBtn.getAttribute('data-category-id');

    fetch(`/admin/delete-category/${categoryId}`, {
        method: 'DELETE'
    })
   
    .then(response => {

        console.log(categoryId);
        if (response.ok) {
            // Category deleted successfully, reload the page
            window.location.reload();
        } else {
            // Handle error response from the server
            return response.json().then(error => {
                // Display custom error message from the server
                showErrorMessage(error.message);
            });
    }
})
    .catch(error => {
        console.error('Error deleting the category:', error);
        // Show the detailed error message in the console
        error.json().then(err => console.log('Server Error:', err)).catch(() => console.log('No detailed server error available'));
        showErrorMessage("An unexpected error occurred while deleting the category.");
    });

}

function showErrorMessage(message) {
    const errorContainer = document.getElementById('errorMessageContainerDelete');
    errorContainer.textContent = message;
    errorContainer.style.display = 'block'; // Show the error message
}

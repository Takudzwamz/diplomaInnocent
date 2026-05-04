document.addEventListener('submit', function (e) {
    // Check if the submitted element is a wishlist form
    if (e.target && e.target.classList.contains('wishlist-form')) {
        e.preventDefault(); // Stop the page from reloading
        const form = e.target;
        const button = form.querySelector('button');
        const icon = button.querySelector('i');

        // Send the request in the background
        fetch(form.action, {
            method: 'POST',
            body: new FormData(form),
            headers: {
                'RequestVerificationToken': form.querySelector('input[name="__RequestVerificationToken"]').value
            }
        })
        .then(response => {
            if (response.status === 401) { // Handle not being logged in
                window.location.href = '/Account/Login';
                return;
            }
            return response.json();
        })
        .then(data => {
            if (!data) return;

            // Update the icon based on the server's response
            if (data.isInWishlist) {
                icon.classList.remove('bi-heart');
                icon.classList.add('bi-heart-fill', 'text-danger');
                button.setAttribute('title', 'Remove from Wishlist');
            } else {
                icon.classList.remove('bi-heart-fill', 'text-danger');
                icon.classList.add('bi-heart');
                button.setAttribute('title', 'Add to Wishlist');
            }
        })
        .catch(error => console.error('Error handling wishlist toggle.', error));
    }
});
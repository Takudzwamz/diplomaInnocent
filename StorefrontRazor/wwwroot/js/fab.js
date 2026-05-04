document.addEventListener('DOMContentLoaded', () => {
    const fabContainer = document.querySelector('.fab-container');
    const fabMainButton = document.querySelector('.fab-main-button');

    if (fabMainButton) {
        fabMainButton.addEventListener('click', () => {
            fabContainer.classList.toggle('active');
        });
    }
});
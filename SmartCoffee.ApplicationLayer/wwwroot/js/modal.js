window.showModal = () => {
    const modal = document.querySelector('.modal');
    console.log("SUP");
    if (modal) {
        modal.style.display = 'block';
    }
};

window.hideModal = () => {
    const modal = document.querySelector('.modal');
    if (modal) {
        modal.style.display = 'none';
    }
};
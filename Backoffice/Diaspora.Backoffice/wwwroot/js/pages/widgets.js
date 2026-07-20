/**
 * Template Name: UBold - Admin & Dashboard Template
 * By (Author): Coderthemes
 * Module/App (File Name): Widgets
 */

window.loadWidgets = function () {
    const container = document.querySelector('#chat-container');
    if (container && container.SimpleBar) {
        container.SimpleBar.getScrollElement().scrollTop = container.SimpleBar.getScrollElement().scrollHeight;
    } else {
        // Fallback if not using custom SimpleBar instance
        const scrollElement = container.querySelector('.simplebar-content-wrapper');
        if (scrollElement) {
            scrollElement.scrollTop = scrollElement.scrollHeight;
        }
    }
}
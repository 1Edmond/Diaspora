/**
 * Template Name: UBold - Admin & Dashboard Template
 * By (Author): Coderthemes
 * Module/App (File Name): Misc Clipboard
 */

window.loadMiscClipboard = function () {

    const elements = document.querySelectorAll('[data-clipboard-target]');

    if (elements && elements.length > 0) {
        new ClipboardJS(elements);
    }
};
// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ---------- Back to top ----------
document.addEventListener("DOMContentLoaded", () => {
    const button = document.getElementById("backToTop");

    if (!button) return;

    const updateButton = () => {
        button.hidden = window.scrollY < 320;
    };

    window.addEventListener("scroll", updateButton, { passive: true });

    button.addEventListener("click", () => {
        const reduceMotion = window.matchMedia(
            "(prefers-reduced-motion: reduce)"
        ).matches;

        window.scrollTo({
            top: 0,
            behavior: reduceMotion ? "auto" : "smooth"
        });
    });

    updateButton();
});

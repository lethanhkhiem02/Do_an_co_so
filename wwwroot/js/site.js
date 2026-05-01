// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ===== DARK / LIGHT MODE =====
document.addEventListener('DOMContentLoaded', function () {

    const toggleBtn = document.getElementById('theme-toggle');
    const icon = document.getElementById('theme-icon');

    // Tải lại theme đã lưu từ lần trước
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
        document.documentElement.setAttribute('data-theme', 'dark');
        icon.className = 'bi bi-moon-fill';
    } else {
        document.documentElement.setAttribute('data-theme', 'light');
        icon.className = 'bi bi-sun-fill';
    }

    // Xử lý khi bấm nút
    toggleBtn.addEventListener('click', function () {
        const currentTheme = document.documentElement.getAttribute('data-theme');

        if (currentTheme === 'dark') {
            document.documentElement.setAttribute('data-theme', 'light');
            localStorage.setItem('theme', 'light');
            icon.className = 'bi bi-sun-fill';
        } else {
            document.documentElement.setAttribute('data-theme', 'dark');
            localStorage.setItem('theme', 'dark');
            icon.className = 'bi bi-moon-fill';
        }
    });
});
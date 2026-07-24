// Theme toggle, sidebar toggle, and dropdown behavior for the premium UI.
const themeToggle = document.getElementById('themeToggle');
const sidebarToggle = document.getElementById('sidebarToggle');
const profileToggle = document.getElementById('profileToggle');
const profileDropdown = document.getElementById('profileDropdown');
const notificationToggle = document.getElementById('notificationToggle');
const notificationsPanel = document.getElementById('notificationsPanel');

const preferredTheme = localStorage.getItem('smartTheme') || 'light';
if (preferredTheme === 'dark') {
    document.documentElement.classList.add('dark-theme');
}

if (themeToggle) {
    themeToggle.addEventListener('click', () => {
        document.documentElement.classList.toggle('dark-theme');
        const theme = document.documentElement.classList.contains('dark-theme') ? 'dark' : 'light';
        localStorage.setItem('smartTheme', theme);
    });
}

if (sidebarToggle) {
    sidebarToggle.addEventListener('click', () => {
        document.querySelector('.app-shell')?.classList.toggle('sidebar-collapsed');
    });
}

if (profileToggle && profileDropdown) {
    profileToggle.addEventListener('click', () => {
        profileDropdown.classList.toggle('show');
    });
}

if (notificationToggle && notificationsPanel) {
    notificationToggle.addEventListener('click', () => {
        notificationsPanel.classList.toggle('active');
    });
}

window.addEventListener('click', (event) => {
    if (!profileToggle?.contains(event.target) && !profileDropdown?.contains(event.target)) {
        profileDropdown?.classList.remove('show');
    }
    if (!notificationToggle?.contains(event.target) && !notificationsPanel?.contains(event.target)) {
        notificationsPanel?.classList.remove('active');
    }
});

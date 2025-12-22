/**
 * Dashboard JavaScript
 * E-Commerce Platform
 */

// Toastr Config
toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "timeOut": "5000"
};

// Sidebar Toggle
class SidebarManager {
    constructor() {
        this.sidebar = document.getElementById('sidebar');
        this.toggleBtn = document.getElementById('sidebarToggle');
        this.overlay = document.getElementById('sidebarOverlay');
        this.storageKey = 'sidebar-collapsed';

        this.init();
    }

    init() {
        // Load saved state
        const isCollapsed = localStorage.getItem(this.storageKey) === 'true';
        if (isCollapsed && window.innerWidth > 992) {
            this.sidebar?.classList.add('collapsed');
        }

        // Toggle button click
        this.toggleBtn?.addEventListener('click', () => this.toggle());

        // Overlay click (mobile)
        this.overlay?.addEventListener('click', () => this.closeMobile());

        // Window resize
        window.addEventListener('resize', () => this.handleResize());
    }

    toggle() {
        if (window.innerWidth <= 992) {
            this.toggleMobile();
        } else {
            this.toggleDesktop();
        }
    }

    toggleDesktop() {
        this.sidebar?.classList.toggle('collapsed');
        const isCollapsed = this.sidebar?.classList.contains('collapsed');
        localStorage.setItem(this.storageKey, isCollapsed);
    }

    toggleMobile() {
        this.sidebar?.classList.toggle('mobile-open');
        this.overlay?.classList.toggle('active');
        document.body.style.overflow = this.sidebar?.classList.contains('mobile-open') ? 'hidden' : '';
    }

    closeMobile() {
        this.sidebar?.classList.remove('mobile-open');
        this.overlay?.classList.remove('active');
        document.body.style.overflow = '';
    }

    handleResize() {
        if (window.innerWidth > 992) {
            this.closeMobile();
        }
    }
}

// Theme Toggle
class ThemeManager {
    constructor() {
        this.html = document.documentElement;
        this.toggleBtn = document.getElementById('themeToggle');
        this.storageKey = 'theme';

        this.init();
    }

    init() {
        // Load saved theme
        const savedTheme = localStorage.getItem(this.storageKey);
        if (savedTheme) {
            this.setTheme(savedTheme);
        } else {
            // Check system preference
            const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            this.setTheme(prefersDark ? 'dark' : 'light');
        }

        // Toggle button click
        this.toggleBtn?.addEventListener('click', () => this.toggle());

        // Update icon
        this.updateIcon();
    }

    toggle() {
        const currentTheme = this.html.getAttribute('data-bs-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        this.setTheme(newTheme);
    }

    setTheme(theme) {
        this.html.setAttribute('data-bs-theme', theme);
        localStorage.setItem(this.storageKey, theme);
        this.updateIcon();
    }

    updateIcon() {
        const icon = this.toggleBtn?.querySelector('i');
        if (icon) {
            const isDark = this.html.getAttribute('data-bs-theme') === 'dark';
            icon.className = isDark ? 'fas fa-sun' : 'fas fa-moon';
        }
    }
}

// Notification Dropdown
class NotificationManager {
    constructor() {
        // Future implementation
    }
}

// Search Handler
class SearchHandler {
    constructor() {
        this.searchInput = document.getElementById('globalSearch');
        this.init();
    }

    init() {
        this.searchInput?.addEventListener('keyup', (e) => {
            if (e.key === 'Enter') {
                this.search(e.target.value);
            }
        });
    }

    search(query) {
        if (query.trim()) {
            // Implement search logic
            console.log('Searching for:', query);
        }
    }
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    window.sidebarManager = new SidebarManager();
    window.themeManager = new ThemeManager();
    window.searchHandler = new SearchHandler();
});

// Chart.js Default Config
if (typeof Chart !== 'undefined') {
    Chart.defaults.font.family = 'Inter, sans-serif';
    Chart.defaults.color = getComputedStyle(document.documentElement)
        .getPropertyValue('--text-secondary').trim() || '#64748b';
}

// Utility Functions
const Utils = {
    formatNumber(num) {
        return new Intl.NumberFormat('tr-TR').format(num);
    },

    formatCurrency(num) {
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY'
        }).format(num);
    },

    formatDate(date) {
        return new Intl.DateTimeFormat('tr-TR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        }).format(new Date(date));
    },

    formatDateTime(date) {
        return new Intl.DateTimeFormat('tr-TR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        }).format(new Date(date));
    }
};
window.themeToggle = {
    setTheme: (isDark) => {
        const body = document.body;
        if (isDark) {
            body.classList.add('dark-mode');
        } else {
            body.classList.remove('dark-mode');
        }
    }
};

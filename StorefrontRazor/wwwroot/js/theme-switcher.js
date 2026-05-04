(() => {
  const setStoredTheme = theme => localStorage.setItem('theme', theme);
  const getStoredTheme = () => localStorage.getItem('theme');
  
  const setTheme = theme => {
    document.documentElement.setAttribute('data-bs-theme', theme);
    setStoredTheme(theme);
    const toggle = document.getElementById('darkMode');
    if (toggle) toggle.checked = theme === 'dark';
  };

  window.addEventListener('DOMContentLoaded', () => {
    const savedTheme = getStoredTheme() || 'light';
    setTheme(savedTheme);

    const toggle = document.getElementById('darkMode');
    if (!toggle) return;

    toggle.addEventListener('change', () => {
      const theme = toggle.checked ? 'dark' : 'light';
      setTheme(theme);
    });
  });
})();
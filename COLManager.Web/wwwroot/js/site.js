window.toggleSidebar = function () {
    const wrapper = document.getElementById('wrapper');
    if (!wrapper) return;
    wrapper.classList.toggle('toggled');
};

window.confirmAction = function (message) {
    return confirm(message || 'Are you sure?');
};

window.showToast = function (message) {
    try {
        const container = document.getElementById('toast-container');
        if (!container) return;
        const body = container.querySelector('.toast-body');
        if (body) body.textContent = message;
        const toastEl = container.querySelector('.toast');
        if (!toastEl) return;
        const toast = new bootstrap.Toast(toastEl);
        toast.show();
    } catch (e) {
        console.warn('Toast error', e);
    }
};

window.downloadFile = function (data, filename) {
    const blob = new Blob([new Uint8Array(data)], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename || 'download.txt';
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
};


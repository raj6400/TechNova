document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.alert-dismissible').forEach(alert => {
        window.setTimeout(() => {
            bootstrap.Alert.getOrCreateInstance(alert).close();
        }, 5000);
    });

    // Product view mode toggle
    const tableViewInput = document.getElementById('tableView');
    const gridViewInput = document.getElementById('gridView');
    const tableViewContainer = document.querySelector('.table-responsive');
    const gridViewContainer = document.getElementById('gridViewContainer');
    const productContainer = document.getElementById('productContainer');

    if (tableViewInput && gridViewInput && tableViewContainer && gridViewContainer) {
        // Load saved view preference from localStorage
        const savedViewMode = localStorage.getItem('productViewMode') || 'table';

        // Set initial view
        if (savedViewMode === 'grid') {
            gridViewInput.checked = true;
            showGridView();
        } else {
            tableViewInput.checked = true;
            showTableView();
        }

        // Event listeners for view toggle
        tableViewInput.addEventListener('change', () => {
            if (tableViewInput.checked) {
                showTableView();
                localStorage.setItem('productViewMode', 'table');
            }
        });

        gridViewInput.addEventListener('change', () => {
            if (gridViewInput.checked) {
                showGridView();
                localStorage.setItem('productViewMode', 'grid');
            }
        });

        function showTableView() {
            tableViewContainer.style.display = '';
            gridViewContainer.style.display = 'none';
        }

        function showGridView() {
            tableViewContainer.style.display = 'none';
            gridViewContainer.style.display = '';
        }
    }
});

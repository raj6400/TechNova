function getCompareProducts() {
    try {
        var storedProducts = JSON.parse(localStorage.getItem('compareProducts'));
        if (!Array.isArray(storedProducts)) {
            return [];
        }

        return [...new Set(storedProducts
            .map(Number)
            .filter(function (id) { return Number.isSafeInteger(id) && id > 0; }))]
            .slice(0, 4);
    } catch {
        return [];
    }
}

function setCompareProducts(products) {
    try {
        localStorage.setItem('compareProducts', JSON.stringify(products));
        return true;
    } catch {
        window.showToast('Comparison storage is unavailable in this browser.', 'error');
        return false;
    }
}

function addToCompare(productId) {
    var id = Number(productId);
    var compareProducts = getCompareProducts();

    if (!Number.isSafeInteger(id) || id <= 0) {
        return;
    }

    if (compareProducts.includes(id)) {
        window.showToast('Product already added for comparison.', 'info');
        return;
    }

    if (compareProducts.length >= 4) {
        window.showToast('Maximum 4 products allowed for comparison.', 'info');
        return;
    }

    compareProducts.push(id);
    if (setCompareProducts(compareProducts)) {
        updateCompareBar();
        window.showToast('Product added to compare list.', 'success');
    }
}

function removeFromCompare(productId) {
    var id = Number(productId);
    var compareProducts = getCompareProducts().filter(function (product) {
        return product !== id;
    });

    if (setCompareProducts(compareProducts)) {
        updateCompareBar();
    }
}

function removeCompareAndReload(productId) {
    removeFromCompare(productId);
    var compareProducts = getCompareProducts();

    if (compareProducts.length < 2) {
        window.location.href = '/Products';
        return;
    }

    goToCompare();
}

function updateCompareBar() {
    var compareProducts = getCompareProducts();
    var compareCount = document.getElementById('compareCount');
    var compareBar = document.getElementById('compareBar');

    if (compareCount) {
        compareCount.textContent = compareProducts.length;
    }

    if (compareBar) {
        compareBar.style.display = compareProducts.length > 0 ? 'flex' : 'none';
    }
}

function goToCompare() {
    var compareProducts = getCompareProducts();
    if (compareProducts.length < 2) {
        window.showToast('Please add at least 2 products to compare.', 'info');
        return;
    }

    var query = new URLSearchParams();
    compareProducts.forEach(function (id) {
        query.append('ids', id);
    });
    window.location.href = '/Products/Compare?' + query.toString();
}

function clearCompareItems() {
    if (setCompareProducts([])) {
        updateCompareBar();
        window.location.href = '/Products';
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', updateCompareBar);
} else {
    updateCompareBar();
}

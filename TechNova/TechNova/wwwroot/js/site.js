// MobileShop Site JavaScript

$(document).ready(function () {
    var searchTimeout;
    $('#searchInput').on('input', function () {
        clearTimeout(searchTimeout);
        var term = $(this).val();

        if (term.length < 2) {
            $('#searchSuggestions').hide();
            return;
        }

        searchTimeout = setTimeout(function () {
            $.get('/Products/Search', { term: term }, function (data) {
                if (data.length > 0) {
                    var html = '';
                    data.forEach(function (item) {
                        html += `<a href="/Products/Details/${item.id}" class="list-group-item list-group-item-action d-flex align-items-center">
                            <img src="${item.image || 'https://via.placeholder.com/40x40?text=No+Image'}" class="rounded me-3" style="width: 40px; height: 40px; object-fit: cover;" />
                            <div>
                                <h6 class="mb-0">${item.name}</h6>
                                <small class="text-muted">${item.brand} - RS ${item.price.toLocaleString()}</small>
                            </div>
                        </a>`;
                    });
                    $('#suggestionsList').html(html);
                    $('#searchSuggestions').show();
                } else {
                    $('#searchSuggestions').hide();
                }
            });
        }, 300);
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#searchInput').length) {
            $('#searchSuggestions').hide();
        }
    });
});

function getAntiForgeryToken() {
    return $('input[name="__RequestVerificationToken"]').first().val();
}

function redirectToLoginIfNeeded(xhr) {
    if (xhr.responseURL && new URL(xhr.responseURL, window.location.origin).pathname.toLowerCase().endsWith('/account/login')) {
        window.location.href = xhr.responseURL;
        return true;
    }

    if (xhr.status === 401) {
        window.location.href = '/Account/Login';
        return true;
    }

    return false;
}

function postWishlistAction(url, productId, onSuccess) {
    return $.ajax({
        url: url,
        method: 'POST',
        data: {
            productId: productId,
            __RequestVerificationToken: getAntiForgeryToken()
        }
    }).done(function (data, _, xhr) {
        if (redirectToLoginIfNeeded(xhr)) {
            return;
        }

        if (data.success) {
            onSuccess(data);
        } else {
            window.showToast(data.message || 'The wishlist request could not be completed.', 'error');
        }
    }).fail(function (xhr) {
        if (!redirectToLoginIfNeeded(xhr)) {
            window.showToast('The wishlist request could not be completed. Please try again.', 'error');
        }
    });
}

window.addToWishlist = function (productId, button) {
    return postWishlistAction('/Account/AddToWishlist', productId, function (data) {
        if (button) {
            button.attr('data-in-wishlist', 'true')
                .attr('aria-label', 'In wishlist')
                .find('i').removeClass('bi-heart').addClass('bi-heart-fill');

            button.contents().filter(function () {
                return this.nodeType === 3 && this.nodeValue.trim().length > 0;
            }).first().each(function () {
                this.nodeValue = ' In Wishlist';
            });
        }

        window.showToast(data.message, 'success');
    });
};

window.showToast = function (message, type) {
    var bgClass = type === 'success' ? 'bg-success' : type === 'error' ? 'bg-danger' : 'bg-info';
    var toast = $('<div class="toast align-items-center text-white position-fixed" role="alert" aria-live="assertive" aria-atomic="true" style="top: 20px; right: 20px; z-index: 1090;"><div class="d-flex"><div class="toast-body"></div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button></div></div>');

    toast.addClass(bgClass).find('.toast-body').text(message);
    $('body').append(toast);

    var toastInstance = new bootstrap.Toast(toast[0], { delay: 3000 });
    toast.one('hidden.bs.toast', function () {
        toast.remove();
    });
    toastInstance.show();
};

$(document).on('click', '.wishlist-btn', function () {
    var button = $(this);

    if (button.attr('data-in-wishlist') === 'true') {
        window.showToast('Already added!', 'success');
        return;
    }

    button.prop('disabled', true);
    window.addToWishlist(button.data('product-id'), button).always(function () {
        button.prop('disabled', false);
    });
});

$(document).on('click', '.remove-wishlist-btn', function () {
    if (!window.confirm('Remove from wishlist?')) {
        return;
    }

    var button = $(this);
    button.prop('disabled', true);

    postWishlistAction('/Account/RemoveFromWishlist', button.data('product-id'), function (data) {
        var item = button.closest('.wishlist-item');
        item.fadeOut(300, function () {
            item.remove();

            if ($('#wishlistItems .wishlist-item').length === 0) {
                $('#wishlistItems').hide();
                $('#wishlistEmptyState').show();
            }
        });

        window.showToast(data.message, 'success');
    }).always(function () {
        button.prop('disabled', false);
    });
});

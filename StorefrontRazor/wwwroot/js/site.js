// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', function () {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    const variantSelectorsContainer = document.getElementById('variant-selectors');
    if (variantSelectorsContainer) {
        initializeVariantLogic();
    }

    // Use a single, efficient event listener on the body for all form submissions.
    // This technique is called "event delegation".
    document.body.addEventListener('submit', function (event) {
        
        // --- Handle "Add to Cart" forms (from both product list and details page) ---
        if (event.target.matches('.add-to-cart-form')) {
            event.preventDefault();
            const form = event.target;
            handleAddToCart(form, token);
        }

        // --- Handle "+/-" Quantity Update forms ---
        if (event.target.matches('.update-cart-form')) {
            event.preventDefault();
            const form = event.target;
            handleUpdateQuantity(form, token);
        }
        
        // --- Handle forms on the main Cart Page ---
        if (event.target.matches('.cart-page-update-form') || event.target.matches('.cart-page-remove-form')) {
            event.preventDefault();
            const form = event.target;
            handleCartPageUpdate(form, token);
        }
    });
});

// =================================================================
// VARIANT SELECTION LOGIC
// =================================================================
function initializeVariantLogic() {
    // These constants are defined in a <script> tag on the Details.cshtml page
    if (typeof productVariants === 'undefined' || productVariants.length === 0) return;

    const selectorsContainer = document.getElementById('variant-selectors');
    const optionGroups = selectorsContainer.querySelectorAll('.variant-pills'); // Find each group of pills
    const priceDisplayContainer = document.getElementById('price-display-container');
    const mainPriceSpan = document.getElementById('main-price');
    const originalPriceSpan = document.getElementById('original-price');
    const originalPriceDel = originalPriceSpan ? originalPriceSpan.querySelector('del') : null;
    const cartControlsContainer = document.querySelector('.cart-controls');
    const variantError = document.getElementById('variant-error');
    const stockInfoSpan = document.getElementById('stock-info');

    const variantMap = new Map();
    productVariants.forEach(variant => {
        const key = variant.optionValueIds.sort((a, b) => a - b).join('-');
        variantMap.set(key, variant);
    });

    // Build a set of all valid option value IDs for quick lookup
    const allValidOptionValueIds = new Set();
    productVariants.forEach(variant => {
        variant.optionValueIds.forEach(id => allValidOptionValueIds.add(id));
    });

    // Function to check if a combination of option values has a valid variant
    function hasValidVariant(optionValueIds) {
        const key = optionValueIds.map(Number).sort((a, b) => a - b).join('-');
        return variantMap.has(key);
    }

    // Function to get all possible combinations with a specific value
    function getValidCombinationsWithValue(valueId) {
        const validCombinations = [];
        productVariants.forEach(variant => {
            if (variant.optionValueIds.includes(Number(valueId))) {
                validCombinations.push(variant.optionValueIds);
            }
        });
        return validCombinations;
    }

    // Function to update which options are available based on current selections
    function updateAvailableOptions() {
        const allRadios = selectorsContainer.querySelectorAll('.variant-radio');
        const selectedRadios = selectorsContainer.querySelectorAll('.variant-radio:checked');
        const selectedValues = Array.from(selectedRadios).map(radio => Number(radio.value));

        // For each option group, determine which values are still available
        optionGroups.forEach(group => {
            const groupRadios = group.querySelectorAll('.variant-radio');
            const checkedInGroup = group.querySelector('.variant-radio:checked');
            const checkedValueInGroup = checkedInGroup ? Number(checkedInGroup.value) : null;

            // Get selected values from OTHER groups (not this one)
            const otherSelectedValues = selectedValues.filter(v => v !== checkedValueInGroup);

            groupRadios.forEach(radio => {
                const valueId = Number(radio.value);
                const label = radio.nextElementSibling; // The label (either .btn or .color-circle-label)
                
                // Check if this value, combined with other selected values, forms a valid variant
                let isAvailable = false;

                if (otherSelectedValues.length === 0) {
                    // No other selections yet - check if this value exists in ANY variant
                    isAvailable = allValidOptionValueIds.has(valueId);
                } else {
                    // Check if this value + other selections can form a valid combination
                    // We need to check all possible combinations
                    const testCombination = [...otherSelectedValues, valueId];
                    
                    // If we have all groups selected, check exact match
                    if (testCombination.length === optionGroups.length) {
                        isAvailable = hasValidVariant(testCombination);
                    } else {
                        // Partial selection - check if any variant contains all these values
                        isAvailable = productVariants.some(variant => {
                            return testCombination.every(val => variant.optionValueIds.includes(val));
                        });
                    }
                }

                // Update the UI to show/hide unavailable options
                if (isAvailable) {
                    radio.disabled = false;
                    radio.style.display = '';
                    if (label) {
                        label.classList.remove('unavailable');
                        label.style.display = '';
                    }
                } else {
                    // If this radio was checked and is now unavailable, uncheck it
                    if (radio.checked) {
                        radio.checked = false;
                    }
                    radio.disabled = true;
                    radio.style.display = 'none';
                    if (label) {
                        label.classList.add('unavailable');
                        label.style.display = 'none';
                    }
                }
            });
        });
    }

    function updateProductDetails() {
        // First, update which options are available
        updateAvailableOptions();

        // Find all the *checked* radio buttons to get the current selection
        const selectedRadios = selectorsContainer.querySelectorAll('.variant-radio:checked');
        const selectedValues = Array.from(selectedRadios).map(radio => radio.value);

        // Only proceed if the number of selections matches the number of option groups.
        if (selectedValues.length !== optionGroups.length) {
            cartControlsContainer.innerHTML = '<p class="text-muted">Please select all options to see price and availability.</p>';
            stockInfoSpan.textContent = '';
            return;
        }

        const selectionKey = selectedValues.map(Number).sort((a, b) => a - b).join('-');
        const selectedVariant = variantMap.get(selectionKey);

        if (selectedVariant) {
            variantError.style.display = 'none';
            const currencyFormatter = new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' });

            if (selectedVariant.discountedPrice != null && originalPriceDel) {
                priceDisplayContainer.firstElementChild.className = 'text-danger fw-bold mb-4';
                mainPriceSpan.textContent = currencyFormatter.format(selectedVariant.discountedPrice);
                originalPriceDel.textContent = currencyFormatter.format(selectedVariant.price);
                originalPriceSpan.style.display = 'inline';
            } else {
                priceDisplayContainer.firstElementChild.className = 'text-primary fw-bold mb-4';
                mainPriceSpan.textContent = currencyFormatter.format(selectedVariant.price);
                if (originalPriceSpan) originalPriceSpan.style.display = 'none';
            }

            if (selectedVariant.imageId) {
                const variantImage = productImages.find(img => img.id === selectedVariant.imageId);
                if (variantImage) {
                    // This calls the selectImage function defined on Details.cshtml
                    selectImage(variantImage.url);
                }
            }
            
            updateCartControls(selectedVariant);
        } else {
            // This shouldn't happen since we hide invalid options, but just in case
            variantError.style.display = 'none';
            stockInfoSpan.textContent = '';
            cartControlsContainer.innerHTML = '<p class="text-muted">Пожалуйста, выберите доступные опции.</p>';
        }
    }
    
    function updateCartControls(variant) {
        if (variant.quantityInStock > 0) {
            stockInfoSpan.textContent = `(В наличии: ${variant.quantityInStock})`;
            const itemInCart = cartData.items.find(i => i.productVariantId === variant.id);
            
            if (itemInCart) {
                const template = document.getElementById('quantity-controls-details-template');
                let newHtml = template.innerHTML.replace(/PRODUCT_ID/g, itemInCart.productId).replace(/VARIANT_ID/g, itemInCart.productVariantId);
                cartControlsContainer.innerHTML = newHtml;
                const controlsDiv = cartControlsContainer.querySelector('.quantity-controls');
                updateQuantityDisplay(controlsDiv, { newQuantity: itemInCart.quantity, stock: variant.quantityInStock });
            } else {
                const template = document.getElementById('add-to-cart-details-template');
                let newHtml = template.innerHTML.replace(/PRODUCT_ID/g, variant.productId).replace(/VARIANT_ID/g, variant.id);
                cartControlsContainer.innerHTML = newHtml;
            }
        } else {
             stockInfoSpan.innerHTML = '<span class="badge text-bg-danger fs-6">Нет в наличии</span>';
             cartControlsContainer.innerHTML = '<button class="btn btn-outline-danger btn-lg disabled ms-3"><i class="bi bi-x-circle"></i> Нет в наличии</button>';
        }
    }

    // Listen for clicks on the entire container
    selectorsContainer.addEventListener('click', function(e) {
        // Handle both regular text buttons and color circles
        // For text buttons: label is clicked directly
        // For color circles: the span.color-circle inside label.color-circle-label is clicked
        const target = e.target;
        let isVariantClick = false;

        if (target.tagName === 'LABEL' && target.previousElementSibling?.classList.contains('btn-check')) {
            // Regular text button label clicked
            isVariantClick = true;
        } else if (target.classList.contains('color-circle') || target.classList.contains('color-circle-label')) {
            // Color circle or its label clicked
            isVariantClick = true;
        }

        if (isVariantClick) {
            // A brief timeout allows the radio button's 'checked' state to update before we read it
            setTimeout(updateProductDetails, 50);
        }
    });
    
    updateProductDetails(); // Initial call
}

// --- LOGIC FOR ADDING A NEW ITEM ---
function handleAddToCart(form, token) {
    const submitButton = form.querySelector('button[type="submit"]');
    const originalButtonContent = submitButton.innerHTML;
    
    submitButton.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
    submitButton.disabled = true;

    postAjaxForm(form, token)
        .then(data => {
            updateCartBadge(data.itemCount);
            // On success, swap the "Add" button to the "+/-" controls
            swapToQuantityControls(form); 
        })
        .catch(error => handleFormError(submitButton, originalButtonContent, error));
}


function handleUpdateQuantity(form, token) {
    const controlsDiv = form.closest('.quantity-controls');
    if (!controlsDiv) return;

    const buttons = controlsDiv.querySelectorAll('button');
    buttons.forEach(b => b.disabled = true);

    postAjaxForm(form, token)
        .then(data => {
            updateCartBadge(data.itemCount);
            if (data.newQuantity > 0) {
                // --- MODIFICATION #1: Pass the entire 'data' object ---
                updateQuantityDisplay(controlsDiv, data);
            } else {
                swapToAddButton(controlsDiv); 
            }
        })
        .catch(error => console.error("Error updating quantity:", error))
        .finally(() => {
            // Re-enable the minus button, but the state of the plus button
            // will be controlled by our updateQuantityDisplay function.
            const minusButton = controlsDiv.querySelector('form:first-of-type button');
            if(minusButton) minusButton.disabled = false;
        });
}


function handleCartPageUpdate(form, token) {
    const button = form.querySelector('button');
    button.disabled = true;

    postAjaxForm(form, token)
        .then(data => {
            // The server response ('data') is now our single source of truth.

            // 1. Get a list of all product IDs that SHOULD be in the cart.
            const productIdsInCart = data.items.map(item => item.productId.toString());

            // 2. Get all the cart item rows currently visible on the page.
            const productRowsOnPage = document.querySelectorAll('.cart-item-row');

            // 3. Loop through the rows on the page. If a row's product ID is NOT 
            //    in our "source of truth" list from the server, remove it from the page.
            productRowsOnPage.forEach(row => {
                const rowProductId = row.dataset.productId;
                if (!productIdsInCart.includes(rowProductId)) {
                    row.remove();
                }
            });

            // 4. Now that any removed items are gone, update the visuals for all 
            //    the items that remain. This function is already correct from our last fix.
            updateFullCartUI(data);
        })
        .catch(error => console.error("Error updating cart page:", error))
        .finally(() => {
            if (button) button.disabled = false;
        });
}



    function swapToQuantityControls(addForm) {
    const controlsContainer = addForm.closest('.cart-controls');
    if (!controlsContainer) return;
    const productId = controlsContainer.dataset.productId;

    const variantId = addForm.querySelector('input[name="productVariantId"]')?.value || '';
    
    // This logic correctly determines if we're on the list page or details page
    const templateId = controlsContainer.closest('.product-card-body') 
        ? 'quantity-controls-template' 
        : 'quantity-controls-details-template';
    const template = document.getElementById(templateId);

    if (template) {
        const newHtml = template.innerHTML
        .replace(/PRODUCT_ID/g, productId)
        .replace(/VARIANT_ID/g, variantId);;
        controlsContainer.innerHTML = newHtml;

        // --- THIS IS THE FIX ---
        // After swapping, find the details page's info text and make it visible.
        const info = document.querySelector('.details-page-cart-info');
        if (info) {
            info.style.display = 'inline'; // Show the message
            info.querySelector('.quantity-display').textContent = '1'; // Set quantity to 1
        }
    }
}

function swapToAddButton(controlsDiv) {
    const controlsContainer = controlsDiv.closest('.cart-controls');
    if (!controlsContainer) return;
    const productId = controlsContainer.dataset.productId;

    const variantId = controlsDiv.querySelector('input[name="productVariantId"]')?.value || '';
    
    const templateId = controlsContainer.closest('.product-card-body') 
        ? 'add-to-cart-template' 
        : 'add-to-cart-details-template';
    const template = document.getElementById(templateId);
    
    if(template) {
        const newHtml = template.innerHTML
        .replace(/PRODUCT_ID/g, productId)
        .replace(/VARIANT_ID/g, variantId);;
        controlsContainer.innerHTML = newHtml;
        
        // --- THIS IS THE FIX ---
        // When swapping back, find the details page's info text and hide it.
        const info = document.querySelector('.details-page-cart-info');
        if (info) {
            info.style.display = 'none'; // Hide the message
        }
    }
}




function updateQuantityDisplay(controlsDiv, data) {
    // Accept the whole 'data' object and use defensive null checks
    const newQuantity = data?.newQuantity ?? 1;
    const stock = data?.stock ?? 999;

    // Update the number between the +/- buttons
    const quantityDisplay = controlsDiv.querySelector('.quantity-display');
    if (quantityDisplay) {
        quantityDisplay.textContent = newQuantity;
    }
    
    // Update the hidden input values for the next click
    const forms = controlsDiv.querySelectorAll('form');
    const plusButton = controlsDiv.querySelector('form:last-of-type button');

    if (forms.length === 2) {
        forms[0].querySelector('input[name="quantity"]').value = newQuantity - 1;
        forms[1].querySelector('input[name="quantity"]').value = newQuantity + 1;
    }
    
    // Use the stock value from the server to disable the '+' button if needed.
    if (plusButton) {
        plusButton.disabled = (newQuantity >= stock);
    }
    
    // This logic for the details page is still correct.
    const detailsPageInfo = document.querySelector('.details-page-cart-info');
    if (detailsPageInfo) {
        const detailsQuantityDisplay = detailsPageInfo.querySelector('.quantity-display');
        if (detailsQuantityDisplay) {
            detailsQuantityDisplay.textContent = newQuantity;
        }
    }
}


/**
 * A reusable function to update all dynamic parts of the main cart page.
 * @param {object} cartData The JSON response from the server.
 */
function updateFullCartUI(cartData) {
    updateCartBadge(cartData.itemCount);

    // === THIS IS THE FIX: Update all parts of the order summary ===
    document.getElementById('summary-subtotal').textContent = cartData.subtotal;
    document.getElementById('summary-delivery').textContent = cartData.delivery;
    document.getElementById('summary-total').textContent = cartData.total;
    
    const discountRow = document.getElementById('summary-discount-row');
    if (cartData.discountCode) {
        document.getElementById('summary-discount-code').textContent = cartData.discountCode;
        document.getElementById('summary-discount').textContent = `-${cartData.discount}`;
        discountRow.style.display = 'flex';
    } else {
        discountRow.style.display = 'none';
    }
    // === END OF FIX ===

    // This part is already correct and updates the item-specific details
    cartData.items.forEach(item => {
        let itemRow;
        // --- THIS IS THE FIX ---
        // If the item has a variantId, use a more specific selector to find its row.
        if (item.productVariantId) {
            itemRow = document.querySelector(`.cart-item-row[data-product-id='${item.productId}'][data-product-variant-id='${item.productVariantId}']`);
        } else {
            // Otherwise, use the old selector for simple products.
            itemRow = document.querySelector(`.cart-item-row[data-product-id='${item.productId}']`);
        }
        // --- END OF FIX ---
        if (itemRow) {
            itemRow.querySelector('.item-total').textContent = item.itemTotal;
            const quantityDisplay = itemRow.querySelector('.quantity-display');
            if (quantityDisplay) {
                quantityDisplay.textContent = item.quantity;
                const controls = itemRow.querySelector('.quantity-controls');
                if (controls) {
                    controls.querySelector('form:first-of-type input[name="quantity"]').value = item.quantity - 1;
                    controls.querySelector('form:last-of-type input[name="quantity"]').value = item.quantity + 1;
                }
            }
        }
    });
    
    if (cartData.itemCount <= 0) {
        const container = document.getElementById('cart-container');
        const template = document.getElementById('empty-cart-template');
        if (container && template) {
            container.innerHTML = template.innerHTML;
        }
    }
}



/**
 * A reusable function to update all cart badges in the navigation bar and floating controls.
 * @param {number} itemCount The total number of items in the cart.
 */
function updateCartBadge(itemCount) {
    // Find ALL elements that display the cart count
    const cartCountBadges = document.querySelectorAll('.cart-badge-count');
    // Find ALL container elements for the badges
    const cartBadgeContainers = document.querySelectorAll('.cart-badge-container');

    if (cartCountBadges.length > 0 && cartBadgeContainers.length > 0) {
        cartCountBadges.forEach(badge => {
            badge.textContent = itemCount;
        });

        cartBadgeContainers.forEach(container => {
            container.style.display = itemCount > 0 ? 'block' : 'none';
        });
    }
}

/**
 * A reusable helper function to submit a form via AJAX.
 * @param {HTMLFormElement} form The form element to submit.
 * @param {string} token The anti-forgery token.
 * @returns {Promise<any>} A promise that resolves with the JSON response.
 */
function postAjaxForm(form, token) {
    if (!token) {
        console.error('Anti-forgery token not found.');
        return Promise.reject('Anti-forgery token not found.');
    }
    return fetch(form.action, {
        method: 'POST',
        body: new FormData(form),
        headers: { 'RequestVerificationToken': token }
    }).then(response => {
        if (!response.ok) throw new Error(`Network response was not ok, status: ${response.status}`);
        return response.json();
    });
}

/**
 * A helper function to handle form submission errors.
 * @param {HTMLElement|HTMLElement[]} elements The button or buttons to update.
 * @param {string|null} originalContent The original HTML of the button to restore.
 */
function handleFormError(elements, originalContent = null) {
    return (error) => {
        console.error('AJAX form submission error:', error);
        const elementArray = Array.isArray(elements) ? elements : [elements];
        elementArray.forEach(el => {
            el.innerHTML = 'Error!';
            el.disabled = false;
            if (originalContent) {
                 setTimeout(() => { el.innerHTML = originalContent; }, 2000);
            }
        });
    };
}


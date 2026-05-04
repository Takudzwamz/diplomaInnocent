/**
 * Recommendation System Tracking
 * Tracks user interactions with recommended products for the A/B testing framework.
 */
(function () {
    'use strict';

    const TRACKING_URL = '/RecommendationTracking';

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    function postTracking(handler, params) {
        const formData = new URLSearchParams(params);
        fetch(`${TRACKING_URL}?handler=${handler}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: formData.toString()
        }).catch(function () { /* silent fail - tracking is non-critical */ });
    }

    // Track clicks on recommendation cards
    document.addEventListener('click', function (e) {
        const recCard = e.target.closest('[data-rec-product-id]');
        if (recCard) {
            postTracking('Click', {
                productId: recCard.dataset.recProductId,
                strategy: recCard.dataset.recStrategy || 'Adaptive',
                position: recCard.dataset.recPosition || '0',
                sourceProductId: recCard.dataset.recSourceId || ''
            });
        }
    });

    // Track impressions when recommendation sections become visible
    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                const section = entry.target;
                const cards = section.querySelectorAll('[data-rec-product-id]');
                cards.forEach(function (card) {
                    postTracking('Impression', {
                        productId: card.dataset.recProductId,
                        strategy: card.dataset.recStrategy || 'Adaptive',
                        position: card.dataset.recPosition || '0',
                        sourceProductId: card.dataset.recSourceId || ''
                    });
                });
                observer.unobserve(section);
            }
        });
    }, { threshold: 0.5 });

    // Observe recommendation sections
    document.querySelectorAll('[data-rec-section]').forEach(function (section) {
        observer.observe(section);
    });
})();

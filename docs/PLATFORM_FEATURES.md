# Skinet E-Commerce Platform - Complete Feature List

## Overview

This document provides a comprehensive breakdown of all features available on the Skinet e-commerce platform, organized by subscription tier. Use this as a reference for the Sputnikdevs.com website to help customers understand what each tier offers.

---

## Quick Reference - Feature Comparison

| Feature Category | Starter | Professional | Enterprise |
|-----------------|---------|--------------|------------|
| **Price** | $50/mo | $99/mo | $299/mo |
| **Product Types** | Simple only | Simple + Variable | Simple + Variable |
| **AI Features** | ❌ None | ⭐ Basic AI | 🚀 Full AI Suite |
| **Email Marketing** | ❌ | ❌ | ✅ |
| **Admin Dashboard** | Basic | Advanced | Full |
| **Support** | Email (48hr) | Priority (24hr) | Phone + WhatsApp |

---

## 🛍️ STOREFRONT FEATURES (Customer-Facing)

### Product Browsing & Discovery

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Product catalog with images | ✅ | ✅ | ✅ |
| **Simple products** (single price, single stock) | ✅ | ✅ | ✅ |
| **Variable products** (sizes, colors, multiple variants) | ❌ | ✅ | ✅ |
| Product image gallery (multiple images per product) | ✅ | ✅ | ✅ |
| Filter by Category | ✅ | ✅ | ✅ |
| Filter by Brand | ✅ | ✅ | ✅ |
| Filter by Product Type | ✅ | ✅ | ✅ |
| Filter by Rating (star ratings) | ❌ | ✅ | ✅ |
| Sort by Price (low-high, high-low) | ✅ | ✅ | ✅ |
| Sort by Name (A-Z) | ✅ | ✅ | ✅ |
| Keyword search | ✅ | ✅ | ✅ |
| Pagination | ✅ | ✅ | ✅ |
| Product detail pages | ✅ | ✅ | ✅ |
| Stock availability display | ✅ | ✅ | ✅ |
| "Out of Stock" indicators | ✅ | ✅ | ✅ |
| Related products | ✅ | ✅ | ✅ |

### AI-Powered Features (Customer-Facing)

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| AI-powered product recommendations | ❌ | ✅ | ✅ |
| AI review summaries (pros, cons, sentiment) | ❌ | ✅ | ✅ |
| **AI semantic search** ("show me red shoes for running") | ❌ | ❌ | ✅ |
| **AI chatbot** (24/7 customer support assistant) | ❌ | ❌ | ✅ |
| Personalized product recommendations | ❌ | ✅ | ✅ |

### Shopping Cart & Wishlist

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Add to cart | ✅ | ✅ | ✅ |
| Cart quantity adjustments | ✅ | ✅ | ✅ |
| Remove items from cart | ✅ | ✅ | ✅ |
| Cart persistence (Redis-backed) | ✅ | ✅ | ✅ |
| **Coupon code application** | ✅ | ✅ | ✅ |
| Wishlist functionality | ✅ | ✅ | ✅ |



### Checkout & Payments

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Multi-step checkout (Address → Delivery → Review) | ✅ | ✅ | ✅ |
| Shipping address management | ✅ | ✅ | ✅ |
| Save address to profile | ✅ | ✅ | ✅ |
| Multiple delivery methods | ✅ | ✅ | ✅ |
| Delivery notes | ✅ | ✅ | ✅ |
| Order review before payment | ✅ | ✅ | ✅ |
| **Paystack payment gateway** (South Africa) | ✅ | ✅ | ✅ |
| **PayFast payment gateway** (South Africa) | ✅ | ✅ | ✅ |
| Secure webhook processing | ✅ | ✅ | ✅ |
| Payment retry for failed transactions | ✅ | ✅ | ✅ |

### Customer Reviews & Ratings

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| View product reviews | ❌ | ✅ | ✅ |
| Star ratings display | ❌ | ✅ | ✅ |
| Write reviews (after purchase) | ❌ | ✅ | ✅ |
| Review moderation | ❌ | ✅ | ✅ |
| Average rating calculation | ❌ | ✅ | ✅ |
| **AI review summaries** | ❌ | ✅ | ✅ |

### Order Management (Customer)

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Order history | ✅ | ✅ | ✅ |
| Order details view | ✅ | ✅ | ✅ |
| Order status tracking | ✅ | ✅ | ✅ |
| Delivery status tracking | ✅ | ✅ | ✅ |
| Order tracking history/timeline | ✅ | ✅ | ✅ |

### Customer Account

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| User registration | ✅ | ✅ | ✅ |
| User login | ✅ | ✅ | ✅ |
| Address book | ✅ | ✅ | ✅ |
| Password reset | ✅ | ✅ | ✅ |
| Order history access | ✅ | ✅ | ✅ |

### Emails (Customer)

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Order confirmation email | ✅ | ✅ | ✅ |
| Delivery status update emails | ✅ | ✅ | ✅ |
| Password reset email | ✅ | ✅ | ✅ |
| **Marketing/promotional emails** | ❌ | ✅ | ✅ |

---

## 🎛️ ADMIN DASHBOARD FEATURES

### Dashboard Overview

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Basic dashboard access | ✅ | ✅ | ✅ |
| Total sales display | ✅ | ✅ | ✅ |
| Total orders count | ✅ | ✅ | ✅ |
| Total products count | ✅ | ✅ | ✅ |
| Total customers count | ✅ | ✅ | ✅ |
| Out of stock alerts | ✅ | ✅ | ✅ |
| **Sales graph (7-day, 30-day, 90-day)** | ❌ | ✅ | ✅ |
| **Top 10 selling products list** | ❌ | ✅ | ✅ |
| Recent customers list | ✅ | ✅ | ✅ |
| Quick action buttons | ✅ | ✅ | ✅ |

### Product Management

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Add/Edit/Delete products | ✅ | ✅ | ✅ |
| **Simple products only** | ✅ | ✅ | ✅ |
| **Variable products** (sizes, colors, variants) | ❌ | ✅ | ✅ |
| Product images upload (Cloudinary) | ✅ | ✅ | ✅ |
| Multiple images per product | ✅ | ✅ | ✅ |
| Set main product image | ✅ | ✅ | ✅ |
| Assign categories, brands, types | ✅ | ✅ | ✅ |
| Inventory/stock management | ✅ | ✅ | ✅ |
| Product search & filtering | ✅ | ✅ | ✅ |
| Pagination for product list | ✅ | ✅ | ✅ |
| **Top Selling Products report** (`/Admin/Products/TopSelling`) | ❌ | ✅ | ✅ |

### Product Options Management

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| **Product Options** (`/Admin/ProductOptions`) | ❌ | ✅ | ✅ |
| Create options (Size, Color, Material) | ❌ | ✅ | ✅ |
| Define option values (S, M, L, XL) | ❌ | ✅ | ✅ |
| Color hex codes for color swatches | ❌ | ✅ | ✅ |
| Link options to variable products | ❌ | ✅ | ✅ |

### Catalog Management

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Category management | ✅ | ✅ | ✅ |
| Brand management | ✅ | ✅ | ✅ |
| Product Type management | ✅ | ✅ | ✅ |

### Coupon & Discount Management

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Create coupons | ✅ | ✅ | ✅ |
| Percentage discounts | ✅ | ✅ | ✅ |
| Fixed amount discounts | ✅ | ✅ | ✅ |
| Coupon validity dates | ✅ | ✅ | ✅ |
| Usage limits | ✅ | ✅ | ✅ |
| **Minimum order amount** | ✅ | ✅ | ✅ |
| Product-specific coupons | ✅ | ✅ | ✅ |
| Coupon usage tracking | ✅ | ✅ | ✅ |

### Order Management (Admin)

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| View all orders | ✅ | ✅ | ✅ |
| Order details | ✅ | ✅ | ✅ |
| Sort by date, total | ✅ | ✅ | ✅ |
| Filter by customer email | ✅ | ✅ | ✅ |
| Update delivery status | ✅ | ✅ | ✅ |
| Add tracking events | ✅ | ✅ | ✅ |
| Order timeline/history | ✅ | ✅ | ✅ |
| Payment status display | ✅ | ✅ | ✅ |
| Refund processing | ✅ | ✅ | ✅ |

### Customer Management

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| View customer list | ✅ | ✅ | ✅ |
| Customer details | ✅ | ✅ | ✅ |
| Customer order history | ✅ | ✅ | ✅ |

### User & Role Management

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| **User Management** (`/Admin/Users`) | ❌ | ✅ | ✅ |
| Create admin users | ❌ | ✅ | ✅ |
| Role-based access control | ❌ | ✅ | ✅ |
| Manage user permissions | ❌ | ✅ | ✅ |

### Content Management (CMS)

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Hero carousel management | ✅ | ✅ | ✅ |
| Upload hero images | ✅ | ✅ | ✅ |
| Set hero slide text/CTAs | ✅ | ✅ | ✅ |
| Content blocks (promotional banners) | ✅ | ✅ | ✅ |
| FAQ management | ✅ | ✅ | ✅ |
| Footer content | ✅ | ✅ | ✅ |
| Social media links | ✅ | ✅ | ✅ |

### Email Marketing

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| **Email Marketing** (`/Admin/Marketing`) | ❌ | ✅ | ✅ |
| Send promotional emails | ❌ | ✅ | ✅ |
| Email template management | ❌ | ✅ | ✅ |
| Create custom templates | ❌ | ✅ | ✅ |
| Use coupon codes in emails | ❌ | ✅ | ✅ |
| Target customer segments | ❌ | ✅ | ✅ |

### AI Insights (Admin)

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| **AI Insights Dashboard** (`/Admin/AIInsights`) | ❌ | ❌ | ✅ |
| Sales forecasting & predictions | ❌ | ❌ | ✅ |
| Inventory insights & stock alerts | ❌ | ❌ | ✅ |
| AI pricing recommendations | ❌ | ❌ | ✅ |
| Customer insights & segmentation | ❌ | ❌ | ✅ |
| Review analysis & sentiment detection | ❌ | ❌ | ✅ |
| Product performance analysis | ❌ | ❌ | ✅ |

### Settings & Configuration

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Store name customization | ✅ | ✅ | ✅ |
| Logo upload | ✅ | ✅ | ✅ |
| Favicon upload | ✅ | ✅ | ✅ |
| Public URL configuration | ✅ | ✅ | ✅ |
| Admin notification email | ✅ | ✅ | ✅ |
| Cloudinary integration settings | ✅ | ✅ | ✅ |
| Email Sender Provider email settings | ✅ | ✅ | ✅ |
| Delivery methods configuration | ✅ | ✅ | ✅ |
| Payment gateway settings | ✅ | ✅ | ✅ |
| Payment gateway selection (Paystack/PayFast) | ✅ | ✅ | ✅ |
| Test/Live mode toggle | ✅ | ✅ | ✅ |
| AI settings (Azure OpenAI) | ❌ | ✅ | ✅ |

---

## 🔧 TECHNICAL FEATURES

### Performance & Infrastructure

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Responsive design (mobile-friendly) | ✅ | ✅ | ✅ |
| Redis caching | ✅ | ✅ | ✅ |
| Image optimization (Cloudinary CDN) | ✅ | ✅ | ✅ |
| HTTPS/SSL encryption | ✅ | ✅ | ✅ |
| Dark/Light theme support | ✅ | ✅ | ✅ |

### SEO & Marketing

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| SEO-optimized meta tags | ✅ | ✅ | ✅ |
| Open Graph tags for social sharing | ✅ | ✅ | ✅ |
| Sitemap generation | ✅ | ✅ | ✅ |
| Product schema markup | ✅ | ✅ | ✅ |

### Security

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| Secure authentication (ASP.NET Identity) | ✅ | ✅ | ✅ |
| Password hashing | ✅ | ✅ | ✅ |
| CSRF protection | ✅ | ✅ | ✅ |
| Input validation | ✅ | ✅ | ✅ |
| Webhook signature verification | ✅ | ✅ | ✅ |
| Role-based authorization | ✅ | ✅ | ✅ |

---



---

*Document Version: 1.0*
*Last Updated: December 2025*
*For internal use by Sputnik Devs development team*

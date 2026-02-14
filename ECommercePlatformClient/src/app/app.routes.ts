// import { Routes } from '@angular/router';
// import { authGuard } from './core/guards/auth-guard';
// import { guestGuard } from './core/guards/guest-guard';

// export const routes: Routes = [
//   // Home
//   {
//     path: '',
//     loadComponent: () => import('./features/home/home/home').then(m => m.HomeComponent),
//     title: 'Ana Sayfa | E-Ticaret'
//   },

//   // Products
//   {
//     path: 'products',
//     children: [
//       {
//         path: '',
//         loadComponent: () => import('./features/products/pages/product-list/product-list').then(m => m.ProductListComponent),
//         title: 'Ürünler | E-Ticaret'
//       },
//       {
//         path: ':id',
//         loadComponent: () => import('./features/products/pages/product-detail/product-detail').then(m => m.ProductDetailComponent),
//         title: 'Ürün Detayı | E-Ticaret'
//       }
//     ]
//   },

//   // Cart
//   {
//     path: 'cart',
//     loadComponent: () => import('./features/cart/cart/cart').then(m => m.CartComponent),
//     title: 'Sepetim | E-Ticaret'
//   },

//   // Auth (Guest only)
//   {
//     path: 'auth',
//     canActivate: [guestGuard],
//     children: [
//       {
//         path: '',
//         redirectTo: 'login',
//         pathMatch: 'full'
//       },
//       {
//         path: 'login',
//         loadComponent: () => import('./features/auth/pages/login/login').then(m => m.LoginComponent),
//         title: 'Giriş Yap | E-Ticaret'
//       },
//       {
//         path: 'register',
//         loadComponent: () => import('./features/auth/pages/register/register').then(m => m.RegisterComponent),
//         title: 'Kayıt Ol | E-Ticaret'
//       },
//       {
//         path: 'confirm-email',
//         loadComponent: () => import('./features/auth/pages/confirm-email/confirm-email').then(m => m.ConfirmEmailComponent),
//         title: 'E-posta Doğrulama | E-Ticaret'
//       }
//     ]
//   },

//   // Account (Protected)
//   {
//     path: 'account',
//     canActivate: [authGuard],
//     children: [
//       {
//         path: '',
//         loadComponent: () => import('./features/account/account').then(m => m.AccountComponent),
//         title: 'Hesabım | E-Ticaret'
//       },
//       {
//         path: 'orders',
//         loadComponent: () => import('./features/account/orders/orders').then(m => m.OrdersComponent),
//         title: 'Siparişlerim | E-Ticaret'
//       },
//       {
//         path: 'wishlist',
//         loadComponent: () => import('./features/account/wishlist/wishlist').then(m => m.WishlistComponent),
//         title: 'Favorilerim | E-Ticaret'
//       }
//     ]
//   },

//   // Categories
//   {
//     path: 'categories',
//     loadComponent: () => import('./features/categories/categories').then(m => m.CategoriesComponent),
//     title: 'Kategoriler | E-Ticaret'
//   },

//   // Static Pages
//   {
//     path: 'about',
//     loadComponent: () => import('./features/static/about/about').then(m => m.AboutComponent),
//     title: 'Hakkımızda | E-Ticaret'
//   },
//   {
//     path: 'contact',
//     loadComponent: () => import('./features/static/contact/contact').then(m => m.ContactComponent),
//     title: 'İletişim | E-Ticaret'
//   },

//   // 404 - Not Found
//   {
//     path: '**',
//     loadComponent: () => import('./features/not-found/not-found').then(m => m.NotFoundComponent),
//     title: 'Sayfa Bulunamadı | E-Ticaret'
//   }
// ];

import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { guestGuard } from './core/guards/guest-guard';

export const routes: Routes = [
  // Home
  {
    path: '',
    loadComponent: () => import('./features/home/home/home').then(m => m.HomeComponent),
    title: 'Ana Sayfa | E-Ticaret'
  },

  // Products
  {
    path: 'products',
    children: [
      {
        path: '',
        loadComponent: () => import('./features/products/product-list/product-list').then(m => m.ProductListComponent),
        title: 'Ürünler | E-Ticaret'
      },
      {
        path: ':id',
        loadComponent: () => import('./features/products/product-detail/product-detail').then(m => m.ProductDetailComponent),
        title: 'Ürün Detayı | E-Ticaret'
      }
    ]
  },

  // Cart
  {
    path: 'cart',
    loadComponent: () => import('./features/cart/cart/cart').then(m => m.CartComponent),
    title: 'Sepetim | E-Ticaret'
  },

  // ==========================================
  // YENİ EKLENEN: Checkout (Sipariş/Ödeme)
  // ==========================================
  {
    path: 'checkout',
    loadComponent: () => import('./features/checkout/checkout').then(m => m.CheckoutComponent),
    canActivate: [authGuard], // Sadece giriş yapmış kullanıcılar
    title: 'Ödeme ve Teslimat | E-Ticaret'
  },

  // Auth (Guest only)
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
      },
      {
        path: 'login',
        loadComponent: () => import('./features/auth/pages/login/login').then(m => m.LoginComponent),
        title: 'Giriş Yap | E-Ticaret'
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/pages/register/register').then(m => m.RegisterComponent),
        title: 'Kayıt Ol | E-Ticaret'
      },
      {
        path: 'confirm-email',
        loadComponent: () => import('./features/auth/pages/confirm-email/confirm-email').then(m => m.ConfirmEmailComponent),
        title: 'E-posta Doğrulama | E-Ticaret'
      }
    ]
  },

  // Account (Protected)
  {
    path: 'account',
    loadComponent: () => import('./features/account/account').then(m => m.AccountComponent), 
    canActivate: [authGuard],
    children: [
      {
        path: 'profile', // /account -> Profil
        loadComponent: () => import('./features/account/profile/profile').then(m => m.ProfileComponent),
        title: 'Profilim | E-Ticaret'
      },
      {
        path: 'orders', // /account/orders -> Sipariş Listesi
        loadComponent: () => import('./features/account/orders/orders').then(m => m.OrdersComponent),
        title: 'Siparişlerim | E-Ticaret'
      },
      {
        path: 'orders/:orderNumber',
        loadComponent: () => import('./features/account/order-detail/order-detail').then(m => m.OrderDetailComponent),
        title: 'Sipariş Detayı | E-Ticaret'
      },
      {
        path: 'wishlist', // /account/wishlist -> Favoriler
        loadComponent: () => import('./features/account/wishlist/wishlist').then(m => m.WishlistComponent),
        title: 'Favorilerim | E-Ticaret'
      },
      {
      path: '',
      redirectTo: 'profile',
      pathMatch: 'full'
      }
    ]
  },

  // Categories
  {
    path: 'categories',
    loadComponent: () => import('./features/categories/categories').then(m => m.CategoriesComponent),
    title: 'Kategoriler | E-Ticaret'
  },

  // Static Pages
  {
    path: 'about',
    loadComponent: () => import('./features/static/about/about').then(m => m.AboutComponent),
    title: 'Hakkımızda | E-Ticaret'
  },
  {
    path: 'contact',
    loadComponent: () => import('./features/static/contact/contact').then(m => m.ContactComponent),
    title: 'İletişim | E-Ticaret'
  },

  // 404 - Not Found
  {
    path: '**',
    loadComponent: () => import('./features/not-found/not-found').then(m => m.NotFoundComponent),
    title: 'Sayfa Bulunamadı | E-Ticaret'
  }
];

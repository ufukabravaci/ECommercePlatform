# 1. AŞAMA: DERLEME (BUILD)
# Node.js ortamında Angular projesini derliyoruz
FROM node:22-alpine AS build
WORKDIR /app

# Sadece bağımlılıkları kopyala ve kur (Önbellekleme için)
COPY package.json package-lock.json ./
RUN npm install

# Tüm kodları kopyala ve production modunda derle
COPY . .
# Not: Eğer projende 'build' script'i farklıysa (örn: npm run build:prod), burayı ona göre güncelle
RUN npm run build

# 2. AŞAMA: SUNUM (SERVE - NGINX)
# Üretilen statik dosyaları Nginx web sunucusu ile dışarı açıyoruz
FROM nginx:alpine
# Angular'ın build ettiği dosyaları Nginx'in HTML klasörüne kopyala
# DİKKAT: 'ecommerceplatformclient' yazan yeri senin angular.json içindeki 'outputPath' veya projenin adına göre değiştirmelisin!
# Genelde dist/proje-adi şeklindedir.
COPY --from=build /app/dist/storefront/browser /usr/share/nginx/html

# Nginx için Angular yönlendirme kurallarını (SPA Routing) kopyala
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
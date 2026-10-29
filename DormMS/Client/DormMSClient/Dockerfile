# 1. Stage: Build React aplikacije
FROM node:20-alpine AS build
WORKDIR /app

# Kopiramo package fajlove i instaliramo zavisnosti
COPY package*.json ./
RUN npm ci

# Kopiramo ostatak koda i pravimo produkcijski build
COPY . .
RUN npm run build

# 2. Stage: Nginx za serviranje fajlova
FROM nginx:alpine
# Ako je izlazni folder 'dist' ostavi ovako (ako je 'build', promeni /app/dist u /app/build)
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
# syntax=docker/dockerfile:1.7
# Stage 1: build
FROM node:22-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN --mount=type=cache,target=/root/.npm npm install
COPY . .
ARG VITE_BACKEND_API_BASE_URL
ENV VITE_BACKEND_API_BASE_URL=$VITE_BACKEND_API_BASE_URL
RUN npm run build

# Stage 2: serve
FROM nginx:stable-alpine
COPY --from=builder /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

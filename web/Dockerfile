# build stage
FROM node:lts-alpine AS build
WORKDIR /app
RUN npm install -g pnpm
COPY . .
RUN rm -rf node_modules && pnpm install && pnpm run build

# production stage
FROM nginx:stable-alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

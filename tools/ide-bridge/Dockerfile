FROM node:22-alpine

WORKDIR /app

COPY package*.json ./

RUN npm install --omit=dev || true

COPY . .

ENV PORT=3000 \
    NODE_ENV=production \
    PUBLIC_HEALTH=true

EXPOSE 3000

CMD ["node", "server.mjs"]

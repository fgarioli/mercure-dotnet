FROM node:lts-alpine AS development

COPY index.js /app/index.js
COPY package.json /app/package.json

WORKDIR /app

RUN npm install

CMD [ "node", "index.js" ]
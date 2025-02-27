FROM node:20-alpine

# TODO: This should be controlled by '.env'..
WORKDIR /app
COPY package*.json ./

RUN npm install -g @angular/cli
RUN npm install --production
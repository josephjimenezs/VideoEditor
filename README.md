# VideoEditor

podman-compose -f docker-compose.dev.yml up --build


## Build and push images to DockerHub

```bash
export DOCKER_USER="tu_usuario_aqui" VERSION="1.0.0"
docker build -t $DOCKER_USER/video-editor-backend:$VERSION ./backend && \
docker build -t $DOCKER_USER/video-editor-frontend:$VERSION ./frontend && \
docker tag $DOCKER_USER/video-editor-backend:$VERSION $DOCKER_USER/video-editor-backend:latest && \
docker tag $DOCKER_USER/video-editor-frontend:$VERSION $DOCKER_USER/video-editor-frontend:latest && \
docker push $DOCKER_USER/video-editor-backend:$VERSION && \
docker push $DOCKER_USER/video-editor-backend:latest && \
docker push $DOCKER_USER/video-editor-frontend:$VERSION && \
docker push $DOCKER_USER/video-editor-frontend:latest
```

## RUN the deployed image
```bash
docker run -d -p 5000:5000 --restart unless-stopped --name videoback josephjimenezs/video-editor-backend:latest
docker run -d -p 3000:3000 --restart unless-stopped --name videofront josephjimenezs/video-editor-frontend:latest
```

Then just open the url:  http://localhost:3000

Or via docker-compose, pulling the images straight from DockerHub:
```bash
podman-compose -f docker-compose.hub.yml up -d
podman-compose -f docker-compose.hub.yml down
```


## Local Development
```bash
podman-compose -f docker-compose.dev.yml up --build
podman-compose -f docker-compose.dev.yml down
```

## Production Setup



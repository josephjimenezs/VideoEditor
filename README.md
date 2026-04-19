# VideoEditor

## Test the deployed image
```bash
docker run -d -p 5000:5000 --restart unless-stopped --name videoback josephjimenezs/video-editor-backend:latest
docker run -d -p 3000:3000 --restart unless-stopped --name videofront josephjimenezs/video-editor-frontend:latest
```

## Local Development
```bash
podman-compose -f docker-compose.dev.yml up --build
podman-compose -f docker-compose.dev.yml down
```

## Production Image generation

```bash
export DOCKER_USER="tu_usuario_aqui" VERSION="1.0.0"
docker build -t $DOCKER_USER/video-processor-backend:$VERSION ./backend && \
docker build -t $DOCKER_USER/video-processor-frontend:$VERSION ./frontend && \
docker tag $DOCKER_USER/video-processor-backend:$VERSION $DOCKER_USER/video-processor-backend:latest && \
docker tag $DOCKER_USER/video-processor-frontend:$VERSION $DOCKER_USER/video-processor-frontend:latest && \
docker push $DOCKER_USER/video-processor-backend:$VERSION && \
docker push $DOCKER_USER/video-processor-backend:latest && \
docker push $DOCKER_USER/video-processor-frontend:$VERSION && \
docker push $DOCKER_USER/video-processor-frontend:latest
```


## Production Setup



# VideoEditor


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



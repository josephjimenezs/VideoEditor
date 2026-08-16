# VideoEditor

## Up
$env:DATA_DIR="$env:USERPROFILE\Downloads"; podman-compose -f docker-compose.hub.yml up -d

## Down
podman compose -f docker-compose.hub.yml down


## Where processed files are saved

All compose files mount the backend's `/data` folder to `${DATA_DIR:-./data}` on the host. Pass `DATA_DIR` inline with the compose command to save processed videos straight to your Downloads folder — otherwise it falls back to `./data` in the repo.

**Windows (PowerShell):**
```powershell
$env:DATA_DIR="$env:USERPROFILE\Downloads"; podman-compose -f docker-compose.hub.yml up -d
```

**Linux / macOS:**
```bash
DATA_DIR="$HOME/Downloads" podman-compose -f docker-compose.hub.yml up -d
```

(Swap `docker-compose.hub.yml` for `docker-compose.dev.yml` or `docker-compose.yml` depending on which environment you're running.)

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

**Windows (PowerShell):**
```powershell
docker run -d -p 5000:5000 -v "${env:USERPROFILE}\Downloads:/data" --restart unless-stopped --name videoback josephjimenezs/video-editor-backend:latest
docker run -d -p 3000:3000 --restart unless-stopped --name videofront josephjimenezs/video-editor-frontend:latest
```

**Linux / macOS:**
```bash
docker run -d -p 5000:5000 -v "$HOME/Downloads:/data" --restart unless-stopped --name videoback josephjimenezs/video-editor-backend:latest
docker run -d -p 3000:3000 --restart unless-stopped --name videofront josephjimenezs/video-editor-frontend:latest
```

Then just open the url:  http://localhost:3000

Or via docker-compose, pulling the images straight from DockerHub (see `DATA_DIR` above to set the save location):
```bash
podman-compose -f docker-compose.hub.yml up -d
podman-compose -f docker-compose.hub.yml down
```


## Local Development
(see `DATA_DIR` above to set the save location)
```bash
podman-compose -f docker-compose.dev.yml up --build
podman-compose -f docker-compose.dev.yml down
```

## Production Setup



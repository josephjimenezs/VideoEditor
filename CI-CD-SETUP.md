# CI/CD Setup - GitHub Actions to Docker Hub

## Paso 1: Configurar Secrets en GitHub

1. Ve a tu repositorio en GitHub
2. Settings → Secrets and variables → Actions → New repository secret
3. Añade estos 2 secrets:

| Nombre | Valor |
|--------|-------|
| `DOCKER_USERNAME` | Tu usuario de Docker Hub |
| `DOCKER_PASSWORD` | Tu token de acceso de Docker Hub (no contraseña) |

### Cómo obtener el token de Docker Hub:

```bash
# Opción 1: Desde Docker Hub Web
# 1. Ve a https://hub.docker.com/settings/security
# 2. Click "New Access Token"
# 3. Dale un nombre descriptivo (ej: "github-actions")
# 4. Copy el token y guárdalo como DOCKER_PASSWORD

# Opción 2: Desde CLI
docker logout
docker login
# Luego busca el token en ~/.docker/config.json
```
 
## Paso 2: Verificar los Workflows

Los workflows están en `.github/workflows/`:

- **build-and-push.yml** - Construye y sube a Docker Hub
- **test.yml** - Tests de compilación (opcional)

## Paso 3: Hacer Push del código

```bash
git add .github/
git commit -m "ci: add github actions workflows"
git push origin main
```

## Paso 4: Ver el progreso

Ve a tu repositorio → Actions y verás los workflows ejecutándose.

## Tagging y Versionado

El workflow automáticamente genera tags basados en:

- **Branches**: `main`, `develop`, etc.
- **Semantic versioning**: `v1.0.0`, `v1.2.3`
- **SHA**: Último commit hash
- **Latest**: Solo en rama default (main)

### Ejemplos de tags generados:

```
Si haces push a main:
- jjimenez/video-processor-backend:main
- jjimenez/video-processor-backend:sha-abc123
- jjimenez/video-processor-backend:latest

Si creas tag v1.0.0:
- jjimenez/video-processor-backend:v1.0.0
- jjimenez/video-processor-backend:1.0
- jjimenez/video-processor-backend:1
- jjimenez/video-processor-backend:latest
```

## Disparadores del Workflow

El workflow se ejecuta automáticamente cuando:

✅ Haces push a `main` o `master`
✅ Creas un tag como `v*`
✅ Abres un Pull Request

En PRs solo construye (no sube a Docker Hub). En main/master sí sube.

## Ejemplo de Deployment

Una vez que tengas las imágenes en Docker Hub:

```bash
# Producción
docker-compose -f docker-compose.yml up -d

# Desarrollo
docker-compose -f docker-compose.dev.yml up -d
```

## Troubleshooting

### Error: "denied: requested access to the resource is denied"

- Verifica que DOCKER_USERNAME y DOCKER_PASSWORD sean correctos
- El token debe tener permisos de lectura/escritura

### Las imágenes no se suben

- Verifica que el workflow no falló en la sección "Log in to Docker Hub"
- Comprueba los logs en GitHub Actions

### Necesito hacer rebuild manual

```bash
# Si necesitas forzar un rebuild sin cambios:
git commit --allow-empty -m "ci: trigger rebuild"
git push
```

## Variables de Entorno (Personalizar)

En `build-and-push.yml`, cambia si necesitas:

```yaml
env:
  REGISTRY: docker.io  # O tu registry privado
  IMAGE_NAME_BACKEND: tu_usuario/video-processor-backend
  IMAGE_NAME_FRONTEND: tu_usuario/video-processor-frontend
```

## Próximos pasos opcionales

1. **Notificaciones**: Configura Slack/Discord notifications
2. **Scanning de seguridad**: Añade Trivy o Grype
3. **Tests adicionales**: Añade pruebas unitarias
4. **Staging**: Despliega automáticamente a un servidor de staging

---

¡Tu CI/CD está listo! 🚀

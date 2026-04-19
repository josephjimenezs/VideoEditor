# CI/CD Setup - GitHub Actions to Docker Hub

## ✅ CHECKLIST DE VERIFICACIÓN

Antes de nada, verifica esto:

- [ ] El archivo `.github/workflows/build-and-push.yml` existe en tu repo
- [ ] Configuraste `DOCKER_USERNAME` en GitHub Secrets
- [ ] Configuraste `DOCKER_PASSWORD` en GitHub Secrets  
- [ ] Tu rama se llama `main` o `master` (no `master-old`, `development`, etc.)
- [ ] Hiciste `git push` a la rama correcta
- [ ] Espere 30 segundos antes de revisar Actions

## Paso 1: Configurar Secrets en GitHub

1. Ve a tu repositorio en GitHub
2. **Settings** → **Secrets and variables** → **Actions** → **New repository secret**
3. Añade estos 2 secrets:

| Nombre | Valor |
|--------|-------|
| `DOCKER_USERNAME` | Tu usuario de Docker Hub (ej: `jjimenez`) |
| `DOCKER_PASSWORD` | Tu token de acceso de Docker Hub |

### ⚠️ IMPORTANTE: Cómo obtener el token

**NO uses tu contraseña de Docker Hub, usa un Access Token:**

1. Ve a https://hub.docker.com/settings/security
2. Click **New Access Token**
3. Dale un nombre descriptivo (ej: "github-actions")
4. Selecciona permisos: **Read & Write**
5. Click **Generate**
6. **COPIA el token inmediatamente** (solo aparece una vez)
7. Pega en GitHub como `DOCKER_PASSWORD`

## Paso 2: Verifica la estructura de carpetas

```
tu-repo/
├── .github/
│   └── workflows/
│       └── build-and-push.yml  ← Debe estar aquí
├── backend/
├── frontend/
└── ...
```

## Paso 3: Haz un commit y push

```bash
git add .github/workflows/build-and-push.yml
git commit -m "ci: setup github actions for docker hub"
git push origin main
```

## Paso 4: Revisa en GitHub Actions

1. Ve a tu repo en GitHub
2. Click en la pestaña **Actions**
3. Deberías ver el workflow ejecutándose

Si no aparece nada, revisa:
- ¿El nombre de la rama es exactamente `main` o `master`?
- ¿Ya hace 30 segundos desde el push?
- ¿GitHub está mostrando "No workflows have run yet"?

## Paso 5: Solucionar problemas

### Error: "refused: requested access to the resource is denied"

**Solución:**
- El token de Docker Hub es inválido o expiró
- Vuelve a generar uno nuevo en https://hub.docker.com/settings/security
- Actualiza en GitHub Secrets

### Workflow no aparece en Actions

**Checklist:**
```bash
# Verifica que estés en la rama correcta
git branch

# Verifica que push se ejecutó
git log --oneline -5

# Si necesitas, fuerza un push
git push -u origin main --force
```

### Logs dicen "No such file or directory"

- Verifica que `backend/Dockerfile` y `frontend/Dockerfile` existen
- Los paths son sensibles a mayúsculas/minúsculas

## Tagging y Versionado automático

El workflow genera tags automáticamente:

```
Si haces push a main:
→ usuario/video-processor-backend:latest
→ usuario/video-processor-backend:abc123def (SHA commit)

Si creas tag v1.0.0:
→ usuario/video-processor-backend:v1.0.0
→ usuario/video-processor-backend:latest
```

## Ejemplo completo de uso

```bash
# 1. Haz cambios en el código
# 2. Commit y push
git add .
git commit -m "feat: add new feature"
git push origin main

# 3. GitHub Actions se ejecuta automáticamente
# 4. Verifica en Actions que todo pasó
# 5. Revisa Docker Hub: https://hub.docker.com/r/tu_usuario/video-processor-backend

# Después, en producción:
docker pull tu_usuario/video-processor-backend:latest
docker run ...
```

## Variables de entorno en el Workflow

Si necesitas cambiar algo, edita `.github/workflows/build-and-push.yml`:

```yaml
# Para cambiar la imagen de compilación:
# Busca "docker/build-push-action" y modifica "context: ./backend"

# Para agregar más branches:
on:
  push:
    branches:
      - main
      - master
      - develop  # ← Añade aquí
```

## ¿Todo funciona? ✅

Cuando veas en GitHub Actions que dice "✓ Build and Push to Docker Hub", significa:

- ✅ Código compiló correctamente
- ✅ Imágenes Docker se crearon
- ✅ Imágenes se subieron a Docker Hub
- ✅ Listo para producción

---

**Próximo paso:** Una vez confirmado que funciona, haz un deployment en producción con las imágenes de Docker Hub.

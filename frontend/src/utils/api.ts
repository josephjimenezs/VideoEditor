const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000'

export interface ProcessingResponse {
  fileId: string
  downloadUrl: string
  filename: string
  fileSizeBytes: number
}

export interface VideoMetadata {
  duration: number
  width: number
  height: number
  fps: number
  videoCodec: string
  audioCodec: string
  fileSizeBytes: number
  audioBitrate: string
}

export interface EditVideoRequest {
  operation: string
  outputFormat: string
  outputName: string
  // Trim
  startTime?: string
  endTime?: string
  // Crop
  cropWidth?: number
  cropHeight?: number
  cropX?: number
  cropY?: number
  // Resize
  scaleWidth?: number
  scaleHeight?: number
  // Speed
  speed?: number
  // Volume
  volume?: number
  // GIF
  gifFps?: number
  gifWidth?: number
  gifLoop?: number
  // Watermark
  watermarkFile?: File
  watermarkPosition?: string
  watermarkX?: number
  watermarkY?: number
  // Equalizer
  brightness?: number
  contrast?: number
  saturation?: number
  // Rotate
  rotation?: number
  flipHorizontal?: boolean
  flipVertical?: boolean
  // Compress
  crf?: number
  // Subtitle
  subtitleFile?: File
  // Thumbnail
  thumbnailTime?: string
  // Concat
  additionalFiles?: File[]
}

// ── Original conversion (keeps old endpoint working) ─────────────────────────
export const uploadAndProcessVideo = async (
  file: File,
  outputFormat: string,
  outputName: string
): Promise<ProcessingResponse> => {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('outputFormat', outputFormat)
  formData.append('outputName', outputName)

  const response = await fetch(`${API_URL}/api/v1/video/process`, {
    method: 'POST',
    body: formData,
  })
  if (!response.ok) {
    const err = await response.json()
    throw new Error(err.error || 'Failed to process video')
  }
  return response.json()
}

// ── New multi-operation edit ──────────────────────────────────────────────────
export const editVideo = async (
  file: File,
  request: EditVideoRequest
): Promise<ProcessingResponse> => {
  const formData = new FormData()
  formData.append('file', file)

  // Append every defined field
  Object.entries(request).forEach(([key, value]) => {
    if (value === undefined || value === null) return
    if (key === 'watermarkFile' || key === 'subtitleFile') return
    if (key === 'additionalFiles') return
    formData.append(key, String(value))
  })

  if (request.watermarkFile) formData.append('watermarkFile', request.watermarkFile)
  if (request.subtitleFile)  formData.append('subtitleFile', request.subtitleFile)
  if (request.additionalFiles) {
    request.additionalFiles.forEach(f => formData.append('additionalFiles', f))
  }

  const response = await fetch(`${API_URL}/api/v1/video/edit`, {
    method: 'POST',
    body: formData,
  })
  if (!response.ok) {
    const err = await response.json()
    throw new Error(err.error || 'Failed to edit video')
  }
  return response.json()
}

// ── Video info ────────────────────────────────────────────────────────────────
export const getVideoInfo = async (file: File): Promise<VideoMetadata> => {
  const formData = new FormData()
  formData.append('file', file)

  const response = await fetch(`${API_URL}/api/v1/video/info`, {
    method: 'POST',
    body: formData,
  })
  if (!response.ok) throw new Error('Failed to get video info')
  return response.json()
}

// ── Download ──────────────────────────────────────────────────────────────────
export const downloadFile = (fileName: string): void => {
  const link = document.createElement('a')
  link.href = `${API_URL}/api/v1/video/download/${encodeURIComponent(fileName)}`
  link.download = fileName
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

// ── Health ────────────────────────────────────────────────────────────────────
export const getHealthStatus = async (): Promise<boolean> => {
  try {
    const response = await fetch(`${API_URL}/health`)
    return response.ok
  } catch {
    return false
  }
}

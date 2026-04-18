import React from 'react'
import { VideoMetadata } from '../../utils/api'

interface VideoPreviewProps {
  file: File | null
  metadata: VideoMetadata | null
}

function formatDuration(s: number) {
  const h = Math.floor(s / 3600)
  const m = Math.floor((s % 3600) / 60)
  const sec = Math.floor(s % 60)
  return [h, m, sec].map(v => String(v).padStart(2, '0')).join(':')
}

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`
  if (bytes < 1073741824) return `${(bytes / 1048576).toFixed(1)} MB`
  return `${(bytes / 1073741824).toFixed(2)} GB`
}

export const VideoPreview: React.FC<VideoPreviewProps> = ({ file, metadata }) => {
  const [objectUrl, setObjectUrl] = React.useState<string | null>(null)

  React.useEffect(() => {
    if (!file) { setObjectUrl(null); return }
    const url = URL.createObjectURL(file)
    setObjectUrl(url)
    return () => URL.revokeObjectURL(url)
  }, [file])

  if (!file) return (
    <div className="video-preview-empty">
      <div className="preview-icon">🎬</div>
      <p>Select a video to preview</p>
    </div>
  )

  return (
    <div className="video-preview">
      <video
        src={objectUrl ?? undefined}
        controls
        className="preview-video"
        preload="metadata"
      />
      {metadata && (
        <div className="metadata-grid">
          <div className="meta-item">
            <span className="meta-label">Duration</span>
            <span className="meta-value">{formatDuration(metadata.duration)}</span>
          </div>
          <div className="meta-item">
            <span className="meta-label">Resolution</span>
            <span className="meta-value">{metadata.width}×{metadata.height}</span>
          </div>
          <div className="meta-item">
            <span className="meta-label">FPS</span>
            <span className="meta-value">{metadata.fps}</span>
          </div>
          <div className="meta-item">
            <span className="meta-label">Video</span>
            <span className="meta-value">{metadata.videoCodec || '—'}</span>
          </div>
          <div className="meta-item">
            <span className="meta-label">Audio</span>
            <span className="meta-value">{metadata.audioCodec || '—'}</span>
          </div>
          <div className="meta-item">
            <span className="meta-label">Size</span>
            <span className="meta-value">{formatBytes(metadata.fileSizeBytes)}</span>
          </div>
        </div>
      )}
    </div>
  )
}

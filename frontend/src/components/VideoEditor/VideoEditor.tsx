import React from 'react'
import { OperationTabs, OPERATIONS } from './OperationTabs'
import { VideoPreview } from './VideoPreview'
import { TrimOperation } from './operations/TrimOperation'
import { CropOperation } from './operations/CropOperation'
import { ResizeOperation } from './operations/ResizeOperation'
import { SpeedOperation } from './operations/SpeedOperation'
import { VolumeOperation } from './operations/VolumeOperation'
import { ExtractAudioOperation } from './operations/ExtractAudioOperation'
import { GifOperation } from './operations/GifOperation'
import { WatermarkOperation } from './operations/WatermarkOperation'
import { EqualizerOperation } from './operations/EqualizerOperation'
import { RotateOperation } from './operations/RotateOperation'
import { RemoveAudioOperation } from './operations/RemoveAudioOperation'
import { CompressOperation } from './operations/CompressOperation'
import { SubtitleOperation } from './operations/SubtitleOperation'
import { ConcatOperation } from './operations/ConcatOperation'
import { ThumbnailOperation } from './operations/ThumbnailOperation'
import { ConvertOperation } from './operations/ConvertOperation'
import { VideoMetadata, EditVideoRequest, editVideo, getVideoInfo, downloadFile } from '../../utils/api'
import './VideoEditor.css'

interface ProcessedFile {
  fileId: string
  fileName: string
  fileSize: number
  downloadUrl: string
}

interface VideoEditorProps {
  onProgress: (pct: number) => void
  onStatus: (status: 'idle' | 'processing' | 'completed' | 'error', msg?: string) => void
  status: 'idle' | 'processing' | 'completed' | 'error'
  processedFile: ProcessedFile | null
  setProcessedFile: (f: ProcessedFile | null) => void
}

export const VideoEditor: React.FC<VideoEditorProps> = ({
  onProgress, onStatus, status, processedFile, setProcessedFile
}) => {
  const [file, setFile] = React.useState<File | null>(null)
  const [metadata, setMetadata] = React.useState<VideoMetadata | null>(null)
  const [activeOp, setActiveOp] = React.useState('Trim')
  const [params, setParams] = React.useState<any>({})
  const [outputName, setOutputName] = React.useState('')
  const [dragOver, setDragOver] = React.useState(false)
  const fileInputRef = React.useRef<HTMLInputElement>(null)

  const isProcessing = status === 'processing'

  // Auto output format from operation defaults
  const currentOp = OPERATIONS.find(o => o.id === activeOp)!
  const outputFormat: string = params.outputFormat ?? currentOp.defaultFormat

  const handleFileSelect = async (selected: File) => {
    setFile(selected)
    setProcessedFile(null)
    onStatus('idle')
    const nameWithoutExt = selected.name.split('.').slice(0, -1).join('.')
    setOutputName(`${nameWithoutExt}_edited`)
    setMetadata(null)
    try {
      const info = await getVideoInfo(selected)
      setMetadata(info)
    } catch { /* non-critical */ }
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    setDragOver(false)
    const f = e.dataTransfer.files[0]
    if (f && f.type.startsWith('video/')) handleFileSelect(f)
  }

  const handleOpChange = (id: string) => {
    setActiveOp(id)
    setParams({})
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!file || !outputName) return

    try {
      onStatus('processing')
      onProgress(0)

      const request: EditVideoRequest = {
        operation: activeOp,
        outputFormat,
        outputName,
        ...params,
      }

      const result = await editVideo(file, request)
      setProcessedFile({
        fileId: result.fileId,
        fileName: result.filename,
        fileSize: result.fileSizeBytes,
        downloadUrl: result.downloadUrl,
      })
      onStatus('completed')
    } catch (err) {
      onStatus('error', err instanceof Error ? err.message : 'Processing failed')
    }
  }

  const renderOperation = () => {
    const props = { params, onChange: setParams, disabled: isProcessing }
    switch (activeOp) {
      case 'Trim':         return <TrimOperation {...props} />
      case 'Crop':         return <CropOperation {...props} />
      case 'Resize':       return <ResizeOperation {...props} />
      case 'Speed':        return <SpeedOperation {...props} />
      case 'Volume':       return <VolumeOperation {...props} />
      case 'ExtractAudio': return <ExtractAudioOperation {...props} />
      case 'VideoToGif':   return <GifOperation {...props} />
      case 'Watermark':    return <WatermarkOperation {...props} />
      case 'Equalizer':    return <EqualizerOperation {...props} />
      case 'Rotate':       return <RotateOperation {...props} />
      case 'RemoveAudio':  return <RemoveAudioOperation {...props} />
      case 'Compress':     return <CompressOperation {...props} />
      case 'Subtitles':    return <SubtitleOperation {...props} />
      case 'Concatenate':  return <ConcatOperation {...props} />
      case 'Thumbnail':    return <ThumbnailOperation {...props} />
      default:             return <ConvertOperation {...props} />
    }
  }

  return (
    <div className="video-editor">
      {/* ── File drop zone ── */}
      <div
        className={`drop-zone ${dragOver ? 'drag-over' : ''} ${file ? 'has-file' : ''}`}
        onDragOver={e => { e.preventDefault(); setDragOver(true) }}
        onDragLeave={() => setDragOver(false)}
        onDrop={handleDrop}
        onClick={() => !file && fileInputRef.current?.click()}
      >
        <input
          ref={fileInputRef}
          type="file"
          accept="video/*"
          hidden
          onChange={e => { const f = e.target.files?.[0]; if (f) handleFileSelect(f) }}
        />
        {!file ? (
          <div className="drop-prompt">
            <div className="drop-icon">📁</div>
            <p>Drop video here or <span className="link">click to browse</span></p>
            <small>MP4, MKV, WebM, AVI, MOV, FLV, WMV… up to 5GB</small>
          </div>
        ) : (
          <div className="file-chip">
            <span>🎬 {file.name}</span>
            <button type="button" className="clear-btn" onClick={e => { e.stopPropagation(); setFile(null); setMetadata(null); setProcessedFile(null); onStatus('idle') }}>✕</button>
          </div>
        )}
      </div>

      {/* ── Preview + metadata ── */}
      <VideoPreview file={file} metadata={metadata} />

      {/* ── Operation tabs ── */}
      <OperationTabs active={activeOp} onChange={handleOpChange} disabled={isProcessing} />

      {/* ── Operation panel ── */}
      <form onSubmit={handleSubmit} className="editor-form">
        <div className="op-content">
          {renderOperation()}
        </div>

        {/* ── Output settings ── */}
        <div className="output-row">
          <div className="form-group flex-1">
            <label htmlFor="output-name">Output Filename</label>
            <input
              id="output-name"
              type="text"
              value={outputName}
              onChange={e => setOutputName(e.target.value)}
              placeholder="output"
              disabled={isProcessing}
              required
            />
          </div>
          <div className="form-group format-badge">
            <label>Format</label>
            <span className="badge">.{outputFormat}</span>
          </div>
        </div>

        <button
          type="submit"
          className="process-btn"
          disabled={!file || !outputName || isProcessing}
        >
          {isProcessing ? (
            <><span className="spinner" /> Processing…</>
          ) : (
            <>{currentOp.icon} {currentOp.label}</>
          )}
        </button>
      </form>

      {/* ── Download result ── */}
      {status === 'completed' && processedFile && (
        <div className="result-card">
          <div className="result-info">
            <span className="result-icon">✅</span>
            <div>
              <strong>{processedFile.fileName}</strong>
              <small>{(processedFile.fileSize / 1024 / 1024).toFixed(2)} MB</small>
            </div>
          </div>
          <button className="download-btn" onClick={() => downloadFile(processedFile.fileName)}>
            ⬇ Download
          </button>
        </div>
      )}
    </div>
  )
}

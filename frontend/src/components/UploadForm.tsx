import React from 'react'
import './UploadForm.css'

interface UploadFormProps {
  onSubmit: (file: File, format: string, name: string) => void
  isLoading: boolean
}

export const UploadForm: React.FC<UploadFormProps> = ({ onSubmit, isLoading }) => {
  const [file, setFile] = React.useState<File | null>(null)
  const [format, setFormat] = React.useState('mp4')
  const [outputName, setOutputName] = React.useState('')
  const [error, setError] = React.useState<string | null>(null)
  const fileInputRef = React.useRef<HTMLInputElement>(null)

  const MAX_FILE_SIZE = 5 * 1024 * 1024 * 1024 // 5GB

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0]
    if (selectedFile) {
      if (selectedFile.size > MAX_FILE_SIZE) {
        setError('File size exceeds 5GB limit')
        setFile(null)
        setOutputName('')
        if (fileInputRef.current) fileInputRef.current.value = ''
        return
      }

      setError(null)
      setFile(selectedFile)
      // Auto-generate output name from input file
      const nameWithoutExt = selectedFile.name.split('.').slice(0, -1).join('.')
      setOutputName(nameWithoutExt || 'output')
    }
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (file && outputName && format) {
      onSubmit(file, format, outputName)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="upload-form">
      <div className="form-group">
        <label htmlFor="file">Select Video File:</label>
        <input
          ref={fileInputRef}
          id="file"
          type="file"
          accept="video/*"
          onChange={handleFileChange}
          disabled={isLoading}
          required
        />
        <div className="file-info">
          <small className="limit-label">Maximum file size: 5GB</small>
          {file && <span className="file-name">{file.name} ({(file.size / (1024 * 1024 * 1024)).toFixed(2)} GB)</span>}
          {error && <span className="error-message">{error}</span>}
        </div>
      </div>

      <div className="form-group">
        <label htmlFor="format">Output Format:</label>
        <select
          id="format"
          value={format}
          onChange={(e) => setFormat(e.target.value)}
          disabled={isLoading}
        >
          <option value="mp4">MP4 (H.264)</option>
          <option value="mkv">MKV (H.265)</option>
          <option value="webm">WebM (VP9)</option>
        </select>
      </div>

      <div className="form-group">
        <label htmlFor="outputName">Output Name:</label>
        <input
          id="outputName"
          type="text"
          value={outputName}
          onChange={(e) => setOutputName(e.target.value)}
          placeholder="output"
          disabled={isLoading}
          required
        />
      </div>

      <button type="submit" disabled={!file || !outputName || isLoading}>
        {isLoading ? 'Processing...' : 'Process Video'}
      </button>
    </form>
  )
}

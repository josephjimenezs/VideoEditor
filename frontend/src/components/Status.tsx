import React from 'react'
import './Status.css'

interface StatusProps {
  status: 'idle' | 'processing' | 'completed' | 'error'
  message?: string
  fileName?: string
  fileSize?: number
  onDownload?: () => void
  onReset?: () => void
}

export const Status: React.FC<StatusProps> = ({
  status,
  message,
  fileName,
  fileSize,
  onDownload,
  onReset,
}) => {
  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i]
  }

  const getStatusIcon = () => {
    switch (status) {
      case 'processing':
        return <div className="spinner"></div>
      case 'completed':
        return <span className="status-icon success">✓</span>
      case 'error':
        return <span className="status-icon error">⚠</span>
      default:
        return null
    }
  }

  return (
    <div className={`status-container ${status}`}>
      <div className="status-content">
        {getStatusIcon()}
        <div className="status-text">
          <h3 className="status-title">
            {status === 'idle' && 'Ready to process'}
            {status === 'processing' && 'Processing...'}
            {status === 'completed' && 'Completed!'}
            {status === 'error' && 'Error'}
          </h3>
          {message && <p className="status-message">{message}</p>}
        </div>
      </div>

      {status === 'completed' && fileName && (
        <div className="completed-info">
          <div className="file-info">
            <span className="info-label">File:</span>
            <span className="info-value">{fileName}</span>
          </div>
          {fileSize && (
            <div className="file-info">
              <span className="info-label">Size:</span>
              <span className="info-value">{formatFileSize(fileSize)}</span>
            </div>
          )}
          <button className="download-btn" onClick={onDownload}>
            ↓ Download File
          </button>
        </div>
      )}

      {status === 'completed' && onReset && (
        <button className="reset-btn" onClick={onReset}>
          Process Another Video
        </button>
      )}
    </div>
  )
}

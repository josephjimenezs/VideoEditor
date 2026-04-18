import React from 'react'
import './ProgressBar.css'

interface ProgressBarProps {
  progress: number
  eta?: number
}

export const ProgressBar: React.FC<ProgressBarProps> = ({ progress, eta }) => {
  const formatTime = (seconds: number): string => {
    if (!seconds || isNaN(seconds)) return '--:--'
    const hours = Math.floor(seconds / 3600)
    const minutes = Math.floor((seconds % 3600) / 60)
    const secs = Math.floor(seconds % 60)
    
    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
    }
    return `${minutes}:${secs.toString().padStart(2, '0')}`
  }

  return (
    <div className="progress-container">
      <div className="progress-header">
        <span className="progress-label">Processing Progress</span>
        <span className="progress-percentage">{Math.round(progress)}%</span>
      </div>
      <div className="progress-bar-wrapper">
        <div 
          className="progress-bar-fill" 
          style={{ width: `${Math.min(100, progress)}%` }}
        ></div>
      </div>
      {eta && eta > 0 && (
        <div className="progress-footer">
          <span className="eta-label">Estimated time remaining: {formatTime(eta)}</span>
        </div>
      )}
    </div>
  )
}

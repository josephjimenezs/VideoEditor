import React from 'react'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const ExtractAudioOperation: React.FC<Props> = ({ params, onChange, disabled }) => (
  <div className="op-panel">
    <p className="op-description">Extract only the audio track from the video and save it as an audio file.</p>
    <div className="format-grid">
      {['mp3', 'aac', 'wav', 'ogg', 'flac'].map(fmt => (
        <button
          key={fmt}
          type="button"
          className={`format-btn ${(params.outputFormat ?? 'mp3') === fmt ? 'active' : ''}`}
          onClick={() => onChange({ ...params, outputFormat: fmt })}
          disabled={disabled}
        >
          {fmt.toUpperCase()}
        </button>
      ))}
    </div>
  </div>
)

import React from 'react'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const ConvertOperation: React.FC<Props> = ({ params, onChange, disabled }) => (
  <div className="op-panel">
    <p className="op-description">Re-encode the video to a different container and codec format.</p>
    <div className="format-grid wide">
      {[
        { fmt: 'mp4',  label: 'MP4',  sub: 'H.264 / AAC' },
        { fmt: 'mkv',  label: 'MKV',  sub: 'H.265 / AAC' },
        { fmt: 'webm', label: 'WebM', sub: 'VP9 / Opus' },
      ].map(({ fmt, label, sub }) => (
        <button key={fmt} type="button"
          className={`format-card ${(params.outputFormat ?? 'mp4') === fmt ? 'active' : ''}`}
          onClick={() => onChange({ ...params, outputFormat: fmt })}
          disabled={disabled}>
          <span className="fmt-name">{label}</span>
          <span className="fmt-sub">{sub}</span>
        </button>
      ))}
    </div>
  </div>
)

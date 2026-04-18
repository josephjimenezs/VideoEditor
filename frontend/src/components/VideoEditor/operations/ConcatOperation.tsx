import React from 'react'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const ConcatOperation: React.FC<Props> = ({ params, onChange, disabled }) => {
  const handleFiles = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files ?? []).slice(0, 4)
    onChange({ ...params, additionalFiles: files })
  }
  const additional: File[] = params.additionalFiles ?? []
  return (
    <div className="op-panel">
      <p className="op-description">Join multiple videos end-to-end. The main file will be first; select up to 4 additional files to append in order.</p>
      <div className="info-panel warn">
        <span className="info-icon">⚠️</span>
        <div>For best results, all videos should have the same resolution, frame rate and codec.</div>
      </div>
      <div className="form-group">
        <label htmlFor="concat-files">Additional Videos (up to 4)</label>
        <input id="concat-files" type="file" accept="video/*" multiple onChange={handleFiles} disabled={disabled} />
      </div>
      {additional.length > 0 && (
        <ul className="concat-list">
          {additional.map((f, i) => <li key={i}>📹 {f.name}</li>)}
        </ul>
      )}
    </div>
  )
}

import React from 'react'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const SubtitleOperation: React.FC<Props> = ({ params, onChange, disabled }) => {
  const handleFile = (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0]
    if (f) onChange({ ...params, subtitleFile: f })
  }
  return (
    <div className="op-panel">
      <p className="op-description">Burn subtitles directly into the video (hardcoded). Supports SRT, ASS, VTT and SUB formats.</p>
      <div className="form-group">
        <label htmlFor="sub-file">Subtitle File</label>
        <input id="sub-file" type="file" accept=".srt,.ass,.vtt,.sub" onChange={handleFile} disabled={disabled} />
        {params.subtitleFile && <small className="hint">✓ {params.subtitleFile.name}</small>}
      </div>
    </div>
  )
}

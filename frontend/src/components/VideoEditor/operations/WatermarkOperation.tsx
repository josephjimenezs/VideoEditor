import React from 'react'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }

const POSITIONS = ['topleft', 'topright', 'bottomleft', 'bottomright']

export const WatermarkOperation: React.FC<Props> = ({ params, onChange, disabled }) => {
  const handleFile = (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0]
    if (f) onChange({ ...params, watermarkFile: f })
  }
  return (
    <div className="op-panel">
      <p className="op-description">Overlay an image (logo/watermark) on the video at the selected position.</p>
      <div className="form-group">
        <label htmlFor="wm-file">Watermark Image (PNG, JPG)</label>
        <input id="wm-file" type="file" accept="image/*" onChange={handleFile} disabled={disabled} />
        {params.watermarkFile && <small className="hint">✓ {params.watermarkFile.name}</small>}
      </div>
      <div className="form-group">
        <label>Position</label>
        <div className="position-grid">
          {POSITIONS.map(pos => (
            <button key={pos} type="button"
              className={`pos-btn ${(params.watermarkPosition ?? 'bottomright') === pos ? 'active' : ''}`}
              onClick={() => onChange({ ...params, watermarkPosition: pos })}
              disabled={disabled}>
              {pos}
            </button>
          ))}
        </div>
      </div>
      <div className="grid-2">
        <div className="form-group">
          <label htmlFor="wm-x">X Offset (px)</label>
          <input id="wm-x" type="number" min={0} value={params.watermarkX ?? 10} onChange={e => onChange({ ...params, watermarkX: +e.target.value })} disabled={disabled} />
        </div>
        <div className="form-group">
          <label htmlFor="wm-y">Y Offset (px)</label>
          <input id="wm-y" type="number" min={0} value={params.watermarkY ?? 10} onChange={e => onChange({ ...params, watermarkY: +e.target.value })} disabled={disabled} />
        </div>
      </div>
    </div>
  )
}

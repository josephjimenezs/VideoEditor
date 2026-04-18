import React from 'react'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }

const PRESETS = [
  { label: 'Custom', w: 0, h: 0 },
  { label: '4K (3840×2160)', w: 3840, h: 2160 },
  { label: '1080p (1920×1080)', w: 1920, h: 1080 },
  { label: '720p (1280×720)', w: 1280, h: 720 },
  { label: '480p (854×480)', w: 854, h: 480 },
  { label: '360p (640×360)', w: 640, h: 360 },
]

export const ResizeOperation: React.FC<Props> = ({ params, onChange, disabled }) => {
  const applyPreset = (w: number, h: number) => onChange({ ...params, scaleWidth: w || undefined, scaleHeight: h || undefined })

  return (
    <div className="op-panel">
      <p className="op-description">Scale the video to a specific resolution. Use -2 to auto-calculate a dimension while maintaining aspect ratio.</p>
      <div className="form-group">
        <label htmlFor="resize-preset">Preset</label>
        <select id="resize-preset" onChange={e => { const p = PRESETS[+e.target.value]; applyPreset(p.w, p.h) }} disabled={disabled}>
          {PRESETS.map((p, i) => <option key={i} value={i}>{p.label}</option>)}
        </select>
      </div>
      <div className="grid-2">
        <div className="form-group">
          <label htmlFor="scale-w">Width (px, -2 = auto)</label>
          <input id="scale-w" type="number" value={params.scaleWidth ?? ''} onChange={e => onChange({ ...params, scaleWidth: +e.target.value })} disabled={disabled} placeholder="-2" />
        </div>
        <div className="form-group">
          <label htmlFor="scale-h">Height (px, -2 = auto)</label>
          <input id="scale-h" type="number" value={params.scaleHeight ?? ''} onChange={e => onChange({ ...params, scaleHeight: +e.target.value })} disabled={disabled} placeholder="-2" />
        </div>
      </div>
    </div>
  )
}

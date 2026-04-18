import React from 'react'
import { NumberSlider } from '../NumberSlider'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const SpeedOperation: React.FC<Props> = ({ params, onChange, disabled }) => {
  const speed = params.speed ?? 1.0
  return (
    <div className="op-panel">
      <p className="op-description">Change the playback speed. Values below 1× slow down, above 1× speed up. Audio pitch is preserved.</p>
      <NumberSlider id="speed-slider" label="Speed" min={0.25} max={4} step={0.05} value={speed} unit="×" onChange={v => onChange({ ...params, speed: v })} disabled={disabled} />
      <div className="speed-presets">
        {[0.25, 0.5, 0.75, 1, 1.25, 1.5, 2, 4].map(s => (
          <button key={s} type="button" className={`preset-btn ${speed === s ? 'active' : ''}`} onClick={() => onChange({ ...params, speed: s })} disabled={disabled}>{s}×</button>
        ))}
      </div>
    </div>
  )
}

import React from 'react'
import { NumberSlider } from '../NumberSlider'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const EqualizerOperation: React.FC<Props> = ({ params, onChange, disabled }) => (
  <div className="op-panel">
    <p className="op-description">Adjust brightness, contrast and saturation of the video using FFmpeg's eq filter.</p>
    <NumberSlider id="eq-bright" label="Brightness" min={-1} max={1} step={0.01} value={params.brightness ?? 0} onChange={v => onChange({ ...params, brightness: v })} disabled={disabled} />
    <NumberSlider id="eq-contrast" label="Contrast" min={-1000} max={1000} step={1} value={params.contrast ?? 1} onChange={v => onChange({ ...params, contrast: v })} disabled={disabled} />
    <NumberSlider id="eq-sat" label="Saturation" min={0} max={3} step={0.05} value={params.saturation ?? 1} onChange={v => onChange({ ...params, saturation: v })} disabled={disabled} />
  </div>
)

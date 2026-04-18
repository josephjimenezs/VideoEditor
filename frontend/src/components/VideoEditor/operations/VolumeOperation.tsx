import React from 'react'
import { NumberSlider } from '../NumberSlider'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const VolumeOperation: React.FC<Props> = ({ params, onChange, disabled }) => (
  <div className="op-panel">
    <p className="op-description">Adjust the audio volume. 1.0 = original, 2.0 = double, 0.0 = mute.</p>
    <NumberSlider id="vol-slider" label="Volume" min={0} max={3} step={0.05} value={params.volume ?? 1.0} unit="×" onChange={v => onChange({ ...params, volume: v })} disabled={disabled} />
  </div>
)

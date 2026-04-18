import React from 'react'
import { NumberSlider } from '../NumberSlider'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const CompressOperation: React.FC<Props> = ({ params, onChange, disabled }) => {
  const crf = params.crf ?? 28
  const quality = crf <= 18 ? 'Lossless' : crf <= 23 ? 'High' : crf <= 28 ? 'Medium' : crf <= 35 ? 'Low' : 'Very Low'
  return (
    <div className="op-panel">
      <p className="op-description">Reduce file size by re-encoding with a higher CRF. Lower = better quality & larger file; Higher = smaller file & lower quality.</p>
      <NumberSlider id="crf-slider" label={`CRF (Quality: ${quality})`} min={0} max={51} step={1} value={crf} onChange={v => onChange({ ...params, crf: v })} disabled={disabled} />
      <div className="crf-info-bar">
        <span className="good">◀ Better Quality</span>
        <span className="bad">Smaller File ▶</span>
      </div>
    </div>
  )
}

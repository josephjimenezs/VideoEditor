import React from 'react'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const GifOperation: React.FC<Props> = ({ params, onChange, disabled }) => (
  <div className="op-panel">
    <p className="op-description">Convert the video (or a portion using Trim) to an animated GIF with an optimized palette.</p>
    <div className="grid-2">
      <div className="form-group">
        <label htmlFor="gif-fps">Frames per Second</label>
        <input id="gif-fps" type="number" min={1} max={30} value={params.gifFps ?? 10} onChange={e => onChange({ ...params, gifFps: +e.target.value })} disabled={disabled} />
      </div>
      <div className="form-group">
        <label htmlFor="gif-w">Width (px)</label>
        <input id="gif-w" type="number" min={64} max={1920} value={params.gifWidth ?? 480} onChange={e => onChange({ ...params, gifWidth: +e.target.value })} disabled={disabled} />
      </div>
      <div className="form-group">
        <label htmlFor="gif-loop">Loop Count (0 = ∞)</label>
        <input id="gif-loop" type="number" min={0} max={100} value={params.gifLoop ?? 0} onChange={e => onChange({ ...params, gifLoop: +e.target.value })} disabled={disabled} />
      </div>
    </div>
  </div>
)

import React from 'react'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const CropOperation: React.FC<Props> = ({ params, onChange, disabled }) => {
  const set = (k: string, v: number) => onChange({ ...params, [k]: v })
  return (
    <div className="op-panel">
      <p className="op-description">Crop the video frame to a specific region. Use width/height for the output size and X/Y for the top-left offset.</p>
      <div className="grid-2">
        <div className="form-group">
          <label htmlFor="crop-w">Width (px)</label>
          <input id="crop-w" type="number" min={1} value={params.cropWidth ?? ''} onChange={e => set('cropWidth', +e.target.value)} disabled={disabled} placeholder="e.g. 1280" />
        </div>
        <div className="form-group">
          <label htmlFor="crop-h">Height (px)</label>
          <input id="crop-h" type="number" min={1} value={params.cropHeight ?? ''} onChange={e => set('cropHeight', +e.target.value)} disabled={disabled} placeholder="e.g. 720" />
        </div>
        <div className="form-group">
          <label htmlFor="crop-x">X Offset (px)</label>
          <input id="crop-x" type="number" min={0} value={params.cropX ?? ''} onChange={e => set('cropX', +e.target.value)} disabled={disabled} placeholder="0" />
        </div>
        <div className="form-group">
          <label htmlFor="crop-y">Y Offset (px)</label>
          <input id="crop-y" type="number" min={0} value={params.cropY ?? ''} onChange={e => set('cropY', +e.target.value)} disabled={disabled} placeholder="0" />
        </div>
      </div>
    </div>
  )
}

import React from 'react'
import { TimeRangeInput } from '../TimeRangeInput'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const ThumbnailOperation: React.FC<Props> = ({ params, onChange, disabled }) => (
  <div className="op-panel">
    <p className="op-description">Extract a single frame from the video as a JPG or PNG image.</p>
    <TimeRangeInput id="thumb-time" label="Timestamp" value={params.thumbnailTime ?? '00:00:01'} onChange={v => onChange({ ...params, thumbnailTime: v })} disabled={disabled} />
  </div>
)

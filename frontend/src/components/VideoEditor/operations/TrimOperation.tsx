import React from 'react'
import { TimeRangeInput } from '../TimeRangeInput'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const TrimOperation: React.FC<Props> = ({ params, onChange, disabled }) => (
  <div className="op-panel">
    <p className="op-description">Cut your video between two timestamps. Only the selected range will be kept.</p>
    <TimeRangeInput id="trim-start" label="Start Time" value={params.startTime ?? ''} onChange={v => onChange({ ...params, startTime: v })} disabled={disabled} />
    <TimeRangeInput id="trim-end" label="End Time" value={params.endTime ?? ''} onChange={v => onChange({ ...params, endTime: v })} disabled={disabled} />
  </div>
)

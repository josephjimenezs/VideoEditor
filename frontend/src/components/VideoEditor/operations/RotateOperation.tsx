import React from 'react'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }

const ROTATIONS = [0, 90, 180, 270]

export const RotateOperation: React.FC<Props> = ({ params, onChange, disabled }) => (
  <div className="op-panel">
    <p className="op-description">Rotate or mirror the video. Rotation and flip transforms can be combined.</p>
    <div className="form-group">
      <label>Rotation</label>
      <div className="rotation-btns">
        {ROTATIONS.map(r => (
          <button key={r} type="button"
            className={`rot-btn ${(params.rotation ?? 0) === r ? 'active' : ''}`}
            onClick={() => onChange({ ...params, rotation: r })}
            disabled={disabled}>
            {r}°
          </button>
        ))}
      </div>
    </div>
    <div className="form-group">
      <label>Flip</label>
      <div className="flip-btns">
        <button type="button"
          className={`flip-btn ${params.flipHorizontal ? 'active' : ''}`}
          onClick={() => onChange({ ...params, flipHorizontal: !params.flipHorizontal })}
          disabled={disabled}>
          ↔ Horizontal
        </button>
        <button type="button"
          className={`flip-btn ${params.flipVertical ? 'active' : ''}`}
          onClick={() => onChange({ ...params, flipVertical: !params.flipVertical })}
          disabled={disabled}>
          ↕ Vertical
        </button>
      </div>
    </div>
  </div>
)

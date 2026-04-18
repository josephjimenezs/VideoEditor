import React from 'react'

interface Props { params: any; onChange: (p: any) => void; disabled: boolean }
export const RemoveAudioOperation: React.FC<Props> = () => (
  <div className="op-panel">
    <div className="info-panel">
      <span className="info-icon">🔇</span>
      <div>
        <strong>Remove Audio Track</strong>
        <p>The audio stream will be completely removed. The video codec stream is copied as-is with no re-encoding for fast processing.</p>
      </div>
    </div>
  </div>
)

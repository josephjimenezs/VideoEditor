import React from 'react'

export const OPERATIONS = [
  { id: 'Trim',         label: 'Trim / Cut',      icon: '✂️',  defaultFormat: 'mp4' },
  { id: 'Convert',      label: 'Convert',          icon: '🔄',  defaultFormat: 'mp4' },
  { id: 'Resize',       label: 'Resize',           icon: '📐',  defaultFormat: 'mp4' },
  { id: 'Crop',         label: 'Crop',             icon: '🖼️',  defaultFormat: 'mp4' },
  { id: 'Speed',        label: 'Speed',            icon: '⚡',  defaultFormat: 'mp4' },
  { id: 'Volume',       label: 'Volume',           icon: '🔊',  defaultFormat: 'mp4' },
  { id: 'Equalizer',    label: 'Brightness',       icon: '🎨',  defaultFormat: 'mp4' },
  { id: 'Rotate',       label: 'Rotate / Flip',    icon: '🔃',  defaultFormat: 'mp4' },
  { id: 'Compress',     label: 'Compress',         icon: '📦',  defaultFormat: 'mp4' },
  { id: 'ExtractAudio', label: 'Extract Audio',    icon: '🎵',  defaultFormat: 'mp3' },
  { id: 'VideoToGif',   label: 'To GIF',           icon: '🎞️',  defaultFormat: 'gif' },
  { id: 'Watermark',    label: 'Watermark',        icon: '💧',  defaultFormat: 'mp4' },
  { id: 'Subtitles',    label: 'Subtitles',        icon: '💬',  defaultFormat: 'mp4' },
  { id: 'Concatenate',  label: 'Concatenate',      icon: '🔗',  defaultFormat: 'mp4' },
  { id: 'RemoveAudio',  label: 'Remove Audio',     icon: '🔇',  defaultFormat: 'mp4' },
  { id: 'Thumbnail',    label: 'Thumbnail',        icon: '🖼️',  defaultFormat: 'jpg' },
]

interface OperationTabsProps {
  active: string
  onChange: (id: string) => void
  disabled: boolean
}

export const OperationTabs: React.FC<OperationTabsProps> = ({ active, onChange, disabled }) => (
  <nav className="op-tabs">
    {OPERATIONS.map(op => (
      <button
        key={op.id}
        type="button"
        className={`op-tab ${active === op.id ? 'active' : ''}`}
        onClick={() => onChange(op.id)}
        disabled={disabled}
        title={op.label}
      >
        <span className="tab-icon">{op.icon}</span>
        <span className="tab-label">{op.label}</span>
      </button>
    ))}
  </nav>
)

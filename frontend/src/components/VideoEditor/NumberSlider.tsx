import React from 'react'

interface NumberSliderProps {
  id: string
  label: string
  min: number
  max: number
  step?: number
  value: number
  onChange: (v: number) => void
  unit?: string
  disabled?: boolean
}

export const NumberSlider: React.FC<NumberSliderProps> = ({
  id, label, min, max, step = 0.01, value, onChange, unit = '', disabled = false
}) => (
  <div className="slider-group">
    <label htmlFor={id}>
      {label}: <span className="slider-value">{value}{unit}</span>
    </label>
    <input
      id={id}
      type="range"
      min={min}
      max={max}
      step={step}
      value={value}
      onChange={e => onChange(parseFloat(e.target.value))}
      disabled={disabled}
      className="slider-input"
    />
    <div className="slider-minmax">
      <span>{min}{unit}</span><span>{max}{unit}</span>
    </div>
  </div>
)

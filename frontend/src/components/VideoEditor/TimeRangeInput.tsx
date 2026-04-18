import React from 'react'

interface TimeRangeInputProps {
  id: string
  label: string
  value: string
  onChange: (v: string) => void
  disabled?: boolean
  placeholder?: string
}

function isValidTime(v: string) {
  return /^\d{2}:\d{2}:\d{2}(\.\d+)?$/.test(v)
}

export const TimeRangeInput: React.FC<TimeRangeInputProps> = ({
  id, label, value, onChange, disabled = false, placeholder = '00:00:00'
}) => (
  <div className="form-group">
    <label htmlFor={id}>{label}</label>
    <input
      id={id}
      type="text"
      value={value}
      onChange={e => onChange(e.target.value)}
      placeholder={placeholder}
      disabled={disabled}
      className={`time-input ${value && !isValidTime(value) ? 'input-error' : ''}`}
      pattern="\d{2}:\d{2}:\d{2}"
    />
    <small className="hint">Format: HH:MM:SS</small>
  </div>
)

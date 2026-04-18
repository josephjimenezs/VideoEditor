import { useEffect, useState } from 'react'
import { VideoEditor } from './components/VideoEditor/VideoEditor'
import { ProgressBar } from './components/ProgressBar'
import { useSignalR } from './hooks/useSignalR'
import { getHealthStatus } from './utils/api'
import './App.css'

type AppStatus = 'idle' | 'processing' | 'completed' | 'error'

interface ProcessedFile {
  fileId: string
  fileName: string
  fileSize: number
  downloadUrl: string
}

function App() {
  const [status, setStatus]               = useState<AppStatus>('idle')
  const [progress, setProgress]           = useState(0)
  const [eta, setEta]                     = useState<number | undefined>()
  const [errorMessage, setErrorMessage]   = useState('')
  const [processedFile, setProcessedFile] = useState<ProcessedFile | null>(null)
  const [backendHealthy, setBackendHealthy] = useState(false)

  const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000'
  const { connect, disconnect, on, off } = useSignalR(`${apiUrl}/progressHub`)

  // Check backend health
  useEffect(() => {
    const checkHealth = async () => setBackendHealthy(await getHealthStatus())
    checkHealth()
    const interval = setInterval(checkHealth, 5000)
    return () => clearInterval(interval)
  }, [])

  // SignalR listeners
  useEffect(() => {
    let mounted = true
    if (status === 'processing') {
      connect().then(() => {
        if (!mounted) return
        on('progress', (percent: number, e: number) => { if (mounted) { setProgress(percent); setEta(e > 0 ? e : undefined) } })
        on('completed', () => { if (mounted) { setStatus('completed'); setProgress(100); setEta(undefined) } })
        on('error', (msg: string) => { if (mounted) { setStatus('error'); setErrorMessage(msg); disconnect() } })
      }).catch(console.error)
    }
    return () => {
      mounted = false
      if (status === 'processing') { off('progress'); off('completed'); off('error') }
    }
  }, [status, connect, disconnect, on, off])

  const handleStatus = (s: AppStatus, msg?: string) => {
    setStatus(s)
    if (s === 'processing') { setProgress(0); setEta(undefined); setErrorMessage('') }
    if (s === 'error') setErrorMessage(msg ?? 'An error occurred')
    if (s === 'idle') { setProgress(0); setEta(undefined); setErrorMessage(''); setProcessedFile(null) }
  }

  return (
    <div className="app-root">
      <header className="app-header">
        <div className="header-inner">
          <div className="logo">
            <span className="logo-icon">🎬</span>
            <div>
              <h1>VideoEditor</h1>
              <p>Powered by FFmpeg</p>
            </div>
          </div>
          <div className={`health-pill ${backendHealthy ? 'healthy' : 'unhealthy'}`}>
            <span className="health-dot" />
            {backendHealthy ? 'Backend Ready' : 'Backend Offline'}
          </div>
        </div>
      </header>

      <main className="app-main">
        {status === 'processing' && (
          <div className="progress-banner">
            <ProgressBar progress={progress} eta={eta} />
          </div>
        )}

        {status === 'error' && (
          <div className="error-banner">
            <span>⚠️</span>
            <span>{errorMessage}</span>
            <button onClick={() => handleStatus('idle')}>✕</button>
          </div>
        )}

        <VideoEditor
          onProgress={setProgress}
          onStatus={handleStatus}
          status={status}
          processedFile={processedFile}
          setProcessedFile={setProcessedFile}
        />
      </main>

      <footer className="app-footer">
        <p>Supports: MP4 · MKV · WebM · AVI · MOV · FLV and more · Max 5GB</p>
      </footer>
    </div>
  )
}

export default App

import { useEffect, useState } from 'react'
import { UploadForm } from './components/UploadForm'
import { ProgressBar } from './components/ProgressBar'
import { Status } from './components/Status'
import { useSignalR } from './hooks/useSignalR'
import { uploadAndProcessVideo, downloadFile, getHealthStatus } from './utils/api'
import './App.css'

type AppStatus = 'idle' | 'processing' | 'completed' | 'error'

interface ProcessedFile {
  fileId: string
  fileName: string
  fileSize: number
  downloadUrl: string
}

function App() {
  const [status, setStatus] = useState<AppStatus>('idle')
  const [progress, setProgress] = useState(0)
  const [eta, setEta] = useState<number | undefined>()
  const [errorMessage, setErrorMessage] = useState('')
  const [processedFile, setProcessedFile] = useState<ProcessedFile | null>(null)
  const [backendHealthy, setBackendHealthy] = useState(false)

  const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000'
  const { connect, disconnect, on, off } = useSignalR(`${apiUrl}/progressHub`)

  // Check backend health
  useEffect(() => {
    const checkHealth = async () => {
      const healthy = await getHealthStatus()
      setBackendHealthy(healthy)
    }
    checkHealth()
    const interval = setInterval(checkHealth, 5000)
    return () => clearInterval(interval)
  }, [])

  // Setup SignalR listeners
  useEffect(() => {
    let isMounted = true

    const setupListeners = async () => {
      if (status === 'processing') {
        try {
          await connect()
          if (isMounted) {
            on('progress', (percent: number, eta: number) => {
              if (isMounted) {
                setProgress(percent)
                setEta(eta > 0 ? eta : undefined)
              }
            })
            on('completed', () => {
              if (isMounted) {
                setStatus('completed')
                setProgress(100)
                setEta(undefined)
              }
            })
            on('error', (message: string) => {
              if (isMounted) {
                setStatus('error')
                setErrorMessage(message)
                disconnect()
              }
            })
          }
        } catch (error) {
          if (isMounted) {
            console.error('Failed to setup SignalR listeners:', error)
          }
        }
      }
    }

    setupListeners()

    return () => {
      isMounted = false
      if (status === 'processing') {
        off('progress')
        off('completed')
        off('error')
      }
    }
  }, [status, connect, disconnect, on, off])

  const handleUpload = async (file: File, format: string, outputName: string) => {
    try {
      setStatus('processing')
      setProgress(0)
      setEta(undefined)
      setErrorMessage('')

      const result = await uploadAndProcessVideo(file, format, outputName)

      setProcessedFile({
        fileId: result.fileId,
        fileName: result.filename,
        fileSize: result.fileSizeBytes,
        downloadUrl: result.downloadUrl,
      })

      setStatus('completed')
    } catch (error) {
      setStatus('error')
      setErrorMessage(error instanceof Error ? error.message : 'An error occurred')
    }
  }

  const handleDownload = () => {
    if (processedFile) {
      downloadFile(processedFile.fileName)
    }
  }

  const handleReset = () => {
    setStatus('idle')
    setProgress(0)
    setEta(undefined)
    setErrorMessage('')
    setProcessedFile(null)
  }

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>🎬 Video Processor</h1>
        <p>Process your videos with FFmpeg in the cloud</p>
        <div className={`health-status ${backendHealthy ? 'healthy' : 'unhealthy'}`}>
          <span className="health-indicator"></span>
          {backendHealthy ? 'Backend Ready' : 'Backend Offline'}
        </div>
      </header>

      <main className="app-main">
        <div className="content-box">
          <UploadForm
            onSubmit={handleUpload}
            isLoading={status === 'processing'}
          />

          {status === 'processing' && (
            <ProgressBar progress={progress} eta={eta} />
          )}

          <Status
            status={status}
            message={errorMessage || undefined}
            fileName={processedFile?.fileName}
            fileSize={processedFile?.fileSize}
            onDownload={handleDownload}
            onReset={handleReset}
          />
        </div>

        <footer className="app-footer">
          <p>Supports: MP4, MKV, WebM • Max file size: 5GB</p>
        </footer>
      </main>
    </div>
  )
}

export default App

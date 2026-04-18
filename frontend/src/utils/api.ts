const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000'

export const uploadAndProcessVideo = async (
  file: File,
  outputFormat: string,
  outputName: string
): Promise<any> => {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('outputFormat', outputFormat)
  formData.append('outputName', outputName)

  try {
    const response = await fetch(`${API_URL}/api/v1/video/process`, {
      method: 'POST',
      body: formData,
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to process video')
    }

    return await response.json()
  } catch (error) {
    console.error('Error uploading video:', error)
    throw error
  }
}

export const downloadFile = (fileName: string): void => {
  const link = document.createElement('a')
  link.href = `${API_URL}/api/v1/video/download/${encodeURIComponent(fileName)}`
  link.download = fileName
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

export const getHealthStatus = async (): Promise<boolean> => {
  try {
    const response = await fetch(`${API_URL}/health`)
    return response.ok
  } catch {
    return false
  }
}

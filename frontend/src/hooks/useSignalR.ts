import { useEffect, useRef, useCallback, useState } from 'react'
import * as signalR from '@microsoft/signalr'

export const useSignalR = (url: string) => {
  const connectionRef = useRef<signalR.HubConnection | null>(null)
  const [connectionStatus, setConnectionStatus] = useState<'disconnected' | 'connecting' | 'connected'>('disconnected')

  const connect = useCallback(async () => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      return
    }

    try {
      setConnectionStatus('connecting')
      const connection = new signalR.HubConnectionBuilder()
        .withUrl(url)
        .withAutomaticReconnect()
        .build()

      connection.onreconnecting(() => setConnectionStatus('connecting'))
      connection.onreconnected(() => setConnectionStatus('connected'))
      connection.onclose(() => setConnectionStatus('disconnected'))

      await connection.start()
      connectionRef.current = connection
      setConnectionStatus('connected')
    } catch (error) {
      console.error('SignalR connection failed:', error)
      setConnectionStatus('disconnected')
      throw error
    }
  }, [url])

  const disconnect = useCallback(async () => {
    if (connectionRef.current) {
      await connectionRef.current.stop()
      connectionRef.current = null
      setConnectionStatus('disconnected')
    }
  }, [])

  const on = useCallback((eventName: string, callback: (...args: any[]) => void) => {
    if (connectionRef.current) {
      connectionRef.current.on(eventName, callback)
    }
  }, [])

  const off = useCallback((eventName: string) => {
    if (connectionRef.current) {
      connectionRef.current.off(eventName)
    }
  }, [])

  const invoke = useCallback(async (methodName: string, ...args: any[]) => {
    if (!connectionRef.current) {
      throw new Error('Connection not established')
    }
    return await connectionRef.current.invoke(methodName, ...args)
  }, [])

  useEffect(() => {
    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop()
      }
    }
  }, [])

  return { connect, disconnect, on, off, invoke, connectionStatus, connection: connectionRef.current }
}

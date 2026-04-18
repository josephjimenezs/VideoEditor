#!/bin/sh
# Ensure /data directory exists and has write permissions
mkdir -p /data
chmod 777 /data

# Run the application
exec "$@"




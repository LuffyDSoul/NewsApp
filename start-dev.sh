#!/bin/bash

echo "Starting NewsApp Backend and Frontend..."

# Start backend in background
echo "Starting .NET Backend..."
cd "src/NewsApp.HttpApi.Host"
dotnet run &
BACKEND_PID=$!

# Wait a moment for backend to start
sleep 5

# Start Angular frontend in background  
echo "Starting Angular Frontend..."
cd "../../NewsApp.Angular"
npm install && ng serve &
FRONTEND_PID=$!

echo "Backend PID: $BACKEND_PID"
echo "Frontend PID: $FRONTEND_PID"
echo ""
echo "Backend running at: https://localhost:44341"
echo "Frontend running at: http://localhost:4200"
echo ""
echo "Press Ctrl+C to stop both servers"

# Wait for user to press Ctrl+C
trap "kill $BACKEND_PID $FRONTEND_PID; exit" INT
wait
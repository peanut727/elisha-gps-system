// Global variables
let watchId = null;
let isTracking = false;

// Start tracking
export function startLocationTracking(dotNetRef) {
    if ("geolocation" in navigator) {
        const options = {
            enableHighAccuracy: true,
            timeout: 5000,
            maximumAge: 0
        };

        watchId = navigator.geolocation.watchPosition(
            (position) => handleLocationUpdate(position, dotNetRef),
            (error) => handleLocationError(error, dotNetRef),
            options
        );
        isTracking = true;
        dotNetRef.invokeMethodAsync('UpdateTrackingStatus', true);
    } else {
        console.error("Geolocation is not supported by this browser.");
        dotNetRef.invokeMethodAsync('OnError', 'Geolocation is not supported by this browser.');
        dotNetRef.invokeMethodAsync('UpdateTrackingStatus', false);
    }
}

// Stop tracking
export function stopLocationTracking(dotNetRef) {
    if (watchId) {
        navigator.geolocation.clearWatch(watchId);
        watchId = null;
        isTracking = false;
        dotNetRef.invokeMethodAsync('UpdateTrackingStatus', false);
    }
}

// Handle location update
async function handleLocationUpdate(position, dotNetRef) {
    const location = {
        latitude: position.coords.latitude,
        longitude: position.coords.longitude,
        speed: position.coords.speed ? position.coords.speed * 3.6 : 0, // Convert m/s to km/h
        heading: position.coords.heading || 0,
        accuracy: position.coords.accuracy || 0,
        timestamp: new Date().toISOString()
    };

    try {
        const response = await fetch('/api/Gps', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(location)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
    } catch (error) {
        console.error('Error sending GPS data:', error);
        dotNetRef.invokeMethodAsync('OnError', `Failed to send GPS data: ${error.message}`);
        
        // Stop tracking if we can't send data to the server
        if (error.message.includes('HTTP error')) {
            stopLocationTracking(dotNetRef);
        }
    }
}

// Handle location error
function handleLocationError(error, dotNetRef) {
    let errorMessage;
    switch(error.code) {
        case error.PERMISSION_DENIED:
            errorMessage = "Location permission denied by user.";
            break;
        case error.POSITION_UNAVAILABLE:
            errorMessage = "Location information unavailable.";
            break;
        case error.TIMEOUT:
            errorMessage = "Location request timed out.";
            break;
        default:
            errorMessage = "An unknown error occurred.";
    }
    
    console.error("Geolocation error:", errorMessage);
    dotNetRef.invokeMethodAsync('OnError', errorMessage);
    stopLocationTracking(dotNetRef);
}
let map;
let marker;
let lastPosition = null;
let tripLine;
let tripCoordinates = [];

export function loadLeaflet() {
    return new Promise((resolve, reject) => {
        if (window.L) {
            resolve();
            return;
        }
        const script = document.createElement('script');
        script.src = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js';
        script.integrity = 'sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo=';
        script.crossOrigin = '';
        script.onload = resolve;
        script.onerror = reject;
        document.head.appendChild(script);
    });
}

export function initializeLeafletMap() {
    try {
        console.log('Starting map initialization...');
        
        const mapElement = document.getElementById('map');
        if (!mapElement) {
            console.error('Map container not found');
            return false;
        }
        
        // Force a reflow to ensure dimensions are calculated
        mapElement.style.display = 'none';
        mapElement.offsetHeight; // Force reflow
        mapElement.style.display = '';
        
        console.log('Map container found, dimensions:', {
            width: mapElement.offsetWidth,
            height: mapElement.offsetHeight,
            clientWidth: mapElement.clientWidth,
            clientHeight: mapElement.clientHeight
        });

        if (typeof L === 'undefined') {
            console.error('Leaflet library not loaded');
            return false;
        }

        console.log('Leaflet library found:', L.version);

        map = L.map('map', {
            center: [14.5995, 120.9842],
            zoom: 13,
            zoomControl: true,
            attributionControl: true
        });

        console.log('Map object created');

        const tileLayer = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: ' OpenStreetMap contributors'
        });

        tileLayer.on('loading', () => console.log('Tiles loading...'));
        tileLayer.on('load', () => {
            console.log('Tiles loaded');
            // Trigger a resize event after tiles are loaded
            map.invalidateSize();
        });
        tileLayer.on('error', (e) => console.error('Tile error:', e));

        tileLayer.addTo(map);

        const vehicleIcon = L.divIcon({
            className: 'vehicle-marker-container',
            html: `<div class="vehicle-marker">
                    <svg viewBox="0 0 24 24" width="24" height="24">
                        <path d="M12 2L4 22h16L12 2z" fill="#00B14F" stroke="white" stroke-width="1"/>
                    </svg>
                   </div>`,
            iconSize: [24, 24],
            iconAnchor: [12, 12]
        });

        marker = L.marker([14.5995, 120.9842], {
            icon: vehicleIcon,
            rotationAngle: 0,
            rotationOrigin: 'center center'
        }).addTo(map);

        // Initialize the trip line
        tripLine = L.polyline([], {
            color: '#00B14F',
            weight: 3,
            opacity: 0.8,
            lineJoin: 'round'
        }).addTo(map);

        // Set up a resize observer to handle container size changes
        const resizeObserver = new ResizeObserver(() => {
            if (map) {
                console.log('Container resized, updating map...');
                map.invalidateSize();
            }
        });
        
        resizeObserver.observe(mapElement);

        // Additional size checks
        setTimeout(() => map.invalidateSize(), 100);
        setTimeout(() => map.invalidateSize(), 500);

        return true;
    } catch (error) {
        console.error('Error initializing map:', error);
        console.error('Stack trace:', error.stack);
        return false;
    }
}

export function updateMarkerPosition(lat, lng, speed) {
    if (!map || !marker) {
        console.error('Map or marker not initialized');
        return;
    }

    try {
        const newLatLng = L.latLng(lat, lng);
        
        // Update marker position
        marker.setLatLng(newLatLng);
        
        // Update trip line
        tripCoordinates.push(newLatLng);
        tripLine.setLatLngs(tripCoordinates);
        
        // Calculate and update marker rotation if we have a previous position
        if (lastPosition) {
            const bearing = calculateBearing(
                lastPosition.lat, lastPosition.lng,
                lat, lng
            );
            const markerElement = marker.getElement().querySelector('.vehicle-marker');
            if (markerElement) {
                markerElement.style.transform = `rotate(${bearing}deg)`;
            }

            // Calculate distance and update total
            const distance = calculateDistance(
                lastPosition.lat, lastPosition.lng,
                lat, lng
            );
            totalDistanceTraveled += distance;

            // Update Blazor component with new distance
            if (window.dotNetHelper) {
                window.dotNetHelper.invokeMethodAsync('UpdateMileage', totalDistanceTraveled);
            }
        }
        
        // Store current position for next update
        lastPosition = newLatLng;
        
        // Use panTo instead of flyTo for smoother updates
        map.panTo(newLatLng, {
            animate: true,
            duration: 0.5
        });
    } catch (error) {
        console.error('Error updating marker position:', error);
    }
}

function calculateBearing(lat1, lon1, lat2, lon2) {
    const toRad = (deg) => deg * Math.PI / 180;
    const toDeg = (rad) => rad * 180 / Math.PI;
    
    const φ1 = toRad(lat1);
    const φ2 = toRad(lat2);
    const Δλ = toRad(lon2 - lon1);

    const y = Math.sin(Δλ) * Math.cos(φ2);
    const x = Math.cos(φ1) * Math.sin(φ2) -
        Math.sin(φ1) * Math.cos(φ2) * Math.cos(Δλ);

    const θ = Math.atan2(y, x);
    return (toDeg(θ) + 360) % 360;
}

function calculateDistance(lat1, lon1, lat2, lon2) {
    const R = 6371; // Earth's radius in kilometers
    const dLat = toRad(lat2 - lat1);
    const dLon = toRad(lon2 - lon1);
    const a = 
        Math.sin(dLat/2) * Math.sin(dLat/2) +
        Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * 
        Math.sin(dLon/2) * Math.sin(dLon/2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
    return R * c; // Distance in kilometers
}

function toRad(degrees) {
    return degrees * (Math.PI/180);
}

let totalDistanceTraveled = 0;

// Add this function to set up the .NET helper
window.setDotNetHelper = (helper) => {
    window.dotNetHelper = helper;
    console.log('DotNet helper initialized');
};

// Add function to clear trip line
export function clearTripLine() {
    console.log('Clearing trip line...');
    try {
        // Reset coordinates
        tripCoordinates = [];
        lastPosition = null;
        totalDistanceTraveled = 0;

        // Clear the polyline
        if (tripLine) {
            console.log('Clearing polyline');
            tripLine.setLatLngs([]);
        } else {
            console.log('Trip line not initialized');
        }

        // Reset marker rotation if it exists
        if (marker && marker.getElement()) {
            const markerElement = marker.getElement().querySelector('.vehicle-marker');
            if (markerElement) {
                markerElement.style.transform = '';
            }
        }

        // Notify Blazor of reset mileage if helper exists
        if (window.dotNetHelper) {
            window.dotNetHelper.invokeMethodAsync('UpdateMileage', 0);
        }

        console.log('Trip line cleared successfully');
        return true;
    } catch (error) {
        console.error('Error clearing trip line:', error);
        return false;
    }
}
window.leafletInterop = (() => {
    const maps = {};

    function initMap(elementId, lat, lng, zoom, dotNetRef) {
        if (maps[elementId]) {
            maps[elementId].map.remove();
            delete maps[elementId];
        }
        const map = L.map(elementId).setView([lat, lng], zoom ?? 12);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors'
        }).addTo(map);
        // suppressNext: true to ignore the moveend fired by the initial setView above
        maps[elementId] = { map, markers: [], dotNetRef, suppressNext: true, debounceTimer: null };

        map.on('moveend', () => {
            const entry = maps[elementId];
            if (!entry) return;
            if (entry.suppressNext) { entry.suppressNext = false; return; }
            if (!entry.dotNetRef) return;
            clearTimeout(entry.debounceTimer);
            entry.debounceTimer = setTimeout(() => {
                const center = entry.map.getCenter();
                const bounds = entry.map.getBounds();
                const ne = bounds.getNorthEast();
                const radiusKm = entry.map.distance(center, ne) / 1000;
                entry.dotNetRef.invokeMethodAsync('NotifyMapMoved', center.lat, center.lng, radiusKm);
            }, 600);
        });
    }

    function setMarkers(elementId, markers, dotNetRef) {
        const entry = maps[elementId];
        if (!entry) return;

        entry.markers.forEach(m => m.remove());
        entry.markers = [];

        markers.forEach(ws => {
            const marker = L.marker([ws.latitude, ws.longitude])
                .addTo(entry.map)
                .bindPopup(`<b>${ws.name}</b>`);

            marker.on('click', () => {
                if (dotNetRef) dotNetRef.invokeMethodAsync('OnMarkerClicked', ws.id);
            });

            entry.markers.push(marker);
        });
    }

    function setView(elementId, lat, lng, zoom) {
        const entry = maps[elementId];
        if (!entry) return;
        // Suppress the moveend that setView will fire — it's a programmatic re-center, not a user gesture
        entry.suppressNext = true;
        entry.map.setView([lat, lng], zoom ?? 12);
    }

    function panTo(elementId, lat, lng) {
        const entry = maps[elementId];
        if (!entry) return;
        entry.suppressNext = true;
        entry.map.panTo([lat, lng]);
    }

    function destroyMap(elementId) {
        if (maps[elementId]) {
            maps[elementId].map.remove();
            delete maps[elementId];
        }
    }

    return { initMap, setMarkers, setView, panTo, destroyMap };
})();

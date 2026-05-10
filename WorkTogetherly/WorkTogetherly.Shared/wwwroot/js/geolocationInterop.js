window.getCurrentPosition = () =>
    new Promise((resolve, reject) =>
        navigator.geolocation.getCurrentPosition(
            pos => resolve({ latitude: pos.coords.latitude, longitude: pos.coords.longitude }),
            err => reject(err.message),
            { enableHighAccuracy: true, timeout: 10000 }
        )
    );

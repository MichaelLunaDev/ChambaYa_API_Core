const ChambaYa = {
    apiBase: (window.ChambaYaApiBase || '') + '/api/Chamba',
    usuarioId: 1,

    async getJson(url, options = {}) {
        const resp = await fetch(url, {
            headers: { 'Content-Type': 'application/json' },
            ...options
        });
        if (!resp.ok) throw new Error('Error en la petición (' + resp.status + ')');
        return resp.json();
    },

    alertaError(mensaje) {
        return Swal.fire({
            icon: 'error',
            title: 'Ocurrió un error',
            text: mensaje,
            confirmButtonColor: '#0d6efd'
        });
    },

    alertaExito(titulo, mensaje) {
        return Swal.fire({
            icon: 'success',
            title: titulo,
            text: mensaje,
            confirmButtonColor: '#0d6efd'
        });
    }
};

(() => {
    const form = document.getElementById('formCrearOferta');
    const btnGuardar = document.getElementById('btnGuardar');

    const validar = () => {
        let valido = true;
        form.querySelectorAll('[required]').forEach(campo => {
            const ok = campo.value.trim() !== '';
            campo.classList.toggle('is-invalid', !ok);
            if (!ok) valido = false;
        });
        return valido;
    };

    form.querySelectorAll('[required]').forEach(campo => {
        campo.addEventListener('input', () => campo.classList.remove('is-invalid'));
    });

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (!validar()) {
            ChambaYa.alertaError('Completa todos los campos obligatorios.');
            return;
        }

        const oferta = {
            idCategoria: parseInt(document.getElementById('idCategoria').value, 10),
            titulo: document.getElementById('titulo').value.trim(),
            descripcion: document.getElementById('descripcion').value.trim(),
            salario: parseFloat(document.getElementById('salario').value),
            ubicacion: document.getElementById('ubicacion').value.trim(),
            modalidad: document.getElementById('modalidad').value,
            requisitos: document.getElementById('requisitos').value.trim()
        };

        btnGuardar.disabled = true;
        btnGuardar.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Publicando...';

        try {
            const resp = await ChambaYa.getJson(ChambaYa.apiBase + '/CrearOferta', {
                method: 'POST',
                body: JSON.stringify(oferta)
            });

            if (resp.success) {
                await ChambaYa.alertaExito('¡Oferta publicada!', resp.message);
                window.location.href = '/';
            } else {
                await ChambaYa.alertaError(resp.message || 'No se pudo publicar la oferta.');
            }
        } catch (error) {
            await ChambaYa.alertaError('No se pudo conectar con el servidor. Inténtalo nuevamente.');
        } finally {
            btnGuardar.disabled = false;
            btnGuardar.innerHTML = '<i class="bi bi-send me-1"></i>Publicar oferta';
        }
    });
})();

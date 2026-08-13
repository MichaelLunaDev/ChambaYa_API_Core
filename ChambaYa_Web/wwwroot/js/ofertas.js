(() => {
    let ofertas = [];
    let seleccionadas = new Set();

    const contenedor = document.getElementById('contenedorOfertas');
    const sinResultados = document.getElementById('sinResultados');
    const inputBuscar = document.getElementById('buscarOferta');
    const selectModalidad = document.getElementById('filtroModalidad');
    const btnCarrito = document.getElementById('btnCarritoFlotante');
    const contador = document.getElementById('contadorCarrito');

    const modalidadIcono = (modalidad) => {
        const m = (modalidad || '').toLowerCase();
        if (m.includes('remoto')) return 'bi-house-check';
        if (m.includes('hibrido') || m.includes('híbrido')) return 'bi-arrow-left-right';
        if (m.includes('medio')) return 'bi-clock';
        if (m.includes('completo') || m.includes('full')) return 'bi-hourglass-split';
        return 'bi-geo-alt';
    };

    const formatearSalario = (valor) => {
        return new Intl.NumberFormat('es-PE', { style: 'currency', currency: 'PEN', maximumFractionDigits: 0 }).format(valor);
    };

    const tarjetaOferta = (oferta) => {
        const seleccionada = seleccionadas.has(oferta.idOferta);
        return `
            <div class="col-md-6 col-lg-4">
                <div class="card oferta-card h-100 ${seleccionada ? 'seleccionada' : ''}" data-id="${oferta.idOferta}">
                    <div class="card-body d-flex flex-column">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <span class="badge badge-categoria"><i class="bi bi-tags me-1"></i>${escapeHtml(oferta.nombreCategoria)}</span>
                            <span class="text-muted small"><i class="bi bi-calendar3 me-1"></i>${new Date(oferta.fechaPublicacion).toLocaleDateString('es-PE')}</span>
                        </div>
                        <h5 class="card-title">${escapeHtml(oferta.titulo)}</h5>
                        <p class="card-text text-muted flex-grow-1">${escapeHtml(recortar(oferta.descripcion, 140))}</p>
                        <div class="d-flex flex-wrap gap-2 small mb-3">
                            <span class="badge text-bg-light border"><i class="bi ${modalidadIcono(oferta.modalidad)} me-1"></i>${escapeHtml(oferta.modalidad)}</span>
                            <span class="badge text-bg-light border"><i class="bi bi-geo-alt me-1"></i>${escapeHtml(oferta.ubicacion)}</span>
                        </div>
                        <p class="salario-texto mb-3"><i class="bi bi-cash-coin me-1"></i>${formatearSalario(oferta.salario)}</p>
                        <button type="button" class="btn btn-outline-primary btn-seleccionar ${seleccionada ? 'seleccionado' : ''}" data-id="${oferta.idOferta}">
                            <i class="bi ${seleccionada ? 'bi-check-circle-fill' : 'bi-circle'} me-1"></i>
                            ${seleccionada ? 'Seleccionada' : 'Agregar al carrito'}
                        </button>
                    </div>
                </div>
            </div>`;
    };

    const escapeHtml = (texto) => {
        const div = document.createElement('div');
        div.textContent = texto ?? '';
        return div.innerHTML;
    };

    const recortar = (texto, max) => {
        if (!texto) return '';
        return texto.length > max ? texto.substring(0, max) + '...' : texto;
    };

    const renderizar = () => {
        const termino = inputBuscar.value.trim().toLowerCase();
        const modalidad = selectModalidad.value;

        const filtradas = ofertas.filter(o => {
            const coincideTexto = !termino ||
                o.titulo.toLowerCase().includes(termino) ||
                o.ubicacion.toLowerCase().includes(termino) ||
                o.descripcion.toLowerCase().includes(termino) ||
                o.requisitos.toLowerCase().includes(termino) ||
                o.nombreCategoria.toLowerCase().includes(termino);
            const coincideModalidad = !modalidad || o.modalidad === modalidad;
            return coincideTexto && coincideModalidad;
        });

        if (filtradas.length === 0) {
            contenedor.innerHTML = '';
            sinResultados.classList.remove('d-none');
            return;
        }

        sinResultados.classList.add('d-none');
        contenedor.innerHTML = filtradas.map(tarjetaOferta).join('');
    };

    const actualizarCarrito = () => {
        contador.textContent = seleccionadas.size;
        btnCarrito.style.display = seleccionadas.size > 0 ? 'inline-flex' : 'none';
        renderizar();
    };

    const toggleSeleccion = (id) => {
        if (seleccionadas.has(id)) seleccionadas.delete(id);
        else seleccionadas.add(id);
        actualizarCarrito();
    };

    const postular = async () => {
        if (seleccionadas.size === 0) return;

        const ofertasSeleccionadas = ofertas.filter(o => seleccionadas.has(o.idOferta));
        const resumen = ofertasSeleccionadas.map(o => `<li>${escapeHtml(o.titulo)}</li>`).join('');

        const { isConfirmed } = await Swal.fire({
            icon: 'question',
            title: 'Confirmar postulación',
            html: `<p class="mb-2">Vas a postularte a las siguientes ofertas:</p><ul class="text-start">${resumen}</ul>`,
            confirmButtonText: 'Sí, postularme',
            cancelButtonText: 'Cancelar',
            showCancelButton: true,
            confirmButtonColor: '#0d6efd',
            reverseButtons: true
        });

        if (!isConfirmed) return;

        try {
            Swal.fire({
                title: 'Procesando...',
                text: 'Registrando tus postulaciones',
                allowOutsideClick: false,
                didOpen: () => Swal.showLoading()
            });

            const resp = await ChambaYa.getJson(ChambaYa.apiBase + '/ProcesarCarrito', {
                method: 'POST',
                body: JSON.stringify({
                    idUsuario: ChambaYa.usuarioId,
                    ofertasSeleccionadas: [...seleccionadas]
                })
            });

            if (resp.success) {
                await ChambaYa.alertaExito('¡Postulación exitosa!', resp.message);
                seleccionadas.clear();
                actualizarCarrito();
            } else {
                await ChambaYa.alertaError(resp.message || 'No se pudo procesar la postulación.');
            }
        } catch (error) {
            await ChambaYa.alertaError('No se pudo conectar con el servidor. Inténtalo nuevamente.');
        }
    };

    contenedor.addEventListener('click', (e) => {
        const btn = e.target.closest('.btn-seleccionar');
        if (btn) toggleSeleccion(parseInt(btn.dataset.id, 10));
    });

    inputBuscar.addEventListener('input', renderizar);
    selectModalidad.addEventListener('change', renderizar);
    btnCarrito.addEventListener('click', postular);

    const cargarOfertas = async () => {
        try {
            const resp = await ChambaYa.getJson(ChambaYa.apiBase + '/Ofertas');
            if (resp.success) {
                ofertas = resp.data || [];
                renderizar();
            } else {
                contenedor.innerHTML = '<div class="col-12 text-center py-5"><p class="text-danger">' + escapeHtml(resp.message) + '</p></div>';
            }
        } catch (error) {
            contenedor.innerHTML = `
                <div class="col-12 text-center py-5">
                    <i class="bi bi-wifi-off display-3 estado-vacio"></i>
                    <h4 class="mt-3">No se pudo conectar con el servidor</h4>
                    <p class="estado-vacio">Verifica que la API esté disponible e intenta nuevamente.</p>
                    <button class="btn btn-chamba mt-2" onclick="location.reload()"><i class="bi bi-arrow-clockwise me-1"></i>Reintentar</button>
                </div>`;
        }
    };

    cargarOfertas();
})();

(() => {
    let ofertas = [];
    let seleccionadas = new Set();

    const contenedor = document.getElementById('contenedorOfertas');
    const sinResultados = document.getElementById('sinResultados');
    const inputBuscar = document.getElementById('buscarOferta');
    const selectModalidad = document.getElementById('filtroModalidad');
    const btnCarrito = document.getElementById('btnCarritoFlotante');
    const contador = document.getElementById('contadorCarrito');
    const contadorOfertas = document.getElementById('contadorOfertas');

    const iconosCategoria = {
        'tecnología': 'bi-code-slash',
        'ventas': 'bi-graph-up-arrow',
        'atención al cliente': 'bi-headset',
        'diseño': 'bi-vector-pen',
        'administración': 'bi-briefcase'
    };

    const iconoCategoria = (nombre) => {
        const clave = (nombre || '').toLowerCase();
        return iconosCategoria[clave] || 'bi-star';
    };

    const formatearSalario = (valor) => {
        return new Intl.NumberFormat('es-PE', { style: 'currency', currency: 'PEN', maximumFractionDigits: 0 }).format(valor);
    };

    const formateaTiempo = (fecha) => {
        const f = new Date(fecha);
        const hoy = new Date();
        // Reset times to compare just dates
        f.setHours(0,0,0,0);
        hoy.setHours(0,0,0,0);
        const diff = Math.floor((hoy - f) / (1000 * 60 * 60 * 24));
        if (diff === 0) return 'Hoy';
        if (diff === 1) return 'Ayer';
        if (diff < 0) return 'Próximamente'; // For future dates like 2026
        return `Hace ${diff} días`;
    };

    const tarjetaOferta = (oferta, indice) => {
        const seleccionada = seleccionadas.has(oferta.idOferta);
        return `
            <div class="col-md-6 col-lg-4">
                <div class="modern-card h-100 d-flex flex-column animar-entrada ${seleccionada ? 'seleccionada' : ''}" data-id="${oferta.idOferta}" style="animation-delay: ${Math.min(indice * 50, 400)}ms">
                    <div class="d-flex justify-content-between align-items-start mb-3">
                        <div class="d-flex align-items-center gap-3">
                            <div class="icon-box">
                                <i class="bi ${iconoCategoria(oferta.nombreCategoria)}"></i>
                            </div>
                            <div>
                                <h5 class="card-title-modern text-truncate" style="max-width:180px;" title="${escapeHtml(oferta.titulo)}">${escapeHtml(oferta.titulo)}</h5>
                                <span class="card-meta">${escapeHtml(oferta.nombreCategoria)} • ${formateaTiempo(oferta.fechaPublicacion)}</span>
                            </div>
                        </div>
                        <div class="salary-tag">${formatearSalario(oferta.salario)}</div>
                    </div>
                    
                    <p class="text-slate-500 small mb-4 flex-grow-1">${escapeHtml(recortar(oferta.descripcion, 110))}</p>
                    
                    <div class="d-flex flex-wrap gap-2 mb-4">
                        <span class="tag-chip"><i class="bi bi-geo-alt text-slate-400"></i> ${escapeHtml(oferta.ubicacion)}</span>
                        <span class="tag-chip"><i class="bi bi-laptop text-slate-400"></i> ${escapeHtml(oferta.modalidad)}</span>
                    </div>
                    
                    <button type="button" class="btn btn-seleccionar ${seleccionada ? 'seleccionado' : ''}" data-id="${oferta.idOferta}">
                        <i class="bi ${seleccionada ? 'bi-check2' : 'bi-plus-lg'} me-1"></i>
                        ${seleccionada ? 'Seleccionada' : 'Seleccionar'}
                    </button>
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

        if (contadorOfertas) {
            contadorOfertas.innerHTML = `<i class="bi bi-briefcase me-1"></i> ${filtradas.length} oferta${filtradas.length === 1 ? '' : 's'}`;
        }

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
        const visible = seleccionadas.size > 0;
        if (visible) {
            btnCarrito.style.display = 'inline-flex';
            btnCarrito.classList.remove('pulso');
            void btnCarrito.offsetWidth;
            btnCarrito.classList.add('pulso');
        } else {
            btnCarrito.style.display = 'none';
        }
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
        const resumen = ofertasSeleccionadas.map(o => `<li class="mb-1 text-slate-700">${escapeHtml(o.titulo)}</li>`).join('');

        const { isConfirmed } = await Swal.fire({
            title: '¿Confirmar postulación?',
            html: `<p class="mb-3 text-slate-500">Enviarás tu perfil a las siguientes ofertas:</p><ul class="text-start ps-4">${resumen}</ul>`,
            showCancelButton: true,
            confirmButtonText: 'Enviar postulación',
            cancelButtonText: 'Cancelar',
            reverseButtons: true,
            customClass: {
                confirmButton: 'btn btn-primary-modern rounded-pill px-4 py-2',
                cancelButton: 'btn btn-light rounded-pill px-4 py-2 text-slate-600',
                popup: 'rounded-4 border-0 shadow-lg'
            },
            buttonsStyling: false
        });

        if (!isConfirmed) return;

        try {
            Swal.fire({
                title: 'Enviando...',
                text: 'Procesando tu solicitud',
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
                await Swal.fire({
                    icon: 'success',
                    title: '¡Postulación enviada!',
                    text: 'Las empresas revisarán tu perfil pronto.',
                    confirmButtonText: 'Genial',
                    customClass: { confirmButton: 'btn btn-primary-modern rounded-pill px-4', popup: 'rounded-4' },
                    buttonsStyling: false
                });
                seleccionadas.clear();
                actualizarCarrito();
            } else {
                await ChambaYa.alertaError(resp.message || 'Error al procesar la postulación.');
            }
        } catch (error) {
            await ChambaYa.alertaError('Error de conexión. Inténtalo nuevamente.');
        }
    };

    contenedor.addEventListener('click', (e) => {
        const btn = e.target.closest('.btn-seleccionar');
        if (btn) toggleSeleccion(parseInt(btn.dataset.id, 10));
    });

    inputBuscar.addEventListener('input', renderizar);
    selectModalidad.addEventListener('change', renderizar);
    btnCarrito.addEventListener('click', postular);

    window.limpiarFiltros = () => {
        inputBuscar.value = '';
        selectModalidad.value = '';
        renderizar();
    };

    const cargarOfertas = async () => {
        try {
            const resp = await ChambaYa.getJson(ChambaYa.apiBase + '/Ofertas');
            if (resp.success) {
                ofertas = resp.data || [];
                renderizar();
            } else {
                contenedor.innerHTML = '<div class="col-12 text-center py-5 text-danger">' + escapeHtml(resp.message) + '</div>';
            }
        } catch (error) {
            contenedor.innerHTML = `
                <div class="col-12 text-center py-5">
                    <i class="bi bi-wifi-off empty-icon"></i>
                    <h4 class="fw-bold text-slate-900">Sin conexión</h4>
                    <button class="btn btn-light rounded-pill mt-3" onclick="location.reload()">Reintentar</button>
                </div>`;
        }
    };

    cargarOfertas();
})();

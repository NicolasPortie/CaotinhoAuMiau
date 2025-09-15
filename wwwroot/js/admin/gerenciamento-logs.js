document.addEventListener('DOMContentLoaded', function() {
    // Funcionalidade do seletor de itens por página
    const selectItensPorPagina = document.getElementById('selectItensPorPagina');
    if (selectItensPorPagina) {
        selectItensPorPagina.addEventListener('change', function() {
            const itensPorPagina = this.value;
            const urlParams = new URLSearchParams(window.location.search);
            urlParams.set('itensPorPagina', itensPorPagina);
            urlParams.set('pagina', '1'); // Reset para primeira página
            window.location.href = window.location.pathname + '?' + urlParams.toString();
        });
    }
});
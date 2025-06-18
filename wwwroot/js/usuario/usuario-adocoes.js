$(document).ready(function() {
    // Manipulador para o seletor de itens por página
    $('#selectItensPorPagina').change(function() {
        var itensPorPagina = $(this).val();
        var url = '@Url.Action("Listar", "Adocao")' + '?pagina=1&itensPorPagina=' + itensPorPagina;
        window.location.href = url;
    });
    
    // Corrigir problema de paginação
    $(document).on('click', '.pagination .page-link', function(e) {
        e.preventDefault();
        var url = $(this).attr('href');
        if (url) {
            window.location.href = url;
        }
        return false;
    });
    
    // Código existente abaixo
}); 
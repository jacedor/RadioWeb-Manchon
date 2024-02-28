function LoadTiposExploracionesPorGurpo (oidGrupo) {
    $.ajax({
        type: 'POST',
        url: '/TipoExploracion/ListaTiposExploraciones',
        data: { oidGrupo: oidGrupo },
        async: 'false',
        beforeSend: function () {
            $(".spiner-texploracion").removeClass("hide");
            $("#placeholderTexploracion").addClass("hide");
            $("#placeholderTexploracion").html("");
        },
        success: function (data) {
            $("#placeholderTexploracion").html(data);
            $("#placeholderTexploracion").removeClass("hide");
            $(".spiner-texploracion").addClass("hide");
        }
    });
}
function LoadAparatosPorGrupo(oidGrupo) {
    $.ajax({
        type: 'POST',
        url: '/Aparato/ListaAparatos',
        data: { oidGrupo: oidGrupo },
        async: 'false',
        beforeSend: function () {
            $(".spiner-aparatos").removeClass("hide");
            $("#placeholderAparatos").addClass("hide");
            $("#placeholderAparatos").html("");
        },
        success: function (data) {
            $("#placeholderAparatos").html(data);
            $("#placeholderAparatos").removeClass("hide");
            $(".spiner-aparatos").addClass("hide");
        }
    });
}
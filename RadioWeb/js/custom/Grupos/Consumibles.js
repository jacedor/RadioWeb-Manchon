function LoadConsumiblesPorGrupo(oidGrupo) {
    $.ajax({
        type: 'POST',
        url: '/Consumible/ListaConsumibles',
        data: { oidGrupo: oidGrupo },
        async: 'false',
        beforeSend: function () {
            $(".spiner-consumibles").removeClass("hide");
            $("#placeholderConsumibles").addClass("hide");
            $("#placeholderConsumibles").html("");
        },
        success: function (data) {
            $("#placeholderConsumibles").html(data);
            $("#placeholderConsumibles").removeClass("hide");
            $(".spiner-consumibles").addClass("hide");
        }
    });
}
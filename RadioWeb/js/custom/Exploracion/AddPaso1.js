function SuccessFiltros(data) {

}
function CompleteFiltros() {
    $("#spiner-cargando-pacientes").addClass('hide');
}
function BeginFiltros() {
    $("#spiner-cargando-pacientes").removeClass('hide');

}
function FailureFiltro() {
    alert('fallo');
}

$(document).on('keyup', '#pacienteFilter,#dniFilter', $.debounce(500, function () {
   
    var busquedaPorDni = $("#dniFilter").val().length > 3 && $("#dniFilter").val() !== "";
    if (busquedaPorDni || $("#pacienteFilter").val().length > 3 && $("#pacienteFilter").val() !== "") {
        $("#PACIENTE1").val($("#pacienteFilter").val());
        $("#BuscarPaciente").trigger("click");
    }
}));


$(document).ready(function () {
    $("li[data-view]").removeClass('active');
    $("li[data-view=ViewListaDia]").addClass('active');
    $("[data-view=ViewListaDia]").parents("ul").removeClass("collapse");




    $("#BuscarPaciente").trigger("click");


});


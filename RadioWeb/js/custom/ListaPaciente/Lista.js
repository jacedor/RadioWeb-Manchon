

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
        $("#BuscarPaciente").trigger("click");
    }
}));
$(document).on('click', '.enabledFusionar', function () {
    
    $("#fusionarPaciente").toggleClass('disabled');
        //.closest('tr').toggleClass('ACTIVA');
 
});

$(document).on('click', '#FusionarAction', function () {
   //pacientes a fusionar
    var arr = [];
    $("#PacienteList > table tr.ACTIVA").each(function () {
        var $this = $(this);
        arr.push($this.data('oid'));
    });
    var pacientePermanece = $("#PacienteListFusionar > table tr.ACTIVA").data('oid');


    var url = "/Paciente/Fusionar/"; //The Url to the Action  Method of the Controller
    var paciente = {}; //The Object to Send Data Back to the Controller
    paciente.PACIENTE1 = $("#BuscarPacienteDuplicar").val();
    $.ajax({
        type: 'POST',
        url: url,
        data: {
            pacientes: arr,
            pacientePrincipal: pacientePermanece
        },
      
        success: function (evt) {
            $("#modal-form-Fusionar").modal('hide');
            $("#pacienteFilter").keyup();
        }
    });
});


$('#fusionarPaciente').click(function (e) {

    e.preventDefault();
    e.stopPropagation();
    e.stopImmediatePropagation();
    $("#modal-form-Fusionar").modal('show');    
    return false;
});

$("#BuscarPacienteDuplicar").keyup($.debounce(250, function () {
    var data = $(this).val();
    var url = "/Paciente/BuscarPacienteFusionar/"; //The Url to the Action  Method of the Controller
    var paciente = {}; //The Object to Send Data Back to the Controller
    paciente.PACIENTE1 = $("#BuscarPacienteDuplicar").val();

    if ($("#BuscarPacienteDuplicar").val().length > 3 && $("#BuscarPacienteDuplicar").val() !== "") {
        $.ajax({
            type: 'POST',
            url: url,
            data: paciente,
            dataType: "html",
            success: function (evt) {
                $('#PacienteListFusionar').empty();
                $('#PacienteListFusionar').html(evt);
            },
        });
    }

}));

$('#modal-form-Fusionar').on('shown.bs.modal', function (e) {
    
    $("#BuscarPacienteDuplicar").val($('#pacienteFilter').val());
    $("#BuscarPacienteDuplicar").keyup();
});




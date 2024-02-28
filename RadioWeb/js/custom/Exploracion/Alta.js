var filtrosBusqueda;

function OnFiltersLoaded(data) {

    filtrosBusqueda = data;
    if ($('#PacienteList tbody').length > 0) {
        $.ajax({
            type: 'POST',
            url: '/Paciente/Paso1AltaExploracion',
            data: JSON.stringify(filtrosBusqueda),
            contentType: 'application/json; charset=utf-8',
            beforeSend: function () {
                $('#spiner-cargando-pacientes').removeClass('hide');
            },

            success: function (data) {
                $('#PacienteList tbody').html(data);
                $('#spiner-cargando-pacientes').addClass('hide');
            },
        });
    }
    

}

function LoadFiltersPacientes() {

    $.post('/Settings/LoadFiltrosPaciente', OnFiltersLoaded);

}

function SaveFiltersPacientes() {
    filtrosBusqueda = {};
    filtrosBusqueda.Nombre = $("#pacienteFilter").val();
    filtrosBusqueda.DNI = $("#dniFilter").val();

    $.post('/Settings/SaveFiltrosPaciente',
        filtrosBusqueda,
        OnFiltersLoaded(filtrosBusqueda));

}



$(document).on('keyup', '#pacienteFilter,#dniFilter', $.debounce(500, function () {

    var busquedaPorDni = $("#dniFilter").val().length > 3 && $("#dniFilter").val() !== "";
    if (busquedaPorDni|| $("#pacienteFilter").val().length > 3 && $("#pacienteFilter").val() !== "") {
        SaveFiltersPacientes();
    }
}));

$(document).on('click', '#btnGuardarAltaExploracion', function () { 
    
    sessionStorage.vistaActual = "ViewListaDia";
});

// evento click del SELECT DEL APARATO QUE RELLENA LAS EXPLORACIONES
$(document).on('change keyup', '#IOR_APARATO, #IOR_ENTIDADPAGADORA', function () {


    var IOR_ENTIDADPAGADORA = $("#IOR_ENTIDADPAGADORA").val();
    sessionStorage.textGrupo = ' ';
    sessionStorage.textAparato = $("#IOR_APARATO option[value=" + $("#IOR_APARATO").val() + "]").data('cod');
    sessionStorage.valAparato = $("#IOR_APARATO option[value=" + $("#IOR_APARATO").val() + "]").val();

    var cantidad = $('#Cantidad').val('');
    var options = {
        url: "/Exploracion/GetTipoExploraciones",
        data: "IOR_APARATO=" + sessionStorage.valAparato + "&" + "IOR_MUTUA=" + IOR_ENTIDADPAGADORA
    };

    $.ajax(options).success(function (data) {

        var sel = $('#IOR_TIPOEXPLORACION');
        sel.empty();
        var markup = '';
        for (var x = 0; x < data.length; x++) {
            if (!data[x].FIL.indexOf("OBSOLETO") !== -1) {
                markup += '<option value="' + data[x].OID + '" data-cod="' + data[x].FIL + '" data-text-value="' + data[x].DES_FIL + '">' + data[x].FIL + ' - ' + data[x].DES_FIL + '</option>';
            }
        }
        sel.html(markup).show();

        $("#IOR_TIPOEXPLORACION option:first").attr('selected', 'selected');
    });
});



// evento click del SELECT DE TIPO DE EXPLORACION QUE CAMBIA EL PRECIO
$(document).on('change keyup', '#IOR_TIPOEXPLORACION', function () {

    var IOR_TIPOEXPLORACION = $("#IOR_TIPOEXPLORACION").val();
    var IOR_ENTIDADPAGADORA = $("#IOR_ENTIDADPAGADORA").val();


    var options = {
        url: "/Exploracion/GetPrecioExploracion",
        data: ({ IOR_TIPOEXPLORACION: IOR_TIPOEXPLORACION, IOR_MUTUA: IOR_ENTIDADPAGADORA }),
        dataType: "html",
        type: "GET"
    };
    $.ajax(options).complete(function (data) {
        $("#CANTIDAD").val(data.responseText);
    });

    //Nos Traemos el texto de la mutua
    $.ajax({
        type: 'POST',
        url: '/Exploracion/GetTextoExploraciones',
        data: { oidAparato: IOR_TIPOEXPLORACION },
        success: function (data) {
            if (data.length > 0) {

                $("#textoDeLaMutua").removeClass('hide');
                $("#textoDeLaMutua").find('.ibox-content').html('').html(data);

            }
            else {
                $("#textoDeLaMutua").addClass('hide');
                $("#ExploracionResumen").show();
            }
        }
    });
});

$(document).on('change keyup', '#Q_ALFA', function () {

    var IOR_TIPOEXPLORACION = $("#IOR_TIPOEXPLORACION").val();
    var IOR_ENTIDADPAGADORA = $("#IOR_ENTIDADPAGADORA").val();
    var qAlfa = $("#Q_ALFA").val();
    if (IOR_TIPOEXPLORACION > 0) {
        var options = {
            url: "/Exploracion/GetPrecioExploracionConDescuento",
            data: (
                {
                    IOR_TIPOEXPLORACION: IOR_TIPOEXPLORACION,
                    IOR_MUTUA: IOR_ENTIDADPAGADORA,
                    Q_ALFA: qAlfa
                }
            ),
            dataType: "html",
            type: "GET"
        };

        $.ajax(options).complete(function (data) {
            $("#CANTIDAD").val(data.responseText);
        });
    } else {
        if (qAlfa!=="0") {
            toastr.error('', 'Para haces Descuento tiene que elegir tipo de exploración!', { timeOut: 5000 });
            $("#Q_ALFA").val("0").change();
        }
       
    }
    
});

$(document).on('ifClicked', "input.TipoExploracion", function (event) {
    
    var oExploracion = {};

    
    oExploracion.HORA = $("#HORA").val();
    oExploracion.FECHA = $("#FECHA").val();
    oExploracion.OID = $("#OID").val();
    oExploracion.IOR_APARATO = $("#IOR_APARATO").val();
    oExploracion.IOR_PACIENTE = $("#IOR_PACIENTE").val();

    var ENTIDAD_PAGADORA = {};
    oExploracion.ENTIDAD_PAGADORA = ENTIDAD_PAGADORA;
    switch ($(this).val()) {
        case "PRI":
            oExploracion.ENTIDAD_PAGADORA.OWNER = 1;
            //oExploracion.ENTIDAD_PAGADORA.OID = 3820080;
            break;
        case "MUT":
            oExploracion.ENTIDAD_PAGADORA.OWNER = 2;
            break;
        case "ICS":
            oExploracion.ENTIDAD_PAGADORA.OWNER = 3;
            break;
        default:

    }
    var request = $.ajax({
        url: "/Exploracion/GetPanelExploracion",
        data: JSON.stringify({ oExploracion: oExploracion }),
        contentType: 'application/json',
        type: "POST"
    });

    $("#GuardarPaso3").empty();
    request.done(function (data) {
        var $target = $("#GuardarPaso3");
     
        var $newHtml = $(data);
        $target.html(data);
        $(".fecha-picker,.date-picker").datepicker({
            format: "dd/mm/yyyy",
            todayBtn: true,
            language: "es",
            autoclose: true,
            todayHighlight: true
        });

    });
    request.fail(function (jqXHR, textStatus) {
        

    });

});



$(function () {
    if (filtrosBusqueda === undefined) {
        LoadFiltersPacientes();
    }
    $("#pacienteFilter").focus();
});
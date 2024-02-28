var filtrosBusqueda;

function OnFiltersLoaded(data) {

    filtrosBusqueda = data;

    $.ajax({
        type: 'POST',
        url: '/Paciente/Paso1AltaExploracionMultiple',
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

function LoadFiltersPacientes() {

    $.post('/Settings/LoadFiltrosPaciente', OnFiltersLoaded);

}

function SaveFiltersPacientes() {
    filtrosBusqueda = {};
    filtrosBusqueda.Nombre = $("#pacienteFilter").val();
    $.post('/Settings/SaveFiltrosPaciente', filtrosBusqueda, OnFiltersLoaded(filtrosBusqueda));

}



$(document).on('keyup', '#pacienteFilter', $.debounce(500, function () {

    if ($("#pacienteFilter").val().length > 3 && $("#pacienteFilter").val() != "") {
        SaveFiltersPacientes();

    }
}));

$(document).on('click', '#btnGuardarAltaExploracion', function () {
    sessionStorage.vistaActual = "ViewListaDia";
});

// evento click del SELECT DEL APARATO QUE RELLENA LAS EXPLORACIONES
$(document).on('change keyup', '#CID', function () {


    var IOR_ENTIDADPAGADORA = $("#CID").val();
   

    $('.entidadPagadora').val(IOR_ENTIDADPAGADORA);


    $(".entidadPagadora").each(function () {
        $this = $(this);

        var ior_aparato = $(".aparatoExploracion[data-indice=" + $this.data('indice') + "]").val();
        var options = {
            url: "/Exploracion/GetTipoExploraciones",
            data: "IOR_APARATO=" + ior_aparato + "&" + "IOR_MUTUA=" + $this.val(),
            async:false
        };

        $.ajax(options).success(function (data) {

            var sel = $(".select2[data-indice=" + $this.data('indice') + "]");
            sel.empty();
            var markup = '';
         
            for (var x = 0; x < data.length; x++) {
                if (!data[x].FIL.indexOf("OBSOLETO") != -1) {
                    markup += '<option value="' + data[x].OID + '" data-cod="' + data[x].FIL + '" data-text-value="' + data[x].DES_FIL + '">' + data[x].FIL + ' - ' + data[x].DES_FIL + '</option>';
                }
            }
            sel.html(markup).show();
            
        });
        
    });    
});



// evento click del SELECT DE TIPO DE EXPLORACION QUE CAMBIA EL PRECIO
$(document).on('change keyup', '.ddlAparatosMutua', function (e) {

    var indice= $(this).data('indice');
    var IOR_ENTIDADPAGADORA = $("#CID").val();
    var IOR_APARATO = $(this).val();
  
    
    var options = {
        url: "/Exploracion/GetTipoExploraciones",
        data: "IOR_APARATO=" + IOR_APARATO + "&" + "IOR_MUTUA=" + IOR_ENTIDADPAGADORA
    };

    $.ajax(options).success(function (data) {
     
        var sel = $(".ddlExploraciones" + indice);// $("#EXPLORACION[" + indice + "].IOR_TIPOEXPLORACION");
        sel.empty();
        var markup = '';
        for (var x = 0; x < data.length; x++) {           
            markup += '<option value="' + data[x].OID + '" data-cod="' + data[x].FIL + '" data-text-value="' + data[x].DES_FIL + '">' + data[x].FIL + ' - ' + data[x].DES_FIL + '</option>';
        }
       
        sel.html(markup).show();

        sel.attr('selected', 'selected');
    });
});





$(function () {
    if (filtrosBusqueda == undefined) {
        LoadFiltersPacientes();
    }
    $("#pacienteFilter").focus();
});
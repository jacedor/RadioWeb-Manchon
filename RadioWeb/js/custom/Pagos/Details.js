function LoadExploraciones(oidValue) {
    $.ajax({
        type: 'GET',
        url: '/Pagos/LoadExploraciones',
        data: { oid: oidValue },
        beforeSend: function () {
            $('.exploracionContainer').html("");
        },
        success: function (data) {
            $('.exploracionContainer').html(data);
            $("#cantidadPendienteTotal").html('<i class="fa fa-briefcase"></i>' + $("#ExploracionesList").data("totalpendiente") + $("#SIMBOLO").val().toString() );
            $("#cantidadPendienteExploracion").html($("#ExploracionesList").data("pendienteexplos") + $("#SIMBOLO").val().toString());

        }
    });
}

function LoadConsumibles(oidValue) {
    $.ajax({
        type: 'GET',
        url: '/Pagos/LoadConsumibles',
        data: { oid: oidValue },
        beforeSend: function () {
            $(".consumiblesEmpty").addClass("hide");
            $(".consumiblesContainer").removeClass("hide");
            $('.consumiblesContainer').html("")
        },
        success: function (data) {
            $('.consumiblesContainer').html(data);
            if ($(data).find("tbody").find("tr").length > 0) {
                LoadPagosConsumible($(".ConsumiblesList tbody tr:first").data("oid"));
                $("#cantidadPendienteTotal").html('<i class="fa fa-briefcase"></i>' + $(".ConsumiblesList").data("totalpendiente") + $("#SIMBOLO").val().toString());
                $("#cantidadPendienteConsumible").html($(".ConsumiblesList").data("pendienteexplos") + $("#SIMBOLO").val().toString());
            } else {
                $(".consumiblesEmpty").removeClass("hide");
                $(".consumiblesContainer").addClass("hide");
            }

        }
    });
}

function LoadPagosConsumible(oidValue) {
    $.ajax({
        type: 'GET',
        url: '/Pagos/LoadPagos',
        data: { oid: oidValue },
        beforeSend: function () {
            $('.pagosConsumiblesContainer').html("");
        },
        success: function (data) {
            $('.pagosConsumiblesContainer').html(data);
            $('.formapago').editable({
                source: [
                    { value: 'C', text: 'Contado' },
                    { value: 'V', text: 'Visa' },
                    { value: 'T', text: 'Talón' }
                ]
            });

            $('.cantidadPagoConsumible').editable({
                inputclass: 'importeAbonado',
                title: 'importe abonado',
                success: function (response, newValue) {
                    LoadConsumibles($("#OID").val());
                }

            }).on('shown', function () {
                $(".editable-buttons")
                    .remove("#PagarTodo")
                    .append('<button id="PagarTodoConsumible" type="button" class="btn btn-warning btn-sm editable-submit" title="Pagar Total"><i title="Pagar Todo Actual" class="glyphicon glyphicon-check"></i></button>');
            });
        }
    });
}

$(document).on("click", "#PagarTodo", function () {

    $(".importeAbonado").val($("#ExploracionesList* .ACTIVA").data("cantidad"));
});

$(document).on("click", "#PagarTodoConsumible", function () {

    $(".importeAbonado").val($(".ConsumiblesList* .ACTIVA").data("cantidad"));
});
function LoadPagos(oidValue) {
    $.ajax({
        type: 'GET',
        url: '/Pagos/LoadPagos',
        data: { oid: oidValue },
        beforeSend: function () {
            $('.pagosContainer').html("");
            $('.pagosConsumiblesContainer').html("");
        },
        success: function (data) {
            $('.pagosContainer').html(data);
            //campo para seleccionar el tipo de pago
            $('.formapago').editable({
                source: [
                    { value: 'C', text: 'Contado' },
                    { value: 'V', text: 'Visa' },
                    { value: 'T', text: 'Talón' }
                ]
            });
            $('.cantidadPago').editable({
                inputclass: 'importeAbonado',
                title: 'importe abonado',
                success: function (response, newValue) {
                    LoadExploraciones(oidValue);
                }
            }).on('shown', function () {
                $(".editable-buttons")
                    .remove("#PagarTodo")
                    .append('<button id="PagarTodo" type="button" class="btn btn-warning btn-sm editable-submit" title="Pagar Total"><i title="Pagar Todo Actual" class="glyphicon glyphicon-check"></i></button>');
            });
            LoadConsumibles($("#OID").val());
        }, error: function (data) {
            LoadConsumibles($("#OID").val());
        }
    });


}



$(document).on('click', '.pagoExploracion', function () {
    var oidPago = $(this).data('oidpago');
    var xeditablePago = $("#PAGO[data-oid=" + oidPago +"]");

    xeditablePago.editable('setValue', xeditablePago.data('cantidad'));
    xeditablePago.editable("submit"); 

});

$(document).on('click', ' #ExploracionesList tbody tr', function () {
    $(this).siblings().removeClass('ACTIVA');
    $(this).addClass('ACTIVA');
    $("#OID").val($(this).data("oid"));
    LoadPagos($(this).data("oid"));

});

$(document).on('click', ' .ConsumiblesList tbody tr', function () {
    $(this).siblings().removeClass('ACTIVA');
    $(this).addClass('ACTIVA');
    LoadPagosConsumible($(this).data("oid"));
});

$('#modal-form-Consumible').on('shown.bs.modal', function (e) {
    $("#OIDEXPLORACION").val($("#OID").val());
});


$(document).on('change', '#IOR_MUTUACONSUMIBLE', function () {

    var oidMutua = $(this).val();
    var oidExploracion = $("#ExploracionesList* .ACTIVA").data("oid");
    $.ajax({
        type: 'GET',
        url: '/Consumible/List?iorMutua=' + oidMutua + '&oidGrupo=' + $("#IOR_GAPARATO").val(),
        async: 'false',
        success: function (data) {

            var sel = $('#IOR_ADDCONSUMIBLE');
            sel.empty();
            var markup = '';
            for (var x = 0; x < data.length; x++) {
                markup += '<option value="' + data[x].IOR_CONSUM + '">' + data[x].CONSUMIBLE.COD_CONSUM + '</option>';
            }
            sel.html(markup).show();
        }
    });
});


//mapeamos el evento click de agregar consmible
$(document).on('click', '.marcarExploracion,.marcarConsumible', function () {
    var checkPagar = $(this);
    var oidLinea = checkPagar.data('oid');
    var urlServer = "/Exploracion/EditarCampo";
    if ($(this).hasClass('marcarConsumible')) {
        urlServer = "/Consumible/EditarCampo";
    }

    if ($(this).hasClass('fa-check')) {
        var option = {
            url: urlServer,
            data: "name=PAGAR&pk=" + oidLinea + "&value=F",
            type: "POST"
        };
        $.ajax(option).complete(function (data) {
            checkPagar.removeClass('fa-check')
                .removeClass('text-navy')
                .addClass('fa-times')
                .addClass('text-danger');

            $("tr[data-oid='" + oidLinea + "']").attr('data-pagar', 'F');
        });

    } else {
        var options = {
            url: urlServer,
            data: "name=PAGAR&pk=" + oidLinea + "&value=T",
            type: "POST"
        };
        $.ajax(options).complete(function (data) {
            checkPagar.removeClass('fa-times')
                .removeClass('text-danger')
                .addClass('text-navy')
                .addClass('fa-check');
            $("tr[data-oid='" + oidLinea + "']").attr('data-pagar', 'T');


        });

    }
    return false;
});

//mapeamos el evento click de agregar consmible
$(document).on('click', '.facturar', function () {

    var exploracionesAFacturar = $("#ExploracionesList tbody tr[data-pagar='T'][data-owner=1][data-facturado='F']").length;
    var consumiblesAFacturar = $(".ConsumiblesList tbody tr[data-pagar='T'][data-owner=1][data-facturado='F']").length;

    if (exploracionesAFacturar === 0 && consumiblesAFacturar === 0) {
        swal("No hay exploraciones ni consumibiles pendientes de Facturar", "Facturar", "error");
    }
    else {

        //var listaExploraciones = $("#ExploracionesList tbody tr[data-facturado='F']").find('.fa-check');
        swal({
            title: "Existen exploracion y/o consumibles marcados sin facturar. Confirme crear una factura nueva con la lineas indicadas",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Aceptar",
            cancelButtonText: "Cancelar",
            closeOnConfirm: true
        }, function (isConfirm) {
            if (isConfirm) {
                window.location = '/Facturas/CreateOrEdit?ior_factura=0&ior_exploracion=' + $("#OID").val();
            }
        });

    }
    //
    return false;
});

$(document).on('click', '.add-pago', function (e) {

    var url = $(this).attr('href');

    var oid = 0;

    if ($(this).hasClass('exploraciones')) {
        oid = $("#ExploracionesList* .ACTIVA").data("oid");
        url = url + '/' + oid + '?tipoPago=exploracion'
    } else {
        oid = $(".ConsumiblesList* .ACTIVA").data("oid");
        url = url + '/' + oid + '?tipoPago=consumible'
    }
    $.post(url, function () {
        LoadPagos($("#OID").val());
    });
    return false;
});



$(function () {

    LoadPagos($("#OID").val());
});
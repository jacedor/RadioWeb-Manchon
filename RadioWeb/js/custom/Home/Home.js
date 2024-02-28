
var scrollTablaExploraciones = 0;



function horarioSorter(a, b) {

    if (a > b) return 1;
    if (a < b) return -1;
    return 0;
}


function actualizaEspera() {

    if ($("#FILTROS_FECHA").val() === moment(new Date()).format('DD/MM/YYYY')) {

        $(".espera").each(function () {
            var horall = $(this).parent('tr').data('horall');

            //var now = moment().format('DD-MM-YYYY HH:mm');
            var now = moment().format('HH:mm');
            if (horall !== "") {

                var t1 = moment(horall.substr(0, 2) + ":" + horall.substr(3, 2), "HH:mm");
                var t2 = moment().format("HH:mm:ss");
                var espera = time_diff(t2, horall.substr(0, 2) + ":" + horall.substr(3, 2));


                $(this).children('.valor').html(espera);
                // outputs: "00:39:30"

            }
        });
    }

    // do some stuff...
    // no need to recall the function (it's an interval, it'll loop forever)
    // set interval
}

function filterRows(statusName) {
    $("#ExploracionesTable tbody tr").each(function () {
        if (!$(this).hasClass(statusName) && !$(this).hasClass('ACTIVA')) {
            $(this).hide();
        }
    });
}

function anularHoraLibre(accion) {

    $("#ExploracionesTable tbody tr.ACTIVA").each(function (i, row) {
        var comentario = "";
        if ($("#OtrosAnulacion").hasClass('hide')) {
            comentario = $("#MotivoAnulacion").val();
        } else {

            comentario = $("#OtrosAnulacion").val();
        }

        var filaActiva = $("#ExploracionesTable* .ACTIVA");

        hora = filaActiva.data('hhora');

        var request = $.ajax({
            url: "/Home/AnularHoraLibre",
            data: "fecha=" + $("#FILTROS_FECHA").val() + "&hhora=" + hora + "&aparato=" + filaActiva.data('aparato') + "&comentario=" + comentario,
            type: "GET"

        });
        request.done(function (data) {
            if (accion === "Desanular") {
                toastr.info(accion, 'Hora ' + accion, { timeOut: 5000 });
            } else {
                toastr.success(accion, 'Hora ' + accion, { timeOut: 5000 });
            }
            $("#EnviarFiltros").trigger("click");
        });
        request.fail(function (jqXHR, textStatus) {
            toastr.warning(accion, 'Error al anular Hora.', { timeOut: 5000 });
        });

    });

}

function trasladarExploracion(oid) {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    // var filtroActivos = getFiltrosBusqueda();
    if (filaActiva.length === 1 && filaActiva.data('anulada') !== 'True') {
        var hora = filaActiva.data('hhora');
        if (hora.length === 0) {
            hora = filaActiva.data('hora');
        }
        var request = $.ajax({
            url: "/Home/Trasladar",
            data: "oid=" + oid + "&fecha=" + $("#FILTROS_FECHA").val() + "&hhora=" + hora + "&aparato=" + filaActiva.data('aparato'),
            type: "GET"

        });
        request.done(function (data) {

            $("#EnviarFiltros").trigger("click");
            toastr.success('Exploración Trasladada', 'Trasladar', {
                timeOut: 3000,
                positionClass: 'toast-bottom-right'
            });
        });
        request.fail(function (jqXHR, textStatus) {
            toastr.error('Trasladar', 'Error al trasladar', {
                timeOut: 3000,
                positionClass: 'toast-bottom-right'
            });
        });
    } else {
        toastr.error('Debe seleccionar un hueco en la lista de exploracion', 'Trasladar',
            {
                timeOut: 3000,
                positionClass: 'toast-bottom-right'
            });
    }
}

function ImprimirListaDia() {
    var urlServer = '/Exploracion/ImprimirLista/';
    var newWin = null;
    newWin = window.open(urlServer, 'popup', 'width=900,height=500');
}

function cantidadFormat(value, row) {
    // 16777215 == ffffff in decimal
    var color = row.COLORESTADO;
    if (row.OID > 0 && row.PAGADO === "F" && row.ESTADO === "3") {
        color = "red";
    }
    var numb = value;
    if (numb) {
        numb = (numb * 1).toFixed(2);
        if (row.OID > 0)
            if (row.SIMBOLO === "€") {
                return '<span style=font-weight:bold;color:' + color + ';>' + (numb * 1).toFixed(2) + '<i class="fa fa-euro" ></i></span>';
            } else {
                return row.SIMBOLO + ' ' + (numb * 1).toFixed(2);
            }
    }
}



function rowAttr(row, index) {
    var HoraHorario = "";
    var claseActiva = "";
    var esHueco = "huecoOcupado";
    var estaPresente = "";
    row.ANULADA = false;
    if (row.PACIENTE && row.PACIENTE.substr(0, 1) === ">") {
        row.ANULADA = true;
        row.COLORESTADO = "red";
    }
    if (row.ESTADO == "2") {
        estaPresente = "espera";
    }

    if (row.OID < 0 && !row.ANULADA) {
        esHueco = "huecoLibre";
    }
    if (row.OID > 0 && row.OID === +$("#FILTROS_OIDACTIVA").val()) {
        claseActiva = "ACTIVA";
    }
    if (row.ANULACONSENTIMIENTO) {
        claseActiva = "ANULACONSENTIMIENTO";
    }
    if (row.HHORA != null) {
        HoraHorario = "background-color: rgba(147, 146, 146, 0.99)";
    }

    if (esHueco == "huecoLibre") {
        HoraHorario = "background-color:rgba(0, 255, 33, 0.86)";
    }

    return {
        "class": esHueco + ' ' + claseActiva,
        "style": "color:" + row.COLORESTADO + "!important",
        "data-oid": row.OID,
        "data-estado": row.ESTADO,
        "data-cid": row.CID,
        "data-ior_master": row.IOR_MASTER,
        "data-informada": row.INFORMADA,
        "data-firmada": row.FIRMA_CONSEN,
        "data-mutua": row.COD_MUT,
        "data-grupo": row.GRUPOAPA,
        "data-hora": row.HORA,
        "data-aparato": row.IOR_APARATO,// $("#FILTROS_IOR_APARATO").val(),
        "data-ior_paciente": row.IOR_PACIENTE,
        "data-horaex": row.HORA_EX,
        "data-anulada": row.ANULADA,
        "data-horall": row.HORA_LL,
        "data-espera": row.ESPERA,
        "data-hhora": row.HHORA,
        "data-facturada": row.FACTURADA,
        "data-pagado": row.PAGADO,
        "data-owner": row.CENTRO,
        "data-estado": row.ESTADO,
        "data-estadodesc": row.ESTADODESC,
        "data-anulaconsentimiento": row.ANULACONSENTIMIENTO
    };
}

function estadoFormat(value, row) {
    switch (row.ESTADO) {

        case "0":
            return '<span class="label badge-PENDIENTE  float-right">Pendiente</span>';
            break;
        case "1":
            return '<span class="label badge-borrado float-right">Borrada</span>';
            break;
        case "2":
            return '<span class="label badge-presencia float-right">Presencia</span>';
            break;
        case "3":
            return '<span class="label badge-confirmado float-right">Realizado</span>';
            break;
        case 4:
            return '<span class="label panel-NoPresentado float-right">No Presentado</span>';
            break;
        case 5:
            return '<span class="label panel-NoPresentado float-right">Llama Anulando</span>';
            break;
        default:
            if (row.ANULADA) {

                return '<span class="label badge-danger  float-right">Anulada</span>';
            }
            if (row.OID < 0) {
                return '<span class="label badge-info  float-right">Libre</span>';

            }

    }

}

function aplazadoFormat(value, row) {
    if (value === 'T') {
        return ' <i style="color: black;" title="Aplazado" class="fa fa-clock"></i>';
    }
}

function oidFormat(value, row) {
    if (value > 0) {
        return value;
    }

}

function facturadoFormat(value, row) {
    if (value === 'T') {
        return '<i style="color: black;" title="Facturada" class="fa fa-money"></i>';
    }

}

function colegiadoFormat(value, row) {
    if (value && value.length > 0) {
        if (value.length > 7) {
            return value.substr(0, 7);

        } else {
            return value;

        }
    }


}

function recordedFormat(value, row) {
    if (row.OID > 0) {
        if (row.RECORDED === 'T') {
            return '<i class="fa fa-volume-up" style="font-size: 13px; color: green;" title="Audio"></i>';
        }
        else {
            return '<i class="fa fa-volume-down" style="font-size: 13px; color: red;" title="Sin Audio"></i>';

        }
    }


}
function lopdFormat(value, row) {
    if (row.OID > 0) {
        if (row.LOPD === 'T') {
            return '<a target="_blank" href="/Exploracion/DownloadLopd?OID=' + row.IOR_PACIENTE + '"><i class="fa fa-smile-o" style="font-size: 11px; color: green;" title="LOPD FIRMADA"></i></a>';
        }
        else {
            return '<i class="fa fa-smile-o" style="font-size: 11px; color: red;" title="LOPD NO FIRMADA"></i>';

        }
    }


}

function firmaConsenFormat(value, row) {
    if (row.OID > 0) {
        if (row.FIRMA_CONSEN === 'T') {
            return '<a target="_blank" href="#" data-toggle="modal" data-target="#modal-form-Respuestas" class="iconVerRespuestas"><i class="fa fa-lightbulb-o" style= "font-size: 11px; color: green;" title= "Consentimiento Firmado" ></i></a>';
        }
        else {
            return '<i class="fa fa-lightbulb-o" style="font-size: 11px; color: red;" title="Consentimiento No Firmado"></i>';

        }
    }

}

function noFacturableFormat(value, row) {
    if (value === 'T') {
        return '<i style="color: black;" title="No Facturable" class="fa fa-thumbs-down"></i>';
    }

}
function informeFormat(value, row) {

    if (value === 'T') {
        if (row.IOR_MASTER > 0) {
            return "<a href=/Informe/Duplicar/" + row.IOR_MASTER + "?ReturnUrl=/Home/Index'>"
                + "<i class='fa fa-copy' style='font-size: 14px;color:green;'" +
                "title='Exploracion Relacionada INFORMADA'></i></a>";
        }
        else {
            return "<a href='/Informe/Duplicar/" + row.OID + "?ReturnUrl=/Home/Index'>"
                + "<i class='fa fa-clipboard' style='font-size: 14px;  color:green;'" +
                "title='INFORMADA'></i></a>";

        }
    }
    else {

        if (row.OID < 0) {
            if (row.ANULADA) {

                return '';
            }
            else {
                return "<i class='fa fa-plus agregarExploracion' " +
                    "data-href='/Exploracion/AddPaso1?FECHA=" + moment(row.FECHA).format('DD/MM/YYYY') + "&HORA=" + row.HHORA
                    + "&IOR_APARATO=" + row.IOR_APARATO +
                    "style='font-size: 11px; color: black;' title='Agregar Exploración'></i>";
            }

        }
        else {
            if (row.IOR_MASTER > 0) {
                return "<a href=/Informe/Duplicar/" + row.IOR_MASTER + "?ReturnUrl=/Home/Index'>"
                    + "<i class='fa fa-copy' style='font-size: 14px;color:red;'" +
                    "title='Exploracion Relacionada NO INFORMADA'></i></a>";
            }
            else {
                if (row.ESTADO == "2" || row.ESTADO == "3") {
                    return "<a href='/Informe/Duplicar/" + row.OID + "?ReturnUrl=/Home/Index'>"
                        + "<i class='fa fa-clipboard' style='font-size: 14px;  color:red;'" +
                        "title='NO INFORMADA'></i></a>";

                }
                else {
                    return "<i class='fa fa-clipboard' style='font-size: 12px;  color:red;'" +
                        "title='Exploracion NO INFORMADA'></i>";

                }
            }
        }

    }

}
function pacienteFormat(value, row) {

    if (row.OID < 0) {
        return '<span style="color:@item.COLORHORARIO;font-size:12px;">' + row.PACIENTE + '</span>';
    }
    else {
        return "<a class='linkirExploracion' href='/Exploracion/Details/" + row.OID + "?ReturnURL=/Home/Index'>"
            + "<span title='Paciente' style='color:" + row.COLORESTADO + "'><b>" + row.PACIENTE + "</b></a>";
    }
}

function consumibleFormat(value, row) {
    if (row.HAYCONSUMIBLE === 'T') {
        return '<a href="#" data-toggle="modal" data-target="#modal-form-consumibles">' +
            '<i class="fa fa-eyedropper" style="font-size: 11px; color: blue;" title="Consumible"></i>' +
            '</a>';

    }
}

function textoFormat(value, row) {

    if (row.OID > 0) {
        if (row.TEXTO != null && row.TEXTO.length > 0) {

            return '<span data-toggle="tooltip" data-placement="left" title="' + row.TEXTO + '" style="color:' + row.COLORESTADO + '">' +
                '<a href="#" data-type="textarea" style="color:' + row.COLORESTADO + '" class="textoXeditable" data-url="/Exploracion/EditarTexto" id="' + row.OID + '" data-pk=" ' + row.OID +
                '  " data-value="' + row.TEXTO + '" data-title="Editar texto">' + row.SUBTEXTO + '"</a></span>';

        }
        else {
            return '<span data-toggle="tooltip" data-placement="left" title="Agregar Texto" style="color:' + row.COLORESTADO + '">' +
                '<a href="#" data-type="textarea" style="color:' + row.COLORESTADO + '" class="textoXeditable" data-url="/Exploracion/EditarTexto" id="' + row.OID + '" data-pk=" ' + row.OID +
                '  " data-value="" data-title="Editar texto">... </a></span>';



        }

    }

}
function fechaMaxFormat(value, row) {
    if (row.FECHAMAXENTREGA !== null) {

        return '<span style="color:' + row.COLORESTADO + ';" class="valor">' + moment(value).format("DD/MM/YYYY") + '</span>';
    }

}


function des_filFormat(value, row) {
    //return moment(value).format("DD/MM/YYYY");
    if (row.OID > 0) {
        if (row.DES_FIL != null && row.DES_FIL.length > 20) {

            return "<span style='color:" + row.COLORESTADO + "'; title='" + row.DES_FIL + "'>" + row.DES_FIL.substr(0, 10) + "</span>";


        } else {
            return "<span style='color:" + row.COLORESTADO + "'; title='" + row.DES_FIL + "'>" + row.DES_FIL + "</span>";


        }
    }




}
function fechaStringFormat(value) {
    //return moment(value).format("DD/MM/YYYY");
    return value.substr(0, 10);


}
function incotableFormat(value, row) {
    if (row.INTOCABLE === true) {
        return '<input type="checkbox" class="INTOCABLE disabled" checked>';
    } else {
        return '<input type="checkbox" class="INTOCABLE disabled" >';
    }
}

function esperaFormat(value, row) {

    if (row.ESPERA !== null && row.ESPERA >= 0) {
        var hours = Math.floor(row.ESPERA / 60);
        var minutes = row.ESPERA % 60;
        hours = hours < 10 ? '0' + hours : hours;
        minutes = minutes < 10 ? '0' + minutes : minutes;
        return '<span style="color:' + row.COLORESTADO + ';font-weight:bold;" class="valor">' + hours + ':' + minutes + '</span>';

    }

}

function qReportFormat(value, row) {

    if (row.QRCOMPARTIRCASO === true) {
        return ' <i class="fa  fa-envelope" title="Qreport " style="font-size: 1.6em; color: blue"></i>';
    } else {
        return '';
    }
}

function fechaFormat(value, row) {

    return moment(value).format('DD/MM/YYYY');

}


function hhoraFormat(value, row) {

    return '<span style=color:' + row.COLORHORARIO + ' class="hhora"><b>' + value + '</b></span>';

}

function versFormat(value, row) {
    if (row.VIP === 'T') {
        return '<i class="fa fa-star" title="Paciente VIP" style="font-size: 2em; color: #FFE40B"></i>';
    }
    if (row.VERS === 1) {
        return '<i class="fa fa-child" title="Primera visita" style="font-size: 1.6em; color: green"></i>';
    }
}
function cidFormat(value, row) {

    if (row.OID < 0) {
        return '<a href="#modal-form-MotivoAnular" data-toggle="modal" class="dropdown-toggle">' +
            '<i class="fa fa-remove" style="color: red; font-size: 11px;" title="Anular Hora"></i>' +
            '</a>';
    }
    if (row.ANULADA) {
        return '<i class="fa fa-thumbs-up desanularHora" style="color: green; font-size: 11px;" title="Desanular Hora"></i>';
    }
    //return '';
    if (row.CID && row.CID != "#000000") {
        return '<div style="background-color:' + row.CID + ';height:100%;width:100%;">&nbsp;&nbsp;&nbsp;&nbsp;</div>';

    } else {
        return '';
    }

}


function cambiarEstadoExploracion(estadoActual, estadoNuevo, Oid, hhora, imprimirficha) {
    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");

    $.ajax({
        type: 'POST',
        url: "/Home/CambiarEstado",
        data: {
            estadoActual: estadoActual,
            estadoNuevo: estadoNuevo,
            oidExploracion: Oid,
            hhora: hhora
        },
        success: function (data) {
            if (imprimirficha) {
                imprimirExploracion(Oid);
            }

        }, complete: function () {

            var targetForm = $('form');
            var urlWithParams = targetForm.attr('action') + "?" + targetForm.serialize();
            scrollTablaExploraciones = $('.fixed-table-body').scrollTop();

            $('#ExploracionesTable').bootstrapTable('refresh', {
                url: urlWithParams,
                silent: true
            });

            toastr.success('Estado', 'Estado exploracion modificado', {
                timeOut: 3000,
                positionClass: 'toast-bottom-right'
            });

            if (+estadoNuevo === EstadosExploracion.Presencia) {
                toastr.info('Estado', 'Exploracion enviada a WorkList', {
                    timeOut: 3000,
                    positionClass: 'toast-bottom-right'
                });
            }
            // $("#EnviarFiltros").trigger("click");
        }
    });
}

function notificarKiosko(oid, action) {

    $.ajax({
        type: 'POST',
        url: "/Kiosko/Notificar",
        data: {
            oid: oid,
            action: action,
        },
        success: function (data) {

            if (data.success == "true") {
                toastr.success('Se ha notificado correctamente.', 'Kiosko', {
                    timeOut: 3000,
                    positionClass: 'toast-bottom-right'
                });
            }
            else {
                toastr.error(data.message, 'Kiosko', {
                    timeOut: 3000,
                    positionClass: 'toast-top-right'
                });
            }
            

        }, complete: function () {

        }
    });
}


function exploracionFormat(value, row) {
    if (row.OID > 0) {
        return '<span title=' + row.DES_FIL + '>' + row.FIL + '</span>';
    }
    //return '<div style="background-color:' + row.CID + ';height:100%;width:100%;">&nbsp;&nbsp;&nbsp;&nbsp;</div>';
}

$(document).on('load-success.bs.table', '#ExploracionesTable', function () {

    accionesTrasCargarTable(scrollTablaExploraciones);
});

$(document).on('changeDate', '#FILTROS_FECHA', function (ev) {

    //BeginFiltros();
    $('#btnHoy').addClass('disabled').prop('disabled', true);
    $('#btnDiaSiguiente').addClass('disabled').prop('disabled', true);
    $('#btnDiaAnterior').addClass('disabled').prop('disabled', true);
    $('.bootstrap-table').addClass('hide');
    $('#cuentaExploraciones').addClass('hide');
    var targetForm = $('form');

    //$("#EnviarFiltros").trigger("click");
    var urlWithParams = targetForm.attr('action') + "?" + targetForm.serialize();
    $('#ExploracionesTable').bootstrapTable('refresh', {
        url: urlWithParams,
        silent: true
    });



});

$(document).on("click", "#btnArqueoDiario", function myfunction() {

    window.location = "/Exploracion/ArqueoPrint?fechaArqueo=" + $("#FILTROS_FECHA").val();
    return false;

});

$(document).on("click", "#btnImprimirRecordatorio", function myfunction() {

    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
    var oidExploracion = filaActiva.data('oid');
    window.open("/Exploracion/ImprimirRecordatorioCita/" + oidExploracion, "Recordatorio Cita", "menubar=1,resizable=1,width=800,height=600");


    return false;

});
//$(document).on("click", "#btnArqueoDiario", function myfunction() {



//    $.ajax({
//        type: 'GET',
//        url: '/Facturas/Arqueo?fecha=' + $("#FILTROS_FECHA").val(),
//        beforeSend: function () {
//            $(".spiner-cargando-arqueoDiario").removeClass('hide');           
//            $('#modal-form-ArqueoDiario').modal('show');
//        },
//        success: function (data) {
//            $(".spiner-cargando-arqueoDiario").addClass('hide');
//            $('#tblArqueDiarioHome').bootstrapTable({
//                data: data
//            });
//            $('#tblArqueDiarioHome').bootstrapTable('load', data);
//         //   $("#NombrePacienteEntrada").html($("#PACIENTE").val());
//        },
//        complete: function () {



//        }
//    });




//    return false;

//});
function BeginFiltros() {

    sessionStorage.vistaActual = "ViewListaDia";
    ajustaEstadoMenuSuperior(null);
    scrollTablaExploraciones = $('.fixed-table-body').scrollTop();
    $('#btnHoy').addClass('disabled').prop('disabled', true);
    $('#btnDiaSiguiente').addClass('disabled').prop('disabled', true);
    $('#btnDiaAnterior').addClass('disabled').prop('disabled', true);
    $("#ViewCalendario").addClass('hide');
    $("#ViewListaDia").removeClass('hide');
    $("li[data-view]").removeClass('active');
    $("[data-view=ViewListaDia]").addClass("active");
    //$('.bootstrap-table').addClass('hide');
    //$('#cuentaExploraciones').addClass('hide');
    // $("#spiner-cargando-listadia").removeClass('hide');
    var l = $('#EnviarFiltros').addClass('btn-info').ladda();
    l.ladda('start');
    if ($("div[data-fecha='" + $("#FILTROS_FECHA").val() + "']").hasClass('festivo')) {
        toastr.warning('Festivo', 'Día Festivo', { timeOut: 5000 });
        $('#addExploracion').addClass('disabled');
        //$("#spiner-cargando-listadia").addClass('hide');
        return false;
    }

}


function SuccessFiltros(data, status, xhr) {

    var datos = data;
    $('.bootstrap-table').removeClass('hide');
    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
    var oidExploracion = filaActiva.data('oid'); 
    var url = "";
    $('#ExploracionesTable').bootstrapTable({
        //Assigning data to table
        undefinedText: '',
        data: data,
        contextMenu: '#context-menu',
        onContextMenuItem: function (row, $el) {

            if ($el.data("item") === "vincularaHL7") {
                var idExploracion = $("#ExploracionesTable tbody tr.ACTIVA").data("oid");

            
                url = "/Exploracion/ListaBadalonaPendienteCitar/" + idExploracion; //The Url to the Action  Method of the Controller

                $.ajax({
                    type: 'GET',
                    url: url,
                    dataType: "html",
                    success: function (evt) {
                        $('#modal-form-vincular').remove();
                        $("body").append(evt);
                        $('#modal-form-vincular').modal('show');
                    },
                    complete: function () {
                        $("#ExploracionesVinculablesHL7").bootstrapTable();
                    }
                });
            }
            if ($el.data("item") === "desvincularMaster") {

          
                 url = "/Exploracion/DesvincularHija/" + oidExploracion; //The Url to the Action  Method of the Controller

                $.ajax({
                    type: 'GET',
                    url: url,
                    dataType: "html",
                    success: function (evt) {
                        $('#modal-form-desvincular').remove();
                        $("body").append(evt);
                        $('#modal-form-desvincular').modal('show');

                    },
                });

            }

            if ($el.data("item") === "pagarExploracion") {
                var filaActiva = $("#ExploracionesTable* .ACTIVA");
             
                var fecha = $("#FILTROS_FECHA").val();
                window.location = "/Pagos2/Index?fecha=" + fecha + '&ior_paciente=' + filaActiva.data('ior_paciente');
            }
            if ($el.data("item") === "entradaCarrito") {
                $("#btnEntrada").trigger("click");
            }
            //Agregar Exploracion al carrito                    
            if ($el.data("item") === "carrito") {
                var idExploracion = $("#ExploracionesTable tbody tr.ACTIVA").data("oid");
                if ($("#ExploracionesMiniTable tbody tr[data-oid=" + idExploracion + "]").length !== 0) {
                    toastr.warning('Carrito', 'La exploracion ya está en el carrito', { timeOut: 5000 });
                } else {
                    $.ajax({
                        type: 'POST',
                        url: '/Settings/SaveExploracion',
                        data: { oid: idExploracion },
                        success: function (data) {
                            loadExploracionesPersonales();
                        }
                    });
                }
            }
            //Agregar todas las exploraciones Exploracion al carrito
            if ($el.data("item") === "carritoTodas") {
                //Añadimos el recuperar la ID, ya que n lo esta hacientdo.
                var idExploracion = $("#ExploracionesTable tbody tr.ACTIVA").data("oid");
                if ($("#ExploracionesMiniTable tbody tr[data-oid=" + idExploracion + "]").length !== 0) {
                    toastr.warning('Carrito', 'La exploracion ya está en el carrito', { timeOut: 5000 });
                } else {
                    $.ajax({
                        type: 'POST',
                        url: '/Settings/SaveExploracion',
                        data: { oid: oidExploracion, todasDelDia: 'true' },
                        success: function (data) {
                            loadExploracionesPersonales();
                        },
                        complete: function () {


                        }
                    });

                }

            }
            if ($el.data("item") === "enviarQreport") {
                var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
                var oidExploracion = $("#ExploracionesTable tbody tr.ACTIVA").data("oid");

                if (filaActiva.data('informada') === "F") {
                    toastr.error('Imposible enviar a QReport. Exploración no informada.', 'QReport', { timeOut: 5000 });
                } else {
                    var url = "/Informe/Qreport?oid=" + oidExploracion + "&esOidExploracion=true"; //T
                    $.ajax({
                        url: url,
                        type: 'GET',
                        success: function (data) {

                            if(data.success) {
                                toastr.success('Enviado a QReport ', 'QReport', { timeOut: 5000 });
                            }
                            else {
                                if (data.message != "") {
                                    toastr.error(data.message, 'QReport', { timeOut: 5000 });
                                }
                                else {
                                    toastr.error('Imposible enviar a QReport ', 'QReport', { timeOut: 5000 });
                                }                                
                            }                     
                        },
                        error: function (x, y, z) {
                            toastr.error('Imposible enviar a QReport ', 'QReport', { timeOut: 5000 });
                        }
                    });

                }
            }
            if ($el.data("item") === "enviarSMS") {
                var idExploracion = $("#ExploracionesTable tbody tr.ACTIVA").data('oid');
                var idPaciente = $("#ExploracionesTable tbody tr.ACTIVA").data('ior_paciente');
                $.ajax({
                    type: 'POST',
                    url: '/Paciente/getLOPDsettingsById',
                    data: {
                        idPaciente: idPaciente
                    },
                    beforeSend: function () {
                        $("#displayPermisoLOPDsms").empty();
                        $("#EnviarSMS").prop("disabled", true);
                    },
                    success: function (data) {
                        $("#displayPermisoLOPDsms").html(data);
                        $("#EnviarSMS").prop("disabled", false);
                        if ($("#ENVIO_SMS").val() === "T") {
                            toastr.warning('El paciente no consiente el envío de SMS en su declaración de la LOPD', 'El SMS no se enviará', {
                                timeOut: 5000
                            });
                        }
                    }
                });
                
                $.ajax({
                    type: 'GET',
                    url: '/Telefono/GetMovilPacienteFromExploracion?oidExploracion=' + idExploracion,
                    success: function (data) {
                        $("#movilEnvioSMS").val(data);
                    }
                });
            }

            if ($el.data("item") === "tiempoEspera") {

                var rowActiva = $("#ExploracionesTable tbody tr[data-oid=" + row.OID + "]");
                $("#HoraProgramada").html(rowActiva.data('hora'));
                $("#HoraLLegada").html(rowActiva.data('horall'));
                $("#tiempoEspera").html(rowActiva.data('espera'));

                $("#HoraRealizada").html(rowActiva.data('horaex'));
            }
            //al hacer boton derecho sobre una fila la activamos desactivando previamente el resto
            if ($el.data("item") === "imprimirLista") {

                ImprimirListaDia();
            }

            if ($el.data("item") === "visoresQreport") {
                var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
                var oidExploracion = $("#ExploracionesTable tbody tr.ACTIVA").data("oid");

                if (filaActiva.data('informada') === "F") {
                    toastr.error('Imposible mostrar los visores QReport. Exploración no informada.', 'QReport', { timeOut: 5000 });
                } else {
                    var url = "/Informe/comprobarEstadoInformeQReport?oid=" + oidExploracion;
                    $.ajax({
                        url: url,
                        type: 'GET',
                        success: function (data) {
                            if (data.success) {
                                $('#modalVisoresQreport').modal('show');
                            }
                            else {
                                toastr.error(data.message, 'Visor QReport', { timeOut: 5000 });
                            }
                        },
                        error: function (x, y, z) {
                            toastr.error('Imposible abrir el visor de QReport', 'Visor QReport', { timeOut: 5000 });
                        }
                    });

                }
            }

            return false;
        },
        onClickRow: function (row, $el) {
            //Ponemos la fila actual con clase activa
            if (!ctrlPressed) {
                $('#ExploracionesTable').find('.ACTIVA').removeClass('ACTIVA');
                $el.addClass('ACTIVA');
                ajustaEstadoMenuSuperior($("#ExploracionesTable").find('.ACTIVA'));
            }
            var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
            var oid = filaActiva.data('oid');
            var oidMaster = filaActiva.data('ior_master');
            var oidMutua = filaActiva.data('mutua');
            var pagado = filaActiva.data('pagado');
            var estado = filaActiva.data('estado');

            if (estado === 3) {
                $("li[data-item='pagarExploracion']").removeClass('hide');
            } else {
                $("li[data-item='pagarExploracion']").addClass('hide');
            }
            var informada = (filaActiva.data('informada') === "T" || filaActiva.data('estado') === 0);
            if (!informada) {
                $("li[data-item='informar']").removeClass('hide');
                var linkInfomar = $("li[data-item='informar']").find("a");
                linkInfomar.attr("href", "/Informe/Duplicar/" + oid + "?ReturnUrl=/Home/Index")
            } else {
                $("li[data-item='informar']").addClass('hide');
            }
            if (oidMaster > 0) {
                $("li[data-item='desvincularMaster']").removeClass('hide');
            } else {
                $("li[data-item='desvincularMaster']").addClass('hide');
            }
            //alert(oidMutua);
            if ((oidMutua === 11042 || oidMutua == "MEDIFI") && (oidMaster < 0 || oidMaster == null)) {
                $("li[data-item='vincularaHL7']").removeClass('hide');
            } else {
                $("li[data-item='vincularaHL7']").addClass('hide');
            }
            if (oid <= 0) {
                $("#context-menu").addClass('hide');

            } else {
                $("#context-menu").removeClass('hide');
            }

        }
    });



    $('#ExploracionesTable').bootstrapTable('load', data);


}

function CompleteFiltros() {

    var l = $('#EnviarFiltros').addClass('btn-info').ladda();
    l.ladda('stop');
    var exploracionescuenta = $('#ExploracionesTable tbody tr[data-oid!=0]').length + " exploraciones";
    var huecosCuenta = $('#ExploracionesTable tbody tr[data-oid=-1]').length;
    $("#ExploracioneTable").removeClass("hide");
    var aparato = $("#FILTROS_IOR_APARATO option:selected").text();
    var oidAparato = $("#FILTROS_IOR_APARATO option:selected").val();
    $('#cuentaExploraciones').removeClass('hide');
    moment.locale('es');
    if (oidAparato > 0) {
        $('#cuentaExploraciones').html(moment($("#FILTROS_FECHA").val(), "DD/MM/YYYY").format('dddd') + ", " + $("#FILTROS_FECHA").val() + "|" + exploracionescuenta + " - " + huecosCuenta + " huecos (" + aparato + ")");

    } else {
        $('#cuentaExploraciones').html(moment($("#FILTROS_FECHA").val(), "DD/MM/YYYY").format('dddd') + ", " + $("#FILTROS_FECHA").val() + "|" + exploracionescuenta + " - " + huecosCuenta + " huecos ");

    }
    sessionStorage.vistaActual = "ViewListaDia";
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    ajustaEstadoMenuSuperior(filaActiva);
    actualizaEspera();


    var w = $(window);
    var row = $("#ExploracionesTable").find('tr.ACTIVA');
    $('.bootstrap-table').removeClass('hide');
    $('#btnHoy').removeClass('disabled').removeAttr('disabled');
    $('#btnDiaSiguiente').removeClass('disabled').removeAttr('disabled');
    $('#btnDiaAnterior').removeClass('disabled').removeAttr('disabled');
    var el = document.querySelector('.bootstrap-table');


    if (row.length && el.scrollTop === 0) {
        $('.fixed-table-body').animate({ scrollTop: row.offset().top - (w.height() / 2) - 100 }, 1);
    }
    //if (!(row.length) && moment(new Date()).format('DD/MM/YYYY') === $("#FILTROS_FECHA").val()) {
    //    var horaActual = moment(new Date()).format('HH:00');
    //    $("div[data-fecha='" + $("#FILTROS_FECHA").val() + "']")
    //    var rowHora = $("#ExploracionesTable").find("tr[data-hora='" + horaActual + "']");
    //    if (rowHora.length) {

    //        $('.fixed-table-body').animate({ scrollTop: rowHora.offset().top - (w.height() / 2) }, 1);
    //    }

    //}

    loadExploracionesPersonales();

}


function onTestFailure(data) {
    if (data.status === 401)
        window.location.href = "/Users/Index";
}


//Evento del botón que anula una un hueco libre
$(document).on('click', '#anularHuecoLibre,.desanularHora', function () {

    var accion = "Anular";
    if ($(this).hasClass("desanularHora")) {
        var trDesanular = $(this).closest('tr');
        trDesanular.siblings().removeClass('ACTIVA');
        trDesanular.addClass('ACTIVA');
        accion = "Desanular";
    }
    anularHoraLibre(accion);
});

$(document).on('change', '#MotivoAnulacion', function () {

    if ($("#MotivoAnulacion").val() === "Otros") {

        $("#OtrosAnulacion").removeClass('hide');
    } else {
        $("#OtrosAnulacion").addClass('hide');
    }
});

$(document).on("click", "#EnviarSMS", function () {
    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
    var oidExploracion = filaActiva.data('oid');

    var permissionSMS = $("#ENVIO_SMS").val();
    var acceptSend = (permissionSMS === "T" ? false : true);

    if (acceptSend) {
        var options = {
            url: "/SMS/Enviar",
            data: "phone=" + $("#movilEnvioSMS").val() + "&texto=" + $("#textoSMS").val() + "&idMensaje=" + oidExploracion,
            type: "GET"
        };

        $.ajax(options).complete(function (data) {
            toastr.success('Enviado correctamente', 'SMS', {
                timeOut: 3000,
                positionClass: 'toast-bottom-right'
            });
        });
    } else {
        toastr.error('Segun la LOPD, este paciente no consiente el envio de SMS', 'SMS No enviado', {
            timeOut: 3000,
            positionClass: 'toast-bottom-right'
        });
    }
    
});

$(document).on('click', '#ExploracionesTable tbody tr', function () {
    ajustaEstadoMenuSuperior($(this));
    if (ctrlPressed) {
        // do something
        if ($(this).hasClass('ACTIVA')) {
            $(this).removeClass('ACTIVA');
        } else {
            $(this).addClass('ACTIVA');
            if ($(this).hasClass('huecoOcupado')) {
                $("#FILTROS_OIDACTIVA").val($(this).data('oid'));
            }
        }
    } else {
        $(this).siblings().removeClass('ACTIVA');
        $(this).addClass('ACTIVA');
        if ($(this).hasClass('huecoOcupado')) {
            $("#FILTROS_OIDACTIVA").val($(this).data('oid'));
        }
    }

});

$(document).on("click", "#btnBorrar,#btnNoPresentado,#btnLlamaAnulando", function myfunction() {

    var trigger = $(this).attr("id");
    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
    //hay que enseñar el pop up si es igual a 0 solamente
    var estadoExploracion = filaActiva.data('estado');
    var estadoNuevo = null;

    if (estadoExploracion === EstadosExploracion.Borrado || estadoExploracion === EstadosExploracion.NoPresentado || estadoExploracion === EstadosExploracion.LlamaAnulando) {
        estadoNuevo = EstadosExploracion.Pendiente;
    }

    if (estadoExploracion === EstadosExploracion.Pendiente) {
        switch (trigger) {
            case "btnBorrar":
                estadoNuevo = EstadosExploracion.Borrado;
                break;
            case "btnNoPresentado":
                estadoNuevo = EstadosExploracion.NoPresentado;
                break;
            case "btnLlamaAnulando":
                estadoNuevo = EstadosExploracion.LlamaAnulando;
                break;
            default:
                break;
        }

    }

    var hhora = filaActiva.data('hhora');

    var oidExploracionSeleccionada = filaActiva.data('oid');

    cambiarEstadoExploracion(estadoExploracion, estadoNuevo, oidExploracionSeleccionada, hhora);
    //LoadListaDia(false);
    return false;
});




//mapeamos el evento click de cada una de las filas del carrito sobre el aparato que lanza el loadListaDia
$(document).on('click', '.filtroAparatoCarrito', function () {

    $('#FILTROS_IOR_APARATO option:contains("' + $(this).data('aparato') + '")').prop('selected', true);
    $("#FILTROS_IOR_APARATO").trigger("change");
    $('.popover').hide();
});

//mapeamos el evento click de cada una de las filas de la tabla de exploraciones de dentro del carrito que permite trasladar exploraciones
$(document).on('click', '.trasladarExploracion', function () {
    trasladarExploracion($(this).data('oid'));
});

//mapeamos el evento click de cada una de las filas de la tabla de exploraciones de dentro del carrito que permite trasladar exploraciones
$(document).on('click', '.vincularHL7', function () {
    var url = $(this).attr("href");

    $.ajax({
        type: 'GET',
        url: url,
        success: function (data, textStatus, xhr) {
            if (xhr.statusText === "VINCULADO") {
                toastr.info('Exploracion vinculada correctamente. ', '', { timeOut: 3000 });
                $('#modal-form-vincular').modal('hide');
                $("#EnviarFiltros").trigger("click");
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            switch (xhr.status) {
                case 404:
                    toastr.error(xhr.statusText, '', { timeOut: 3000 });
            }
        }

    });
    return false;
});


//mapeamos el evento click de cada una de las filas de la tabla de exploraciones de dentro del carrito que permite trasladar exploraciones
$(document).on('click', '.duplicarExploracion,.exploracionRelacionada', function (e) {
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    if (filaActiva.length === 1 && filaActiva.data('anulada') !== 'True') {
        var oidRelacionada = -1;

        var fechaExplo = "01-01-1990";
        var hora = filaActiva.data('hhora');

        if ($(this).hasClass('exploracionRelacionada')) {
            //Si queremos duplicar relacionando con otra exploración 
            //tenemos que obtener la fecha en la que estamos en el lista dia
            oidRelacionada = $(this).data('oid');
        }
        if (hora.length === 0) {
            hora = filaActiva.data('hora');
        }
        window.location = "/Exploracion/Duplicar/" + $(this).data('oid')
            + "?hora=" + hora + '&ioraparato=' + filaActiva.data('aparato') + '&relacionada=' + oidRelacionada + '&fecha=' + $("#FILTROS_FECHA").val();
    } else {
        toastr.error('Debe seleccionar un hueco en la lista de exploracion', 'Duplicar', { timeOut: 5000 });
    }
    return false;
});

//mapeamos el evento click de cada una de las filas de la tabla de exploraciones de dentro del carrito que permite trasladar exploraciones
$(document).on('click', '.eliminarDelCarrito', function () {
    eliminarDelCarrito($(this).data('oid'));
});

//mapeamos el evento click de cada una de las filas de la tabla de exploraciones de dentro del carrito que permite trasladar exploraciones
$(document).on('click', '.eliminarDePeticiones', function () {
    eliminarDePeticiones($(this).data('oid'));
    loadPeticiones();
    // eliminarDelCarrito($(this).data('oid'));
});

//los evento change y mover son up y down de todos los filtros del calendario
$(document).on('change', '#ddlEmail', function () {
    var idPlantillaMail = $(this).data('oid');
    var texto = $("#ddlEmail option[value=" + $("#ddlEmail").val() + "]").data('contenido');
    $("#textoSMS").val(texto);
});


$(document).on('click', '#btnDiaAnterior', function () {
    var fecha = moment($("#FILTROS_FECHA").val(), "DD/MM/YYYY").add('days', -1).format('DD/MM/YYYY');
    $("#FILTROS_FECHA").datepicker("setDate", fecha);
    //$("#EnviarFiltros").trigger("click");

    return false;
});

$(document).on('click', '#btnHoy', function () {
    //setCurrentDayActions(moment(new Date()).format('DD-MM-YYYY'));
    var fecha = moment(new Date()).format('DD/MM/YYYY');
    $("#FILTROS_FECHA").datepicker("setDate", fecha);
    // $("#EnviarFiltros").trigger("click");

});


$(document).on('click', '#btnDiaSiguiente', function () {
    //setCurrentDayActions(addDays($("#fechaSelect").val(), 1));
    var fecha = moment($("#FILTROS_FECHA").val(), "DD/MM/YYYY").add('days', 1).format('DD/MM/YYYY');
    $("#FILTROS_FECHA").datepicker("setDate", fecha);
    // $("#EnviarFiltros").trigger("click");

});


$(document).on("click", "#btnConfirmar", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");


    //var estadoNuevo = EstadosExploracion.Confirmado;
    var estadoNuevo = filaActiva.data('estado') === EstadosExploracion.Confirmado
        ? EstadosExploracion.Presencia
        : EstadosExploracion.Confirmado;

    var hhora = filaActiva.data('hhora');

    var oidExploracionSeleccionada = filaActiva.data('oid');

    cambiarEstadoExploracion(filaActiva.data('estado'), estadoNuevo, oidExploracionSeleccionada, hhora);
    return false;
});

$(document).on("click", "#btnActualizarPresencia,#btnActualizarPresenciaEntrada", function myfunction() {
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var estadoNuevo = filaActiva.data('estado') === 0
        ? '2'
        : '0';

    var hhora = filaActiva.data('hhora');
    var oidExploracionSeleccionada = filaActiva.data('oid');

    cambiarEstadoExploracion(filaActiva.data('estado'), estadoNuevo, oidExploracionSeleccionada, hhora);
    $("#btnActualizarPresencia").prop('disabled', false);
    $("#btnActualizarPresenciaEntrada").prop('disabled', false);

    return false;

});

$(document).on("click", "#btnPagoRapido", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidpaciente = filaActiva.data('ior_paciente');
    var fecha = $("#FILTROS_FECHA").val();
    window.location = "/Pagos2/Index?fecha=" + fecha + '&ior_paciente=' + oidpaciente;
    return false;

});

$(document).on("click", "#btnCapturarDesdeTablet", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActiva.data('oid');

    window.location = "/Imagenes/Create/" + oidExploracion;
    return false;

});




$(document).on("click", "#btnPresenciaImprimir,#btnPresenciaImprimirEntrada", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var estadoNuevo = filaActiva.data('estado') === 0
        ? '2'
        : '0';

    var hhora = filaActiva.data('hhora');
    var oidExploracionSeleccionada = filaActiva.data('oid');
    cambiarEstadoExploracion(filaActiva.data('estado'), estadoNuevo, oidExploracionSeleccionada, hhora, true);
    return false;

});

$('#modalImprimirJustificante').on('show.bs.modal', function (e) {
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    $("#HoraLLegadaJustificante").val(filaActiva.data('horall'));
    // $("#HoraRealizadaJustificante").val(filaActiva.data('horaex'));
    $("#HoraRealizadaJustificante").val(moment().format("HH:mm"));

});

$(document).on("click", "#btnImprimir", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActiva.data('oid');
    imprimirExploracion(oidExploracion);
    return false;

});

$(document).on("click", "#btnImprimirJustificante", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActiva.data('oid');
    imprimirJustificante(oidExploracion,
        $("#HoraLLegadaJustificante").val(),
        $("#HoraRealizadaJustificante").val(),
        $("#textoLibreJustificante").val());

    return false;

});



$(document).on("click", "#btnFichaPaciente", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidPaciente = filaActiva.data('ior_paciente');
    window.location = "/Paciente/Details?ior_paciente=" + oidPaciente + "&TraerInformesYExplos=true&ReturnUrl=/Home/Index";
    return false;

});


$(document).on("click", "#btnKiosko, #btnKioskoOpcionesAction", function myfunction() {


    var action = "cancel";
    if ($(this).attr('id') === "btnKiosko") {
        action = "call";
    }
    else if ($(this).attr('id') === "btnKioskoOpcionesAction") {
        action = "cancel";
    }
    else
    {
        toastr.error('Error inesperado', 'Error',{
            timeOut: 3000,
            positionClass: 'toast-top-right'
        });
        return true;
    }


    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var estado = filaActiva.data('estado');
    var oidExploracionSeleccionada = filaActiva.data('oid');

    if (action === "call" && estado != EstadosExploracion.Presencia)
    {
        toastr.error('El estado de la exploración debe ser Presencia.', 'Error', {
            timeOut: 3000,
            positionClass: 'toast-top-right'
        });
        return true;
    }

    notificarKiosko(oidExploracionSeleccionada, action);
    return true;
});

//Al escribir sobre la caja de texto del modal popup de pacintes
$(".linkirExploracion").keyup($.debounce(100, function () {

    window.location = $(this).href;
    return false;
}));

//Al escribir sobre la caja de texto del modal popup de pacintes
$("#FILTROS_PACIENTE").keyup($.debounce(500, function () {
    if ($('#FILTROS_BUSQUEDATOTAL').is(":checked")) {
        return true;
    }
    if (($("#FILTROS_PACIENTE").val().length === 0) || ($("#FILTROS_PACIENTE").val().length > 2 && $("#FILTROS_PACIENTE").val() !== "")) {

        $("#EnviarFiltros").trigger("click");

    }

}));

function activarTypeAhead() {
    $(".typeahead").typeahead({
        source: function (query, process) {
            var pacientes = [];
            map = {};

            // This is going to make an HTTP post request to the controller
            return $.post('/Paciente/Buscar', { query: query }, function (data) {

                // Loop through and push to the array
                $.each(data, function (i, paciente) {
                    map[paciente.PACIENTE1] = paciente;
                    pacientes.push(paciente.PACIENTE1);
                });

                // Process the details
                process(pacientes);
            });
        },
        updater: function (item) {

            $("#FILTROS_PACIENTE").text(map[item].PACIENTE1);
            $("#FILTROS_IOR_PACIENTE").val(map[item].OID);
            $("#EnviarFiltros").trigger("click");
            return item;
        }
    });
}

$(document).on('change', '#FILTROS_BUSQUEDATOTAL', function () {
    if (this.checked) {
        activarTypeAhead();
    } else {
        $('.typeahead').typeahead('destroy');
    }
});




$(document).on("click", "#btnAgregarExploracion ,.agregarExploracion,.agendarPeticion", function myfunction() {

    var vieneBolsaPeticion = false;
    var oidPeticion = -1;
    if ($(this).hasClass("agregarExploracion")) {
        var trAlta = $(this).closest('tr');
        trAlta.siblings().removeClass('ACTIVA');
        trAlta.addClass('ACTIVA');
        //window.location = $(this).data('href');
    }
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    if ($(this).hasClass("agendarPeticion")) {
        oidPeticion = $(this).data('iorbolsa');
        var iorGaparato = $(this).data('iorgrupoaparato');
        vieneBolsaPeticion = true;
        if (filaActiva.data('grupo') !== iorGaparato) {
            toastr.error('Debe seleccionar un hueco compatible con el tipo de exploración solicitada', 'Agendar',
                {
                    timeOut: 3000,
                    positionClass: 'toast-bottom-right'
                });
            return false;
        }
    }

    hora = filaActiva.data('hora');
    aparato = filaActiva.data('aparato');

    // now you can search the returned html data using .find().
    var form = document.createElement("form");
    form.setAttribute("action", "/Exploracion/AddPaso1")
    filaActiva.each(function (i, row) {
        var $rowActual = $(row);
        var hora = $rowActual.data('hhora');
        if (hora.length === 0) {
            hora = $rowActual.data('hora');
        }
        var input = document.createElement("input");
        input.setAttribute("type", "hidden");
        input.setAttribute("name", "HUECOS[" + i + "].HORA");
        input.setAttribute("value", hora);
        form.appendChild(input);
        var input = document.createElement("input");
        input.setAttribute("type", "hidden");
        input.setAttribute("name", "HUECOS[" + i + "].FECHA");
        input.setAttribute("value", $("#FILTROS_FECHA").val());
        form.appendChild(input);
        var input = document.createElement("input");
        input.setAttribute("type", "hidden");
        input.setAttribute("name", "HUECOS[" + i + "].IOR_APARATO");
        input.setAttribute("value", $rowActual.data('aparato'));
        form.appendChild(input);
        if (vieneBolsaPeticion) {
            var input = document.createElement("input");
            input.setAttribute("type", "hidden");
            input.setAttribute("name", "HUECOS[" + i + "].IOR_BOLSA");
            input.setAttribute("value", oidPeticion);
            form.appendChild(input);
        }
    });
    document.body.appendChild(form);
    form.submit();



});



$(document).on("sort.bs.table", "#ExploracionesTable", function (name, order) {
    var direccionOrden = name.handleObj.handler.arguments[2];
    var campoOrden = order;
    $("#orderDirection").val(direccionOrden);
    $("#orderField").val(campoOrden);
});



$("#BuscarConsumible").keyup($.debounce(250, function () {
    var data = $(this).val();
    var url = "/Consumible/List/"; //The Url to the Action  Method of the Controller
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var mutua = filaActiva.data('mutua');
    var grupoOid = filaActiva.data('grupo');
    var grupoDesc = $("#ddlGrupo option[value=" + grupoOid + "]").text();
    var Consumbile = {}; //The Object to Send Data Back to the Controller
    Consumbile.USERNAME = $("#BuscarConsumible").val();
    Consumbile.IOR_ENTIDADPAGADORA = mutua;
    Consumbile.OWNER = grupoOid;
    // Check whether the TextBox Contains text
    // if it does then make ajax call

    if ($("#BuscarConsumible").val().length > 3 && $("#BuscarConsumible").val() !== "") {
        $.ajax({
            type: 'POST',
            url: url,
            data: Consumbile,
            dataType: "html",
            success: function (data) {
                $('#Consumbile > tbody').html(data);
            },
        });
    }

}));

// store the currently selected tab in the hash value
$(document).on("click", ".iconVerRespuestas", function myfunction() {
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActiva.data('oid');
    //var ContenedorModalImagenes = $('#contenedorModalImagenes');
    var ContenedorRespuestas = $('#contenedorRespuestas');
    $.ajax({
        type: 'POST',
        url: '/VidSigner/ListaRespuestas',
        data: { oid: oidExploracion },
        beforeSend: function () {

            $(".spiner-cargando").removeClass('hide');
            ContenedorRespuestas.html('');
            $('#modal-form-Respuestas').modal('show');
        },
        success: function (data) {
            $(".spiner-cargando").addClass('hide');
            ContenedorRespuestas.html(data);
        }
    });
});

$(document).on('click', '#ColorPicker li a', function () {
    var cidSelected = $(this).data('cid');
    var color = $(this).css('background-color');
    $('#ColorPickerParent').removeClass('open');
    //primero cambiamos el color de la fila activa
    var filaActiva = $('#ExploracionesTable tbody tr.ACTIVA');

    if (filaActiva.data('oid') > 0) {
        filaActiva.find('.cid .fa-circle').css('color', color);

        var request = $.ajax({
            url: "/Exploracion/CambiarColor",
            data: "cid=" + cidSelected + "&" + "oid=" + filaActiva.data('oid'),
            type: "POST"
        });


        request.done(function (data) {

            toastr.success('Color asignado a la exploracion', '', { timeOut: 5000 });
            $("#EnviarFiltros").trigger("click");

        });
        request.fail(function (jqXHR, textStatus) {
            toastr.success('Error al asignar el color a la exploracion', '', { timeOut: 5000 });


        });
    }


    return false;
});

$(document).on("click", "#ImprimirImagen", function myfunction() {

    $(".imagenExploracion").print({
        globalStyles: true,
        mediaPrint: false,
        stylesheet: null,
        noPrintSelector: ".no-print",
        iframe: true,
        append: null,
        prepend: null,
        manuallyCopyFormValues: true,
        deferred: $.Deferred(),
        timeout: 250,
        title: null,
        doctype: '<!doctype html>'
    });
    return false;
});

$(document).ready(function () {
    $("li[data-view]").removeClass('active');
    $("li[data-view=ViewListaDia]").addClass('active');
    $("[data-view=ViewListaDia]").parents("ul").removeClass("collapse");


    var tid = setInterval(actualizaEspera, 18000);
    if ($('#FILTROS_BUSQUEDATOTAL').is(":checked")) {
        activarTypeAhead();
    }

    $(".wrapper-content").css("padding-top", "0")

    $("#EnviarFiltros").trigger("click");
});

$(document).on("click", "#visorJpeg", function () {
    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
    var oidExploracion = filaActiva.data('oid');

    getVisorQReport(oidExploracion, 1);
});

$(document).on("click", "#visorDicom", function () {
    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
    var oidExploracion = filaActiva.data('oid');

    getVisorQReport(oidExploracion, 2);
});

$(document).on("click", "#descargarVisor", function () {
    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
    var oidExploracion = filaActiva.data('oid');

    getVisorQReport(oidExploracion, 3);
});

function getVisorQReport(oidExploracion, tipo) {
    var url = "/Informe/VisorQReport?oid=" + oidExploracion + "&tipo=" + tipo;
    $.ajax({
        url: url,
        type: 'GET',
        success: function (data) {
            if (data.success) {

                switch (tipo) {
                    case 1:
                        window.open(data.message);
                        break;
                    case 2:
                        window.open(data.message);
                        break;
                    case 3:
                        window.location.assign(data.message);
                        break;
                }

            }
            else {
                if (data.message != "") {
                    toastr.error(data.message, 'Visor QReport', { timeOut: 5000 });
                }
                else {
                    toastr.error('Imposible abrir el visor de QReport', 'Visor QReport', { timeOut: 5000 });
                }
            }
        },
        error: function (x, y, z) {
            toastr.error('Imposible abrir el visor de QReport', 'Visor QReport', { timeOut: 5000 });
        }
    });
}








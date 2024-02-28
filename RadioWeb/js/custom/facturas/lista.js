function numeroFacturaFormat(value, row) {

    return row.NUM_FAC + '/' + moment(row.FECHA_FAC).year() + '-' + row.COD1;
}

function tipoFacturaFormat(value, row) {

    if (row.IOR_GPR === 1) {
        return 'PRIVADO';
    } else {
        return 'MUTUA';
    }
   
}

function bloqueadaFormat(value, row) {
    if (value === 'T') {
        return '<i style="color: green;" title="Bloqueada" class="fa fa-lock"></i>';
    } else {
        return '<i style="color: red;" title="No Bloqueada" class="fa fa-unlock"></i>';

    }

}
function sumFormatter(data) {
    field = this.field;
    var total_sum = data.reduce(function (sum, row) {
        return (sum) + (row[field] || 0);
    }, 0);
    return '<b>' + (total_sum * 1).toFixed(2) +
        ' <i class="fa fa-euro"></i></b>';
 
}
function footerStyle(value, row, index) {
    return {
        css: { "class": "bg-info" }
    };
}

function totalTextFormatter(data) {
    return 'Total';
}
function cantidadFormat(value, row) {

    var numb = value;
    if (numb) {
        return (numb * 1).toFixed(2);
        //if (row.SIMBOLO === "€") {
        //    return (numb * 1).toFixed(2) +
        //        ' <i class="fa fa-euro"></i>';
        //} else {
        //    return row.SIMBOLO +  (numb * 1).toFixed(2);
        //}
         
              
        
    }
}





$(document).ready(function () {


    $("li[data-view]").removeClass('active');
    $("[data-view=ViewFacturas]").addClass("active");
    $("[data-view=ViewFacturas]").parents("ul").removeClass("collapse");


    makeBootstrapTable("tblFacturas");


});
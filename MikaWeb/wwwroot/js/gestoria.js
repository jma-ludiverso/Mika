$(document).ready(function () {
    $.fn.dataTable.moment("DD/MM/YYYY HH:mm:ss");
    $.fn.dataTable.moment("DD/MM/YYYY");

    $("#tbListado1").DataTable({
        // Design Assets
        stateSave: true,
        autoWidth: true,
        // ServerSide Setups
        processing: true,
        serverSide: true,
        // Paging Setups
        paging: false,
        // Searching Setups
        searching: false,
        //searching: { regex: true },
        // Ajax Filter
        ajax: {
            url: "/Gestoria/LoadFichas",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                let additionalValues = [];
                additionalValues[0] = 0;
                additionalValues[1] = $("#hdSalon").val();
                additionalValues[2] = $("#hdAnio").val();
                additionalValues[3] = $("#hdMes").val();
                d.AdditionalValues = additionalValues;
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "nFicha" },
            { data: "fecha" },
            { data: "formaPago" },
            { data: "base" },
            { data: "descuentoImp" },
            { data: "iva" },
            { data: "total" }
        ],
        // Column Definitions
        columnDefs: [
            //{
            //    targets: 0,
            //    render: $.fn.dataTable.render.linkFichaExc()
            //},
            {
                targets: 1,
                render: $.fn.dataTable.render.fecha()
            },
            {
                targets: 2,
                render: $.fn.dataTable.render.formaPago()
            },
            {
                targets: [3, 4, 5, 6],
                render: $.fn.dataTable.render.numero()
            },
            { targets: "no-sort", orderable: false },
            { targets: "no-search", searchable: false },
            {
                targets: "trim",
                render: function (data, type, full, meta) {
                    if (type === "display") {
                        data = strtrunc(data, 10);
                    }

                    return data;
                }
            },
            { targets: "date-type", type: "date-eu" }
        ],
        language: {
            url: "//cdn.datatables.net/plug-ins/1.10.21/i18n/Spanish.json"
        }
    });

    $("#tbListado2").DataTable({
        // Design Assets
        stateSave: true,
        autoWidth: false,
        // ServerSide Setups
        processing: true,
        serverSide: true,
        // Paging Setups
        paging: false,
        // Searching Setups
        searching: false,
        //searching: { regex: true },
        // Ajax Filter
        ajax: {
            url: "/Gestoria/LoadFichas",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                let additionalValues = [];
                additionalValues[0] = 1;
                additionalValues[1] = $("#hdSalon").val();
                additionalValues[2] = $("#hdAnio").val();
                additionalValues[3] = $("#hdMes").val();
                d.AdditionalValues = additionalValues;
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "nFicha" },
            { data: "fecha" },
            { data: "formaPago" },
            { data: "base" },
            { data: "descuentoImp" },
            { data: "iva" },
            { data: "total" }
        ],
        // Column Definitions
        columnDefs: [
            //{
            //    targets: 0,
            //    render: $.fn.dataTable.render.linkFichaInc()
            //},
            {
                targets: 1,
                render: $.fn.dataTable.render.fecha()
            },
            {
                targets: 2,
                render: $.fn.dataTable.render.formaPago()
            },
            {
                targets: [3, 4, 5, 6],
                render: $.fn.dataTable.render.numero()
            },
            { targets: "no-sort", orderable: false },
            { targets: "no-search", searchable: false },
            {
                targets: "trim",
                render: function (data, type, full, meta) {
                    if (type === "display") {
                        data = strtrunc(data, 10);
                    }

                    return data;
                }
            },
            { targets: "date-type", type: "date-eu" }
        ],
        language: {
            url: "//cdn.datatables.net/plug-ins/1.10.21/i18n/Spanish.json"
        }
    });

});

function strtrunc(str, num) {
    if (str.length > num) {
        return str.slice(0, num) + "...";
    }
    else {
        return str;
    }
}

jQuery.fn.dataTable.render.fecha = function () {
    return function (d, type, row) {
        var iData = moment(d).format('DD/MM/YYYY');
        return '<div style="text-align: right;">' + iData + '</div>';
    };
};

jQuery.fn.dataTable.render.formaPago = function () {
    return function (d, type, row) {
        var s = "";
        if (d.toString() == "Efectivo") {
            s = '<div style="text-align: center;"><i class="fas fa-coins" title="Efectivo"></i></div>';
        }
        else {
            s = '<div style="text-align: center;"><i class="fas fa-credit-card" title="Tarjeta"></i></div>';
        }
        return s;
    };
};

//jQuery.fn.dataTable.render.linkFichaInc = function () {
//    return function (d, type, row) {
//        return '<span class="badge badge-warning">' + d.toString() + '</span>&nbsp;' +
//            '<span class="badge badge-success"><a href="#" class="incluir" data-nficha="' + d.toString() + '">Incluir</a></span>';
//    };
//};

//jQuery.fn.dataTable.render.linkFichaExc = function () {
//    return function (d, type, row) {
//        var s = '<span class="badge badge-warning">' + d.toString() + '</span>&nbsp;';
//        if (row.formaPago == "Efectivo") {
//            s += '<span class="badge badge-danger"><a href="#" class="excluir" data-nficha="' + d.toString() + '">Excluir</a></span>';
//        }
//        return s;
//    };
//};


jQuery.fn.dataTable.render.numero = function () {
    return function (d, type, row) {
        var iData = $.fn.dataTable.render.number('.', ',', 2).display(d);
        return '<div style="text-align: right;">' + iData + ' €</div>';
    };
};
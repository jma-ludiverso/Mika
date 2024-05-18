$(document).ready(function () {
    $.fn.dataTable.moment("DD/MM/YYYY HH:mm:ss");
    $.fn.dataTable.moment("DD/MM/YYYY");

    $("#tbFichasAll").DataTable({
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
            url: "/Cajas/LoadFichas",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                let additionalValues = [];
                additionalValues[0] = $("#NCaja").val();
                additionalValues[1] = 0;
                d.AdditionalValues = additionalValues;
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "nFicha" },
            { data: "cliente" },
            { data: "formaPago" },
            { data: "base" },
            { data: "descuentoImp" },
            { data: "iva" },
            { data: "total" }
        ],
        // Column Definitions
        columnDefs: [
            {
                targets: 0,
                render: $.fn.dataTable.render.linkFicha()
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

    $("#tbFichasE").DataTable({
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
            url: "/Cajas/LoadFichas",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                let additionalValues = [];
                additionalValues[0] = $("#NCaja").val();
                additionalValues[1] = 1;
                d.AdditionalValues = additionalValues;
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "nFicha" },
            { data: "cliente" },
            { data: "base" },
            { data: "descuentoImp" },
            { data: "iva" },
            { data: "total" }
        ],
        // Column Definitions
        columnDefs: [
            {
                targets: 0,
                render: $.fn.dataTable.render.linkFicha()
            },
            {
                targets: [2, 3, 4, 5],
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

    $("#tbFichasT").DataTable({
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
            url: "/Cajas/LoadFichas",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                let additionalValues = [];
                additionalValues[0] = $("#NCaja").val();
                additionalValues[1] = 2;
                d.AdditionalValues = additionalValues;
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "nFicha" },
            { data: "cliente" },
            { data: "base" },
            { data: "descuentoImp" },
            { data: "iva" },
            { data: "total" }
        ],
        // Column Definitions
        columnDefs: [
            {
                targets: 0,
                render: $.fn.dataTable.render.linkFicha()
            },
            {
                targets: [2, 3, 4, 5],
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

jQuery.fn.dataTable.render.linkFicha = function () {
    return function (d, type, row) {
        var s = '<span class="badge badge-warning"><a href="../Fichas/Ficha?numficha=' + d.toString() + '">' + d.toString() + '</a></span>&nbsp;';
        if (row.anulable != "N/A") {
            s += '<span class="badge badge-danger"><a href="#" class="anular" data-id="' + d.toString() + '" data-toggle="modal" data-target="#borrafichaModal">Anular</a></span>&nbsp;';
        }            
        return s;            
    };
};

jQuery.fn.dataTable.render.numero = function () {
    return function (d, type, row) {
        var iData = $.fn.dataTable.render.number('.', ',', 2).display(d);
        return '<div style="text-align: right;">' + iData + ' €</div>';
    };
};
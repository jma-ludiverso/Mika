$(document).ready(function () {
    $.fn.dataTable.moment("DD/MM/YYYY HH:mm:ss");
    $.fn.dataTable.moment("DD/MM/YYYY");

    $("#tbGastos").DataTable({
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
            url: "/Cajas/LoadGastos",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                let additionalValues = [];
                additionalValues[0] = $("#NCaja").val();
                d.AdditionalValues = additionalValues;
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "linea" },
            { data: "concepto" },
            { data: "importe" },
            { data: "iva" }
        ],
        // Column Definitions
        columnDefs: [
            {
                targets: 0,
                render: $.fn.dataTable.render.linkGasto()
            },
            {
                targets: [2, 3],
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

jQuery.fn.dataTable.render.linkGasto = function () {
    return function (d, type, row) {
        //return '<span class="badge badge-warning"><a href="#" onclick="nuevaLinea(' + d.toString() + ')" data-nlinea="' + d.toString() + '">Gasto # ' + d.toString() + '</a></span>&nbsp;' +
        //    '<span class="badge badge-danger"><a href="#" onclick="borraLinea(' + d.toString() + ')" data-toggle="modal" data-target="#borralineaModal">Eliminar</a></span>';
        return '<span class="badge badge-warning"><a href="#" class="nuevalinea" data-id="' + d.toString() + '">Gasto # ' + d.toString() + '</a></span>&nbsp;' +
            '<span class="badge badge-danger"><a href="#" class="borralinea" data-id="' + d.toString() + '" data-toggle="modal" data-target="#borralineaModal">Eliminar</a></span>';
    };
};

jQuery.fn.dataTable.render.numero = function () {
    return function (d, type, row) {
        var iData = $.fn.dataTable.render.number('.', ',', 2).display(d);
        return '<div style="text-align: right;">' + iData + ' €</div>';
    };
};

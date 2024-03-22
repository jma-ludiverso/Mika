$(document).ready(function () {
    $.fn.dataTable.moment("DD/MM/YYYY HH:mm:ss");
    $.fn.dataTable.moment("DD/MM/YYYY");

    $("#tbProduccion").DataTable({
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
        info: false,
        ordering: false,
        // Ajax Filter
        ajax: {
            url: "/Gestoria/GetProduccion",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                let additionalValues = [];
                additionalValues[0] = $("#hdSalon").val();
                additionalValues[1] = $("#anio").val();
                additionalValues[2] = $("#hdMes").val();
                d.AdditionalValues = additionalValues;
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "empleado" },
            { data: "nServiciosL" },
            { data: "prodServiciosL" },
            { data: "comisionServiciosL" },
            { data: "nServiciosR" },
            { data: "prodServiciosR" },
            { data: "comisionServiciosR" },
            { data: "nServiciosT" },
            { data: "prodServiciosT" },
            { data: "comisionServiciosT" },
            { data: "nProductos" },
            { data: "prodProductos" },
            { data: "comisionProductos" },
            { data: "totalProduccion" },
            { data: "totalComisiones"}
        ],
        // Column Definitions
        columnDefs: [
            {
                targets: [2, 3, 5, 6, 8, 9, 11, 12, 13],
                render: $.fn.dataTable.render.numero()
            },
            {
                targets: 14,
                render: $.fn.dataTable.render.linkImprimir()
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

jQuery.fn.dataTable.render.linkImprimir = function () {
    return function (d, type, row) {
        var iData = $.fn.dataTable.render.number('.', ',', 2).display(d);
        var s = '<div style="text-align: right;display: block ruby;">' + iData + '&nbsp;&nbsp;' +
            '<span class="badge badge-warning"><a href="./Gestoria/PrintDet?anio=' + $("#anio").val() + '&mes=' + $("#hdMes").val() + '&salon=' + $("#hdSalon").val() + '&codigo=' + row.codigo + 'S" target="_blank">Imprimir</a></span>' +
            '<span class="badge badge-warning"><a href="./Gestoria/PrintDet?anio=' + $("#anio").val() + '&mes=' + $("#hdMes").val() + '&salon=' + $("#hdSalon").val() + '&codigo=' + row.codigo + 'D" target="_blank">Detalle</a></span></div > ';
        return s;
    };
};

jQuery.fn.dataTable.render.numero = function () {
    return function (d, type, row) {
        var iData = $.fn.dataTable.render.number('.', ',', 2).display(d);
        return '<div style="text-align: right;">' + iData + '</div>';
    };
};

$(document).ready(function () {
    $.fn.dataTable.moment("DD/MM/YYYY HH:mm:ss");
    $.fn.dataTable.moment("DD/MM/YYYY");

    $("#tbLineas").DataTable({
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
            url: "/Fichas/GetLineas",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                let additionalValues = [];
                additionalValues[0] = $("input[name=NFicha]").val();
                d.AdditionalValues = additionalValues;
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "linea" },
            { data: "empleado" },
            { data: "tipo" },
            { data: "descripcion" },
            { data: "base" },
            { data: "descuentoPorc" },
            { data: "ivaPorc" },
            { data: "total" }
        ],
        // Column Definitions
        columnDefs: [
            {
                targets: 0,
                render: $.fn.dataTable.render.linkFila()
            },
            {
                targets: 2,
                render: $.fn.dataTable.render.tipoServicio()
            },
            {
                targets: [4, 6, 7],
                render: $.fn.dataTable.render.numero()
            },
            {
                targets: 5,
                render: $.fn.dataTable.render.porcentaje()
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

jQuery.fn.dataTable.render.linkFila = function () {
    return function (d, type, row) {
        var estado = $("#hdEstado").val();
        if (estado == "True") {
            var ret = '<span class="badge badge-warning">Modificar</span>&nbsp;' +
                '<span class="badge badge-danger">Eliminar</span>';
        } else {
            var ret = '<span class="badge badge-warning"><a href="#" class="nuevalinea" data-id="' + d.toString() + '" data-nlinea="' + d.toString() + '">Modificar</a></span>&nbsp;' +
                '<span class="badge badge-danger"><a href="#" class="borralinea" data-id="' + d.toString() + '" data-toggle="modal" data-target="#borralineaModal">Eliminar</a></span>';
        }
        return ret;
    };
};

jQuery.fn.dataTable.render.numero = function () {
    return function (d, type, row) {
        var iData = $.fn.dataTable.render.number('.', ',', 2).display(d);
        return '<div style="text-align: right;">' + iData + ' €</div>';
    };
};

jQuery.fn.dataTable.render.porcentaje = function () {
    return function (d, type, row) {
        var iData = $.fn.dataTable.render.number('.', ',', 2).display(d);
        return '<div style="text-align: right;">' + iData + ' %</div>';
    };
};

jQuery.fn.dataTable.render.tipoServicio = function () {
    return function (d, type, row) {
        var s = "";
        if (d.toString() == "Producto") {
            s = '<div style="text-align: center;"><i class="fab fa-product-hunt" title="Venta producto"></i></div>';
        }
        else {
            s = '<div style="text-align: center;"><i class="fas fa-cut" title="Servicio"></i></div>';
        }
        return s;
    };
};

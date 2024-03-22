$(document).ready(function () {
    $.fn.dataTable.moment("DD/MM/YYYY HH:mm:ss");
    $.fn.dataTable.moment("DD/MM/YYYY");

    $("#tbServicios").DataTable({
        // Design Assets
        stateSave: true,
        autoWidth: true,
        // ServerSide Setups
        processing: true,
        serverSide: true,
        // Paging Setups
        paging: true,
        // Searching Setups
        searching: { regex: true },
        // Ajax Filter
        ajax: {
            url: "/Servicios/LoadTable",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                let additionalValues = [];
                additionalValues[0] = $("input[name=SoloActivos]:checked").val();
                d.AdditionalValues = additionalValues;
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "idServicio" },
            { data: "codigo"},
            { data: "tipo" },
            { data: "grupo"}, 
            { data: "nombre" },
            { data: "precio" },
            { data: "ivaPorc" },
            { data: "ivaCant" },
            { data: "pvp"},
            { data: "activo" }
        ],
        // Column Definitions
        columnDefs: [
            {
                targets: 0,
                render: $.fn.dataTable.render.linkServicio()
            },
            {
                targets: 2,
                render: $.fn.dataTable.render.tipoServicio()
            },
            {
                targets: [5, 6, 7, 8],
                render: $.fn.dataTable.render.numero()
            },
            {
                targets: 9,
                render: $.fn.dataTable.render.switch()
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

jQuery.fn.dataTable.render.linkServicio = function () {
    return function (d, type, row) {
        if (row.tipo.toString() != "Producto") {
            return '<span class="badge badge-warning"><a href="./Servicios/Edit/' + d.toString() + '">Modificar</a></span>';
        } else {
            return '---<input type="hidden" value="' + d.toString() + '"/>';
        } 
    };
};

jQuery.fn.dataTable.render.numero = function () {
    return function (d, type, row) {
        var iData = $.fn.dataTable.render.number('.',',',2).display(d); 
        return '<div style="text-align: right;">' + iData + ' €</div>';
    };
};

jQuery.fn.dataTable.render.tipoServicio = function () {
    return function (d, type, row) {
        var s = "";
        if (d.toString() == "Producto") {
            s = '<div style="text-align: center;"><i class="fab fa-product-hunt" title="Venta producto"></i></div>';
        }
        else
        {
            s = '<div style="text-align: center;"><i class="fas fa-cut" title="Servicio"></i></div>';
        }
        return s;
    };
};

jQuery.fn.dataTable.render.switch = function () {
    return function (d, type, row) {
        var s = "";
        var b = JSON.parse(d.toString());
        if (b == true) {
            s = '<div style="text-align: center;"><i class="fa fa-check-circle" aria-hidden="true"></i></div>';
        }
        else
        {
            s = '';
        }
        return s;
    };
};

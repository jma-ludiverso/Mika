package com.example.mikaapp;

public class DBStructure {

    private DBStructure(){}

    public static final String TABLE_USUARIOS = "AspNetUsers";
    public static final String TABLE_CLIENTES = "Clientes";
    public static final String TABLE_EMPLEADOSCOMISIONES = "EmpleadosComisiones";
    public static final String TABLE_EMPLEADOSSERVICIOS = "EmpleadosServicios";
    public static final String TABLE_EMPRESAS = "Empresas";
    public static final String TABLE_SALONES = "Salones";
    public static final String TABLE_SERVICIOS = "Servicios";
    public static final String TABLE_CLIENTESHISTORIAL = "Clientes_Historial";
    public static final String TABLE_FICHAS = "Fichas";
    public static final String TABLE_FICHASLINEAS = "Fichas_Lineas";

    public static final String USUARIOS_ID = "Id";
    public static final String USUARIOS_USERNAME = "UserName";
    public static final String USUARIOS_EMAIL = "Email";
    public static final String USUARIOS_SECURITYSTAMP = "SecurityStamp";
    public static final String USUARIOS_PHONENUMBER = "PhoneNumber";
    public static final String USUARIOS_ACTIVO = "Activo";
    public static final String USUARIOS_APELLIDOS = "Apellidos";
    public static final String USUARIOS_NOMBRE = "Nombre";
    public static final String USUARIOS_ISADMIN = "IsAdmin";
    public static final String USUARIOS_CODIGO = "Codigo";
    public static final String USUARIOS_SALON = "Salon";
    public static final String USUARIOS_SECURITYEXPIRATION = "SecurityExpiration";


    public static final String CLIENTES_ID = "idCliente";
    public static final String CLIENTES_SALON = "idSalon";
    public static final String CLIENTES_NOMBRE = "Nombre";
    public static final String CLIENTES_TELEFONO = "Telefono";
    public static final String CLIENTES_EMAIL = "Email";
    public static final String CLIENTES_NUEVO = "Nuevo";



    public static final String EMPLEADOSCOMISIONES_CODIGO = "Codigo";
    public static final String EMPLEADOSCOMISIONES_SALON = "Salon";
    public static final String EMPLEADOSCOMISIONES_PRODUCTOS_E1 = "ProductosE1";
    public static final String EMPLEADOSCOMISIONES_PRODUCTOS_E2 = "ProductosE2";
    public static final String EMPLEADOSCOMISIONES_PRODUCTOS_E3 = "ProductosE3";
    public static final String EMPLEADOSCOMISIONES_PRODUCTOS_E4 = "ProductosE4";
    public static final String EMPLEADOSCOMISIONES_PRODUCTOS_P1 = "ProductosP1";
    public static final String EMPLEADOSCOMISIONES_PRODUCTOS_P2 = "ProductosP2";
    public static final String EMPLEADOSCOMISIONES_PRODUCTOS_P3 = "ProductosP3";
    public static final String EMPLEADOSCOMISIONES_PRODUCTOS_P4 = "ProductosP4";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_LE1 = "ServiciosLE1";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_LE2 = "ServiciosLE2";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_LE3 = "ServiciosLE3";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_LE4 = "ServiciosLE4";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_LP1 = "ServiciosLP1";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_LP2 = "ServiciosLP2";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_LP3 = "ServiciosLP3";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_LP4 = "ServiciosLP4";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_SE1 = "ServiciosSE1";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_SE2 = "ServiciosSE2";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_SE3 = "ServiciosSE3";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_SE4 = "ServiciosSE4";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_SP1 = "ServiciosSP1";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_SP2 = "ServiciosSP2";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_SP3 = "ServiciosSP3";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_SP4 = "ServiciosSP4";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_TE1 = "ServiciosTE1";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_TE2 = "ServiciosTE2";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_TE3 = "ServiciosTE3";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_TE4 = "ServiciosTE4";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_TP1 = "ServiciosTP1";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_TP2 = "ServiciosTP2";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_TP3 = "ServiciosTP3";
    public static final String EMPLEADOSCOMISIONES_SERVICIOS_TP4 = "ServiciosTP4";


    public static final String EMPLEADOSSERVICIOS_CODIGO = "Codigo";
    public static final String EMPLEADOSSERVICIOS_ID_SERVICIO = "IdServicio";
    public static final String EMPLEADOSSERVICIOS_COMISION_P1 = "ComisionP1";
    public static final String EMPLEADOSSERVICIOS_COMISION_P2 = "ComisionP2";
    public static final String EMPLEADOSSERVICIOS_COMISION_P3 = "ComisionP3";
    public static final String EMPLEADOSSERVICIOS_COMISION_P4 = "ComisionP4";


    public static final String EMPRESAS_IDEMPRESA = "IdEmpresa";
    public static final String EMPRESAS_NOMBRE = "Nombre";
    public static final String EMPRESAS_CIF = "CIF";
    public static final String EMPRESAS_DIRECCION = "Direccion";
    public static final String EMPRESAS_CP = "CP";
    public static final String EMPRESAS_CIUDAD = "Ciudad";
    public static final String EMPRESAS_TELEFONO = "Telefono";
    public static final String EMPRESAS_EMAIL = "Email";


    public static final String SALONES_ID = "IdSalon";
    public static final String SALONES_EMPRESA = "IdEmpresa";
    public static final String SALONES_NOMBRE = "Nombre";
    public static final String SALONES_DIRECCION = "Direccion";
    public static final String SALONES_TELEFONO = "Telefono";

    public static final String SERVICIOS_ID = "IdServicio";
    public static final String SERVICIOS_EMPRESA = "IdEmpresa";
    public static final String SERVICIOS_CODIGO = "Codigo";
    public static final String SERVICIOS_TIPO = "Tipo";
    public static final String SERVICIOS_GRUPO = "Grupo";
    public static final String SERVICIOS_NOMBRE = "Nombre";
    public static final String SERVICIOS_PRECIO = "Precio";
    public static final String SERVICIOS_IVA_PORC = "IvaPorc";
    public static final String SERVICIOS_IVA_CANT = "IvaCant";
    public static final String SERVICIOS_PVP = "PVP";
    public static final String SERVICIOS_ACTIVO = "Activo";


    public static final String CLIENTESHISTORIA_IDCLIENTE = "IdCliente";
    public static final String CLIENTESHISTORIA_IDHISTORIA = "IdHistoria";
    public static final String CLIENTESHISTORIA_FECHA = "Fecha";
    public static final String CLIENTESHISTORIA_DESCRIPCION = "Descripcion";
    public static final String CLIENTESHISTORIA_NUEVA = "Nueva";

    public static final String FICHAS_NFICHA = "NFicha";
    public static final String FICHAS_FECHA = "Fecha";
    public static final String FICHAS_ANIO = "Anio";
    public static final String FICHAS_MES = "Mes";
    public static final String FICHAS_NUMERO = "Numero";
    public static final String FICHAS_IDSALON = "IdSalon";
    public static final String FICHAS_IDCLIENTE = "IdCliente";
    public static final String FICHAS_FORMAPAGO = "FormaPago";
    public static final String FICHAS_BASE = "Base";
    public static final String FICHAS_DESCUENTOPORC = "DescuentoPorc";
    public static final String FICHAS_DESCUENTOIMP = "DescuentoImp";
    public static final String FICHAS_DESCUENTOS = "Descuentos";
    public static final String FICHAS_ILVA = "Iva";
    public static final String FICHAS_TOTAL = "Total";
    public static final String FICHAS_PAGADO = "Pagado";
    public static final String FICHAS_CAMBIO = "Cambio";
    public static final String FICHAS_CERRADA = "Cerrada";

    public static final String FICHASLINEAS_NFICHA = "NFicha";
    //public static final String FICHASLINEAS_NOMBREEMPLEADO = "NombreEmpleado";
    public static final String FICHASLINEAS_IDSALON = "IdSalon";
    public static final String FICHASLINEAS_LINEA = "Linea";
    public static final String FICHASLINEAS_CODIGO = "Codigo";
    public static final String FICHASLINEAS_IDSERCIO = "IdServicio";
    public static final String FICHASLINEAS_DESCRIPCION = "Descripcion";
    public static final String FICHASLINEAS_BASE = "Base";
    public static final String FICHASLINEAS_DESCUENTOPORC = "DescuentoPorc";
    public static final String FICHASLINEAS_DESCUENTOCANT = "DescuentoCant";
    public static final String FICHASLINEAS_IVAPORC = "IvaPorc";
    public static final String FICHASLINEAS_IVACANT = "IvaCant";
    public static final String FICHASLINEAS_TOTAL = "Total";
    public static final String FICHASLINEAS_COMISIONP1 = "ComisionP1";
    public static final String FICHASLINEAS_COMISIONP2 = "ComisionP2";
    public static final String FICHASLINEAS_COMISIONP3 = "ComisionP3";
    public static final String FICHASLINEAS_COMISIONP4 = "ComisionP4";
}
